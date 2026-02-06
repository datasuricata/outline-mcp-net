using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Outline.Mcp.Shared.Models;
using Outline.Mcp.Shared.Exceptions;
using Outline.Mcp.Shared.Validation;

namespace Outline.Mcp.Shared.Api;

public class OutlineApiClient : IOutlineApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly JsonSerializerOptions _jsonOptions;

    public OutlineApiClient(string baseUrl, string apiKey)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _apiKey = apiKey;
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public OutlineApiClient(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _baseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
        _apiKey = apiKey;
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<List<OutlineCollection>> ListCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<CollectionsData>("/api/collections.list", new { }, cancellationToken);
        return response.GetCollections();
    }

    public async Task<OutlineCollection> CreateCollectionAsync(
        CreateCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate request before sending
        RequestValidator.ValidateCreateCollection(request);

        var response = await PostAsync<CollectionData>("/api/collections.create", request, cancellationToken);
        return response.Data ?? throw new OutlineApiException(
            "Failed to create collection: API returned null data", 
            500, 
            "null_response");
    }

    public async Task<List<SearchResult>> SearchDocumentsAsync(
        string query,
        string? collectionId = null,
        bool includeArchived = false,
        bool includeDrafts = true,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        var request = new SearchDocumentsRequest
        {
            Query = query,
            CollectionId = collectionId,
            IncludeArchived = includeArchived,
            IncludeDrafts = includeDrafts,
            Limit = limit
        };

        // Validate and normalize request
        RequestValidator.ValidateSearch(request);

        var response = await PostAsync<SearchResultsData>("/api/documents.search", request, cancellationToken);
        return response.Data;
    }

    public async Task<OutlineDocument> GetDocumentAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new OutlineValidationException("Document ID is required");
        }

        var request = new { id = documentId };
        var response = await PostAsync<DocumentData>("/api/documents.info", request, cancellationToken);
        
        return response.Data ?? response.Document ?? throw new OutlineNotFoundException("Document", documentId);
    }

    public async Task<OutlineDocument> CreateDocumentAsync(
        CreateDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate request before sending
        RequestValidator.ValidateCreateDocument(request);

        var response = await PostAsync<DocumentData>("/api/documents.create", request, cancellationToken);
        return response.Data ?? response.Document ?? throw new OutlineApiException(
            "Failed to create document: API returned null data", 
            500, 
            "null_response");
    }

    public async Task<OutlineDocument> UpdateDocumentAsync(
        string documentId,
        UpdateDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate request before sending
        RequestValidator.ValidateUpdateDocument(documentId, request);

        var updateRequest = new
        {
            id = documentId,
            title = request.Title,
            text = request.Text,
            append = request.Append,
            publish = request.Publish,
            done = request.Done,
            icon = request.Icon,
            color = request.Color,
            emoji = request.Emoji,
            fullWidth = request.FullWidth
        };

        var response = await PostAsync<DocumentData>("/api/documents.update", updateRequest, cancellationToken);
        return response.Data ?? response.Document ?? throw new OutlineApiException(
            $"Failed to update document {documentId}: API returned null data", 
            500, 
            "null_response");
    }

    public async Task<bool> DeleteDocumentAsync(
        string documentId,
        bool permanent = false,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            id = documentId,
            permanent = permanent
        };

        try
        {
            await PostAsync<object>("/api/documents.delete", request, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<OutlineRevision>> ListRevisionsAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var request = new { documentId = documentId };
        var response = await PostAsync<RevisionsData>("/api/revisions.list", request, cancellationToken);
        return response.Data.Count > 0 ? response.Data : response.Revisions;
    }

    public async Task<OutlineRevision> GetRevisionAsync(
        string revisionId,
        CancellationToken cancellationToken = default)
    {
        var request = new { id = revisionId };
        var response = await PostAsync<RevisionData>("/api/revisions.info", request, cancellationToken);
        return response.Data ?? response.Revision ?? throw new Exception($"Revision {revisionId} not found");
    }

    public async Task<OutlineDocument> RestoreDocumentAsync(
        string documentId,
        string revisionId,
        string? collectionId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            id = documentId,
            revisionId = revisionId,
            collectionId = collectionId
        };
        
        var response = await PostAsync<DocumentData>("/api/documents.restore", request, cancellationToken);
        return response.Data ?? response.Document ?? throw new Exception($"Failed to restore document {documentId} to revision {revisionId}");
    }

    private async Task<T> PostAsync<T>(string endpoint, object body, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"[DEBUG] Outline API Request: {endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ERROR] Outline API Response ({response.StatusCode}): {responseContent}");
                
                // Try to parse error response
                var errorInfo = TryParseErrorResponse(responseContent);
                
                // Handle specific HTTP status codes
                throw (int)response.StatusCode switch
                {
                    401 => new OutlineAuthenticationException(
                        errorInfo.Message ?? "Authentication failed. Check your API key.", 
                        responseContent),
                    
                    404 => new OutlineNotFoundException(
                        "Resource", 
                        endpoint, 
                        responseContent),
                    
                    429 => new OutlineRateLimitException(
                        errorInfo.Message ?? "Rate limit exceeded. Please try again later.",
                        errorInfo.RetryAfter),
                    
                    400 => new OutlineValidationException(
                        errorInfo.Message ?? "Invalid request data"),
                    
                    _ => new OutlineApiException(
                        errorInfo.Message ?? $"API request failed with status {response.StatusCode}",
                        (int)response.StatusCode,
                        errorInfo.ErrorCode,
                        responseContent)
                };
            }

            Console.WriteLine($"[DEBUG] Outline API Response: Success");

            var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            
            return result ?? throw new OutlineApiException(
                "Failed to deserialize API response", 
                500, 
                "deserialization_failed", 
                responseContent);
        }
        catch (OutlineException)
        {
            // Re-throw our custom exceptions
            throw;
        }
        catch (TaskCanceledException ex)
        {
            throw new OutlineApiException(
                "Request timed out. The server took too long to respond.",
                408,
                "timeout",
                ex.Message);
        }
        catch (HttpRequestException ex)
        {
            throw new OutlineApiException(
                $"Network error: {ex.Message}",
                0,
                "network_error",
                ex.ToString());
        }
        catch (Exception ex)
        {
            throw new OutlineApiException(
                $"Unexpected error: {ex.Message}",
                500,
                "unexpected_error",
                ex.ToString());
        }
    }

    private (string? Message, string? ErrorCode, int? RetryAfter) TryParseErrorResponse(string responseContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var message = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : null;
            var errorCode = root.TryGetProperty("error", out var errProp) ? errProp.GetString() : null;
            
            int? retryAfter = null;
            if (root.TryGetProperty("retryAfter", out var retryProp) && retryProp.TryGetInt32(out var retry))
            {
                retryAfter = retry;
            }

            return (message, errorCode, retryAfter);
        }
        catch
        {
            return (null, null, null);
        }
    }
}
