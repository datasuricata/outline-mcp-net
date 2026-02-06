using System.Text.Json.Serialization;

namespace Outline.Mcp.Shared.Models;

public class OutlineRevision
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("createdBy")]
    public OutlineUser? CreatedBy { get; set; }

    [JsonPropertyName("collectionId")]
    public string? CollectionId { get; set; }
}
