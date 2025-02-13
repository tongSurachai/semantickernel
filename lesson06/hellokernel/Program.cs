using Microsoft.SemanticKernel;
using OllamaSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

// Add Semantic Kernel as a singleton
builder.Services.AddSingleton(sp => 
    Kernel.CreateBuilder()
        .AddOllamaChatCompletion(new OllamaApiClient("http://localhost:11434", "llama2:latest"))
        .Build());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(b => b
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Models endpoint for OpenWebUI compatibility
app.MapGet("/v1/models", () => new
{
    data = new[]
    {
        new
        {
            id = "llama2",
            obj = "model",
            created = 1677610602,
            owned_by = "ollama"
        }
    }
});

// Chat completions endpoint
app.MapPost("/v1/chat/completions", async (ChatRequest request, Kernel kernel) =>
{
    var messages = request.Messages.Select(m => m.Content).ToList();
    var lastMessage = messages.LastOrDefault() ?? "";

    var functionResult = await kernel.InvokePromptAsync(lastMessage);
    var response = functionResult.ToString();

    return new ChatResponse
    {
        Id = "chatcmpl-" + Guid.NewGuid().ToString("N"),
        Obj = "chat.completion",
        Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Model = "llama2",
        Choices = new[]
        {
            new Choice
            {
                Index = 0,
                Message = new Message
                {
                    Role = "assistant",
                    Content = response
                },
                FinishReason = "stop"
            }
        },
        Usage = new Usage
        {
            PromptTokens = 0,
            CompletionTokens = 0,
            TotalTokens = 0
        }
    };
});

app.Run();

public class ChatRequest
{
    [JsonPropertyName("messages")]
    public Message[] Messages { get; set; } = Array.Empty<Message>();

    [JsonPropertyName("model")]
    public string Model { get; set; } = "";
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public class ChatResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("object")]
    public string Obj { get; set; } = "";

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("choices")]
    public Choice[] Choices { get; set; } = Array.Empty<Choice>();

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = new();
}

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = "";
}

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
