using Microsoft.SemanticKernel;
using OllamaSharp;

// Create a kernel with an Ollama Client
var kernel = Kernel.CreateBuilder().AddOllamaChatCompletion(
    new OllamaApiClient("http://localhost:11434","llama3.2:latest")
).Build();

// Send a prompt
var response = await kernel.InvokePromptAsync("Hello World");

// Output the response
Console.WriteLine(response);
