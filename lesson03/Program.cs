using System.ComponentModel;
using Microsoft.SemanticKernel;
using OllamaSharp;

var builder = Kernel.CreateBuilder().AddOllamaChatCompletion(
     new OllamaApiClient("http://localhost:11434","llama3.2:latest")
);

builder.Plugins.AddFromType<MangoPlugin>();
var kernel = builder.Build();

var settings = new PromptExecutionSettings() {
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
};

var response = await kernel.InvokePromptAsync("How much for .5 kg of mangoes",new (settings));

Console.WriteLine(response);

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