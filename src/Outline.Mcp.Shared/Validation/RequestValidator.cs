using Outline.Mcp.Shared.Exceptions;
using Outline.Mcp.Shared.Models;

namespace Outline.Mcp.Shared.Validation;

/// <summary>
/// Validates requests before sending to Outline API
/// </summary>
public static class RequestValidator
{
    public static void ValidateCreateCollection(CreateCollectionRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate name
        if (!OutlineConstants.Names.IsValid(request.Name))
        {
            errors["name"] = new[] { 
                $"Name is required and must be between {OutlineConstants.Names.MinLength} and {OutlineConstants.Names.MaxLength} characters" 
            };
        }

        // Validate permission
        if (!OutlineConstants.Permissions.IsValid(request.Permission))
        {
            errors["permission"] = new[] { 
                $"Permission must be one of: {string.Join(", ", OutlineConstants.Permissions.All)}" 
            };
        }

        // Validate color
        if (!OutlineConstants.Colors.IsValid(request.Color))
        {
            errors["color"] = new[] { "Color must be in hex format: #RRGGBB (e.g., #FF5733)" };
        }

        // Validate icon
        if (!OutlineConstants.Icons.IsValid(request.Icon))
        {
            errors["icon"] = new[] { OutlineConstants.Icons.GetRecommendation() ?? "Invalid icon" };
        }

        if (errors.Any())
        {
            throw new OutlineValidationException("Validation failed for CreateCollectionRequest", errors);
        }
    }

    public static void ValidateCreateDocument(CreateDocumentRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate title
        if (!OutlineConstants.Names.IsValid(request.Title))
        {
            errors["title"] = new[] { 
                $"Title is required and must be between {OutlineConstants.Names.MinLength} and {OutlineConstants.Names.MaxLength} characters" 
            };
        }

        // Validate text
        if (!OutlineConstants.Content.IsValid(request.Text))
        {
            errors["text"] = new[] { 
                $"Text content exceeds maximum length of {OutlineConstants.Content.MaxLength} characters" 
            };
        }

        // Validate collection ID
        if (string.IsNullOrWhiteSpace(request.CollectionId))
        {
            errors["collectionId"] = new[] { "Collection ID is required" };
        }

        // Validate color
        if (!OutlineConstants.Colors.IsValid(request.Color))
        {
            errors["color"] = new[] { "Color must be in hex format: #RRGGBB (e.g., #FF5733)" };
        }

        // Validate icon
        if (!OutlineConstants.Icons.IsValid(request.Icon))
        {
            errors["icon"] = new[] { OutlineConstants.Icons.GetRecommendation() ?? "Invalid icon" };
        }

        if (errors.Any())
        {
            throw new OutlineValidationException("Validation failed for CreateDocumentRequest", errors);
        }
    }

    public static void ValidateUpdateDocument(string documentId, UpdateDocumentRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate document ID
        if (string.IsNullOrWhiteSpace(documentId))
        {
            errors["documentId"] = new[] { "Document ID is required" };
        }

        // Validate title if provided
        if (request.Title != null && !OutlineConstants.Names.IsValid(request.Title))
        {
            errors["title"] = new[] { 
                $"Title must be between {OutlineConstants.Names.MinLength} and {OutlineConstants.Names.MaxLength} characters" 
            };
        }

        // Validate text if provided
        if (request.Text != null && !OutlineConstants.Content.IsValid(request.Text))
        {
            errors["text"] = new[] { 
                $"Text content exceeds maximum length of {OutlineConstants.Content.MaxLength} characters" 
            };
        }

        // Validate color
        if (!OutlineConstants.Colors.IsValid(request.Color))
        {
            errors["color"] = new[] { "Color must be in hex format: #RRGGBB (e.g., #FF5733)" };
        }

        // Validate icon
        if (!OutlineConstants.Icons.IsValid(request.Icon))
        {
            errors["icon"] = new[] { OutlineConstants.Icons.GetRecommendation() ?? "Invalid icon" };
        }

        if (errors.Any())
        {
            throw new OutlineValidationException("Validation failed for UpdateDocumentRequest", errors);
        }
    }

    public static void ValidateSearch(SearchDocumentsRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate query
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            errors["query"] = new[] { "Search query is required" };
        }

        // Normalize limit
        if (request.Limit != null)
        {
            request.Limit = OutlineConstants.Search.NormalizeLimit(request.Limit);
        }

        if (errors.Any())
        {
            throw new OutlineValidationException("Validation failed for SearchDocumentsRequest", errors);
        }
    }
}
