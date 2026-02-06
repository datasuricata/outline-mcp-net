using System.Text.Json.Serialization;

namespace Outline.Mcp.Shared.Models;

public class CreateCollectionRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("permission")]
    public string? Permission { get; set; }

    [JsonPropertyName("sharing")]
    public bool? Sharing { get; set; }
}

public class CreateDocumentRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("collectionId")]
    public string CollectionId { get; set; } = string.Empty;

    [JsonPropertyName("parentDocumentId")]
    public string? ParentDocumentId { get; set; }

    [JsonPropertyName("template")]
    public bool? Template { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("publish")]
    public bool? Publish { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("emoji")]
    public string? Emoji { get; set; }

    [JsonPropertyName("fullWidth")]
    public bool? FullWidth { get; set; }
}

public class UpdateDocumentRequest
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("append")]
    public bool? Append { get; set; }

    [JsonPropertyName("publish")]
    public bool? Publish { get; set; }

    [JsonPropertyName("done")]
    public bool? Done { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("emoji")]
    public string? Emoji { get; set; }

    [JsonPropertyName("fullWidth")]
    public bool? FullWidth { get; set; }
}

public class SearchDocumentsRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("collectionId")]
    public string? CollectionId { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("dateFilter")]
    public string? DateFilter { get; set; }

    [JsonPropertyName("includeArchived")]
    public bool? IncludeArchived { get; set; }

    [JsonPropertyName("includeDrafts")]
    public bool? IncludeDrafts { get; set; }

    [JsonPropertyName("snippetMinWords")]
    public int? SnippetMinWords { get; set; }

    [JsonPropertyName("snippetMaxWords")]
    public int? SnippetMaxWords { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}
