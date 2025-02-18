using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using OllamaSharp;
using OllamaSharp.Models;

#pragma warning disable SKEXP0070

public class KernelService
{
    private readonly Kernel _kernel;
    private readonly OllamaApiClient _ollamaClient;
    public KernelService()
    {
        _ollamaClient = new OllamaApiClient("http://localhost:11434", "llama2:latest");
            
        _kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion(new OllamaApiClient("http://localhost:11434", "llama2:latest"))
            .Build();
    }

    public async Task<string> GetCompletionResponseAsync(string prompt)
    {
        var result = await _kernel.InvokePromptAsync(prompt);
        return result.ToString();
    }
    
    public async IAsyncEnumerable<string> GetCompletionResponseStreamAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new GenerateRequest
        {
            Prompt = prompt,
            Model = _ollamaClient.SelectedModel
        };
        
        // Pass the GenerateRequest object to GenerateAsync
        await foreach (var responseSegment in _ollamaClient.GenerateAsync(request, cancellationToken))
        {
            if (responseSegment?.Response is not null)
            {
                yield return responseSegment.Response;
            }
            else
            {
                yield return string.Empty; 
            }
        }
    }    
}