using Microsoft.Playwright;
using Microsoft.SemanticKernel;
using OllamaSharp;
using System.ComponentModel;


// Create a kernel with an Ollama Client
var builder = Kernel.CreateBuilder()
    .AddOllamaChatCompletion(
        new OllamaApiClient("http://localhost:11434", "llama3.2:latest")
    );

//builder.Services.AddLogging(configure => configure.AddConsole());


// Add function by type
builder.Plugins.AddFromType<BrowserPlugin>();
    
// Create the kernel
var kernel = builder.Build();

// Define function calling behaviour. Sends all funcs to kernel to pick approppriate 
// plugin and invoke automatically
var settings = new PromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

 
//// Invoke the prompt

while (true) // Infinite loop to keep the interaction running
{
    Console.WriteLine("How can I help you today? ");
    var userInput = Console.ReadLine(); // Get user input

    if (string.IsNullOrWhiteSpace(userInput)) continue; // Ignore empty input
    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase)) break; // Allow exit

    Console.WriteLine("Thinking...");

    //var response = await kernel.InvokePromptAsync(userInput, new(settings));
    var response = await kernel.InvokePromptAsync(userInput);
    Console.WriteLine("🤖 " + response);
}


public class BrowserPlugin
{
    private readonly IBrowser _browser;

    public BrowserPlugin()
    {

        // Initialize Playwright and launch the browser
        var playwright = Playwright.CreateAsync().Result;
        
        _browser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Run in headless mode
        }).Result;
    }

    [KernelFunction, Description("Navigates to the specified URL and captures a screenshot.")]
    public async Task<string> NavigateAndCaptureAsync(string url)
    {
        try
        {
            // Ensure the URL is well-formed
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                url = "https://" + url.Trim();
            }

            var page = await _browser.NewPageAsync();
            Console.WriteLine($"Navigating to {url}");

            await page.GotoAsync(url);
            var screenshotPath = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            Console.WriteLine($"Capturing screenshot to {screenshotPath}");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
            await page.CloseAsync();

            var response = $"Screenshot successfully captured: {screenshotPath}";
            Console.WriteLine("Generated response:" + response);

            return response;
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error navigating to {url}: {ex.Message}";
            Console.WriteLine("Generated error response: " + errorMessage);
            return errorMessage; // Ensures the LLM gets a meaningful error instead of a misleading response
        }
    }


    public async Task CloseAsync()
    {
        await _browser.CloseAsync();
    }
}
