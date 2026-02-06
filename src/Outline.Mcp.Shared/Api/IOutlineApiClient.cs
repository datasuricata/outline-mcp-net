using Outline.Mcp.Shared.Models;

namespace Outline.Mcp.Shared.Api;

public interface IOutlineApiClient
{
    Task<List<OutlineCollection>> ListCollectionsAsync(CancellationToken cancellationToken = default);
    
    Task<OutlineCollection> CreateCollectionAsync(
        CreateCollectionRequest request,
        CancellationToken cancellationToken = default);
    
    Task<List<SearchResult>> SearchDocumentsAsync(
        string query,
        string? collectionId = null,
        bool includeArchived = false,
        bool includeDrafts = true,
        int limit = 25,
        CancellationToken cancellationToken = default);
    
    Task<OutlineDocument> GetDocumentAsync(
        string documentId,
        CancellationToken cancellationToken = default);
    
    Task<OutlineDocument> CreateDocumentAsync(
        CreateDocumentRequest request,
        CancellationToken cancellationToken = default);
    
    Task<OutlineDocument> UpdateDocumentAsync(
        string documentId,
        UpdateDocumentRequest request,
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteDocumentAsync(
        string documentId,
        bool permanent = false,
        CancellationToken cancellationToken = default);
    
    Task<List<OutlineRevision>> ListRevisionsAsync(
        string documentId,
        CancellationToken cancellationToken = default);
    
    Task<OutlineRevision> GetRevisionAsync(
        string revisionId,
        CancellationToken cancellationToken = default);
    
    Task<OutlineDocument> RestoreDocumentAsync(
        string documentId,
        string revisionId,
        string? collectionId = null,
        CancellationToken cancellationToken = default);
}
