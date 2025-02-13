using Microsoft.SemanticKernel;
using OllamaSharp;

// Create a kernel with an Ollama Client
var kernel = Kernel.CreateBuilder().AddOllamaChatCompletion(
    new OllamaApiClient("http://localhost:11434","llama3.2:latest")
).Build();

await KernelWithoutHistory(kernel);
return;

async static Task KernelWithoutHistory(Kernel kernel)
{
    while (true)
    {
        // Say hello
        Console.WriteLine();
        Console.WriteLine("Hi, I'm an AI, how can I help you ?");
        Console.WriteLine("-----------------------------------");

        // Read input from console
        var prompt = Console.ReadLine();

        // Send the prompt to the kernel if it's not null
        if (!string.IsNullOrEmpty(prompt))
        {
            var response = await kernel.InvokePromptAsync(prompt);
            // Print the response
            Console.WriteLine(response);
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("Prompt cannot be empty. Please enter a valid prompt.");
        }
    }   
}



