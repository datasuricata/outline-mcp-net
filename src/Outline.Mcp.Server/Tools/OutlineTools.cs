using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Outline.Mcp.Shared.Api;
using Outline.Mcp.Shared.Exceptions;

namespace Outline.Mcp.Server.Tools;

[McpServerToolType]
public static class OutlineTools
{
    private static string HandleException(Exception ex)
    {
        object error = ex switch
        {
            OutlineValidationException validationEx => new
            {
                success = false,
                error = "validation_error",
                message = validationEx.Message,
                details = validationEx.ValidationErrors
            },
            OutlineAuthenticationException authEx => new
            {
                success = false,
                error = "authentication_error",
                message = authEx.Message,
                statusCode = authEx.StatusCode,
                hint = "Check if your OUTLINE_API_KEY is correct and has the necessary permissions"
            },
            OutlineNotFoundException notFoundEx => new
            {
                success = false,
                error = "not_found",
                message = notFoundEx.Message,
                statusCode = notFoundEx.StatusCode
            },
            OutlineRateLimitException rateLimitEx => new
            {
                success = false,
                error = "rate_limit_exceeded",
                message = rateLimitEx.Message,
                retryAfter = rateLimitEx.RetryAfterSeconds,
                hint = "Please wait before making more requests"
            },
            OutlineApiException apiEx => new
            {
                success = false,
                error = apiEx.ErrorCode ?? "api_error",
                message = apiEx.Message,
                statusCode = apiEx.StatusCode
            },
            _ => new
            {
                success = false,
                error = "unexpected_error",
                message = ex.Message
            }
        };

        return JsonSerializer.Serialize(error, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("List all available collections in Outline. Returns a list of collections with their IDs, names, descriptions, and metadata.")]
    public static async Task<string> ListCollections(IOutlineApiClient outlineClient)
    {
        var collections = await outlineClient.ListCollectionsAsync();
        
        var result = collections.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            description = c.Description,
            icon = c.Icon,
            color = c.Color,
            permission = c.Permission,
            createdAt = c.CreatedAt,
            updatedAt = c.UpdatedAt
        }).ToList();

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Create a new collection in Outline. Collections are used to organize documents into groups with specific permissions.")]
    public static async Task<string> CreateCollection(
        IOutlineApiClient outlineClient,
        [Description("Collection name")] string name,
        [Description("Optional description for the collection")] string? description = null,
        [Description("Optional icon for the collection (use emoji or null; named icons are not supported)")] string? icon = null,
        [Description("Optional color for the collection in hex format (e.g., #4E5C6E)")] string? color = null,
        [Description("Permission level: 'read' or 'read_write' (default: read_write)")] string? permission = "read_write")
    {
        try
        {
            var request = new Shared.Models.CreateCollectionRequest
            {
                Name = name,
                Description = description,
                Icon = icon,
                Color = color,
                Permission = permission ?? "read_write"
            };

            var collection = await outlineClient.CreateCollectionAsync(request);

            var output = new
            {
                success = true,
                id = collection.Id,
                name = collection.Name,
                description = collection.Description,
                icon = collection.Icon,
                color = collection.Color,
                permission = collection.Permission,
                createdAt = collection.CreatedAt,
                url = $"{Environment.GetEnvironmentVariable("OUTLINE_BASE_URL")}/collection/{collection.Id}"
            };

            return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [McpServerTool, Description("Search for documents in Outline by query. Returns matching documents with context snippets, rankings, and metadata.")]
    public static async Task<string> SearchDocuments(
        IOutlineApiClient outlineClient,
        [Description("Search query string")] string query,
        [Description("Optional collection ID to limit search scope")] string? collectionId = null,
        [Description("Include archived documents in results")] bool includeArchived = false,
        [Description("Include draft documents in results")] bool includeDrafts = true,
        [Description("Maximum number of results to return")] int limit = 25)
    {
        var results = await outlineClient.SearchDocumentsAsync(
            query,
            collectionId,
            includeArchived,
            includeDrafts,
            limit);

        var output = results.Select(r => new
        {
            ranking = r.Ranking,
            context = r.Context,
            document = new
            {
                id = r.Document.Id,
                title = r.Document.Title,
                urlId = r.Document.UrlId,
                collectionId = r.Document.CollectionId,
                createdAt = r.Document.CreatedAt,
                updatedAt = r.Document.UpdatedAt
            }
        }).ToList();

        return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Retrieve a specific document from Outline by its ID. Returns the full document content, metadata, and associated information.")]
    public static async Task<string> GetDocument(
        IOutlineApiClient outlineClient,
        [Description("The ID of the document to retrieve")] string documentId)
    {
        var document = await outlineClient.GetDocumentAsync(documentId);

        var output = new
        {
            id = document.Id,
            title = document.Title,
            text = document.Text,
            urlId = document.UrlId,
            collectionId = document.CollectionId,
            parentDocumentId = document.ParentDocumentId,
            publishedAt = document.PublishedAt,
            createdAt = document.CreatedAt,
            updatedAt = document.UpdatedAt,
            createdBy = document.CreatedBy?.Name,
            updatedBy = document.UpdatedBy?.Name,
            icon = document.Icon,
            emoji = document.Emoji,
            color = document.Color,
            template = document.Template,
            fullWidth = document.FullWidth,
            revision = document.Revision
        };

        return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Create a new document in Outline. Supports Markdown content including Mermaid diagrams. Returns the created document with its ID and metadata.")]
    public static async Task<string> CreateDocument(
        IOutlineApiClient outlineClient,
        [Description("Document title")] string title,
        [Description("Document content in Markdown format (supports Mermaid diagrams with ```mermaid code blocks)")] string text,
        [Description("Collection ID where the document will be created")] string collectionId,
        [Description("Optional parent document ID to create a nested document")] string? parentDocumentId = null,
        [Description("Whether to publish the document immediately")] bool publish = true,
        [Description("Optional icon for the document (use emoji or null; named icons are not supported)")] string? icon = null,
        [Description("Optional color for the document")] string? color = null,
        [Description("Whether to display the document in full width")] bool fullWidth = false)
    {
        try
        {
            var request = new Shared.Models.CreateDocumentRequest
            {
                Title = title,
                Text = text,
                CollectionId = collectionId,
                ParentDocumentId = parentDocumentId,
                Publish = publish,
                Icon = icon,
                Color = color,
                FullWidth = fullWidth
            };

            var document = await outlineClient.CreateDocumentAsync(request);

            var output = new
            {
                success = true,
                id = document.Id,
                title = document.Title,
                urlId = document.UrlId,
                collectionId = document.CollectionId,
                parentDocumentId = document.ParentDocumentId,
                publishedAt = document.PublishedAt,
                createdAt = document.CreatedAt,
                icon = document.Icon,
                color = document.Color,
                fullWidth = document.FullWidth,
                url = $"{Environment.GetEnvironmentVariable("OUTLINE_BASE_URL")}/doc/{document.UrlId}"
            };

            return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [McpServerTool, Description("Update an existing document in Outline. Can update title, content, publish status, and styling. Supports Markdown with Mermaid diagrams.")]
    public static async Task<string> UpdateDocument(
        IOutlineApiClient outlineClient,
        [Description("The ID of the document to update")] string documentId,
        [Description("New document title")] string? title = null,
        [Description("New document content in Markdown format (supports Mermaid diagrams)")] string? text = null,
        [Description("If true, append text to existing content instead of replacing")] bool append = false,
        [Description("Whether to publish the document")] bool? publish = null,
        [Description("Optional icon for the document (use emoji or null)")] string? icon = null,
        [Description("Optional color for the document")] string? color = null,
        [Description("Whether to display the document in full width")] bool? fullWidth = null)
    {
        try
        {
            var request = new Shared.Models.UpdateDocumentRequest
            {
                Title = title,
                Text = text,
                Append = append,
                Publish = publish,
                Icon = icon,
                Color = color,
                FullWidth = fullWidth
            };

            var document = await outlineClient.UpdateDocumentAsync(documentId, request);

            var output = new
            {
                success = true,
                id = document.Id,
                title = document.Title,
                urlId = document.UrlId,
                collectionId = document.CollectionId,
                publishedAt = document.PublishedAt,
                updatedAt = document.UpdatedAt,
                icon = document.Icon,
                color = document.Color,
                fullWidth = document.FullWidth,
                revision = document.Revision,
                url = $"{Environment.GetEnvironmentVariable("OUTLINE_BASE_URL")}/doc/{document.UrlId}"
            };

            return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [McpServerTool, Description("Delete a document from Outline. Can permanently delete or move to trash. Use with caution as permanent deletion cannot be undone.")]
    public static async Task<string> DeleteDocument(
        IOutlineApiClient outlineClient,
        [Description("The ID of the document to delete")] string documentId,
        [Description("If true, permanently delete the document. If false, move to trash")] bool permanent = false)
    {
        var success = await outlineClient.DeleteDocumentAsync(documentId, permanent);

        var output = new
        {
            success = success,
            documentId = documentId,
            permanent = permanent,
            message = permanent 
                ? "Document permanently deleted" 
                : "Document moved to trash"
        };

        return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("List all revisions for a specific document. Returns revision history with timestamps, authors, and revision IDs. Essential for tracking document changes and enabling rollback.")]
    public static async Task<string> ListRevisions(
        IOutlineApiClient outlineClient,
        [Description("The ID of the document to retrieve revisions for")] string documentId)
    {
        var revisions = await outlineClient.ListRevisionsAsync(documentId);

        var output = revisions.Select(r => new
        {
            id = r.Id,
            documentId = r.DocumentId,
            title = r.Title,
            createdAt = r.CreatedAt,
            createdBy = r.CreatedBy?.Name,
            createdByEmail = r.CreatedBy?.Email,
            collectionId = r.CollectionId,
            preview = r.Text?.Length > 100 ? r.Text.Substring(0, 100) + "..." : r.Text
        }).ToList();

        var result = new
        {
            documentId = documentId,
            totalRevisions = output.Count,
            revisions = output
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Retrieve detailed information about a specific revision. Returns the full content and metadata of a document at a specific point in time.")]
    public static async Task<string> GetRevision(
        IOutlineApiClient outlineClient,
        [Description("The ID of the revision to retrieve")] string revisionId)
    {
        var revision = await outlineClient.GetRevisionAsync(revisionId);

        var output = new
        {
            id = revision.Id,
            documentId = revision.DocumentId,
            title = revision.Title,
            text = revision.Text,
            createdAt = revision.CreatedAt,
            createdBy = new
            {
                id = revision.CreatedBy?.Id,
                name = revision.CreatedBy?.Name,
                email = revision.CreatedBy?.Email
            },
            collectionId = revision.CollectionId
        };

        return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Restore a document to a previous revision. Creates a new revision with the content from the specified revision. Non-destructive operation - creates new version rather than deleting history. Uses documents.restore endpoint with revisionId parameter.")]
    public static async Task<string> RestoreRevision(
        IOutlineApiClient outlineClient,
        [Description("The ID of the document to restore")] string documentId,
        [Description("The ID of the revision to restore to")] string revisionId,
        [Description("Optional collection ID if the document should be moved to a different collection")] string? collectionId = null)
    {
        var document = await outlineClient.RestoreDocumentAsync(documentId, revisionId, collectionId);

        var output = new
        {
            success = true,
            documentId = documentId,
            revisionId = revisionId,
            document = new
            {
                id = document.Id,
                title = document.Title,
                urlId = document.UrlId,
                collectionId = document.CollectionId,
                updatedAt = document.UpdatedAt,
                revision = document.Revision,
                url = $"{Environment.GetEnvironmentVariable("OUTLINE_BASE_URL")}/doc/{document.UrlId}"
            },
            message = $"Document successfully restored to revision {revisionId}"
        };

        return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
    }
}
