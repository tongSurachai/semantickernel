using System.Text.Json.Serialization;

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