using Microsoft.SemanticKernel;
using OllamaSharp;

// Create a kernel builder for an Ollama Model
var builder = Kernel.CreateBuilder().AddOllamaChatCompletion(
    new OllamaApiClient("http://localhost:11434","llama3.2:latest")
);

// Add function by type
builder.Plugins.AddFromType<DatePlugin>();

// Create the kernel
var kernel = builder.Build();

// Define function calling behaviour. Sends all funcs to kernel to pick approppriate 
// plugin and invoke automatically
var settings = new PromptExecutionSettings{ 
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// invoke the prompt
var response = await kernel.InvokePromptAsync("What's today's date ?",new(settings));


Console.WriteLine(response);

public class DatePlugin
{
    [KernelFunction("GetCurrentDate")]
    public string GetCurrentDate() 
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
}