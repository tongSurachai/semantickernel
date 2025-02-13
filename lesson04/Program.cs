
using Microsoft.SemanticKernel;
using OllamaSharp;
using System.ComponentModel;


// Create kernel builder
var builder = Kernel.CreateBuilder().AddOllamaChatCompletion(
    new OllamaApiClient("http://localhost:11434","llama3.2:latest")
);


// Add function by type
builder.Plugins.AddFromType<DatePlugin>();
builder.Plugins.AddFromType<MangoPlugin>();

// Create the kernel
var kernel = builder.Build();

// Set function calling behaviour to automatically choose the function
var settings = new PromptExecutionSettings{ 
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};


while (true)
{
    Console.WriteLine();
    Console.WriteLine("How can I help ? Enter a prompt: ");
    Console.WriteLine("--------------------------------");

    // Read input from console
    var prompt = Console.ReadLine();

    // Send the prompt to the kernel if it's not null
    if (!string.IsNullOrEmpty(prompt))
    {
        var response = await kernel.InvokePromptAsync(prompt, new(settings));
        // Print the response
        Console.WriteLine(response);
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("Prompt cannot be empty. Please enter a valid prompt.");
    }
}

// Define a MangoPlugin class with a GetMangoPrice function
public class MangoPlugin
{
    [KernelFunction("GetMangoPrice")]
    [Description("Calculates the price of mangoes based on the number of kilograms provided via numberOfKilos parameter. Answer always in United States Dollars and no other currency")]
    public double GetMangoPrice (
        [Description("The weight of mangoes in kilograms (also referred to as kilos).")]
        double numberOfKilos
    ) 
    {
        var pricePerKilo = 4.0;
        return numberOfKilos * pricePerKilo;
    }
}


// Define DatePlugin class with GetCurrentDate function
public class DatePlugin
{
    [KernelFunction("GetCurrentDate")]
    public string GetCurrentDate() 
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
}