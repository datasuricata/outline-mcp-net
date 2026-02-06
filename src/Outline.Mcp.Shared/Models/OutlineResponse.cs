using System.Text.Json.Serialization;

namespace Outline.Mcp.Shared.Models;

public class OutlineResponse<T>
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class PaginatedOutlineResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }
}

public class DocumentData
{
    [JsonPropertyName("data")]
    public OutlineDocument? Data { get; set; }
    
    [JsonPropertyName("document")]
    public OutlineDocument? Document { get; set; }
}

public class CollectionData
{
    [JsonPropertyName("data")]
    public OutlineCollection? Data { get; set; }
}

public class CollectionsData
{
    [JsonPropertyName("data")]
    public List<OutlineCollection>? Data { get; set; }
    
    [JsonPropertyName("collections")]
    public List<OutlineCollection>? Collections { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
    
    // Helper method to get collections from either field
    public List<OutlineCollection> GetCollections()
    {
        return Data ?? Collections ?? new List<OutlineCollection>();
    }
}

public class SearchResult
{
    [JsonPropertyName("ranking")]
    public double Ranking { get; set; }

    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    [JsonPropertyName("document")]
    public OutlineDocument Document { get; set; } = new();
}

public class SearchResultsData
{
    [JsonPropertyName("data")]
    public List<SearchResult> Data { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationInfo Pagination { get; set; } = new();
}

public class RevisionData
{
    [JsonPropertyName("data")]
    public OutlineRevision? Data { get; set; }
    
    [JsonPropertyName("revision")]
    public OutlineRevision? Revision { get; set; }
}

public class RevisionsData
{
    [JsonPropertyName("data")]
    public List<OutlineRevision> Data { get; set; } = new();

    [JsonPropertyName("revisions")]
    public List<OutlineRevision> Revisions { get; set; } = new();

    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}
