using System.Text.Json.Serialization;

public class ChatRequest
{
    [JsonPropertyName("messages")]
    public Message[] Messages { get; set; } = Array.Empty<Message>();

    [JsonPropertyName("model")]
    public string Model { get; set; } = "";
}