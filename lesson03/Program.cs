using System.ComponentModel;
using Microsoft.SemanticKernel;
using OllamaSharp;

// Setup kernel to use Ollama chat completion
var builder = Kernel.CreateBuilder().AddOllamaChatCompletion(
     new OllamaApiClient("http://localhost:11434","llama3.2:latest")
);

// Add MangoPlugin to the kernel
builder.Plugins.AddFromType<MangoPlugin>();

// Build the kernel
var kernel = builder.Build();

// Configure the kernel to automatically choose the function
var settings = new PromptExecutionSettings() {
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
};

// Send a prompt to the kernel
var response = await kernel.InvokePromptAsync("How much for .5 kg of mangoes",new (settings));

// Print the response
Console.WriteLine(response);


// Define a MangoPlugin class with a GetMangoPrice function
public class MangoPlugin
{
    [KernelFunction("GetMangoPrice")]
    [Description("Calculates the price of mangoes based on weight in kilograms. Returns price in USD.")]
    public double GetMangoPrice (
        [Description("The weight of mangoes in kilograms.")]
        double numberOfKilos
    ) 
    {
        var pricePerKilo = 4.0;
        return numberOfKilos * pricePerKilo;
    }
}