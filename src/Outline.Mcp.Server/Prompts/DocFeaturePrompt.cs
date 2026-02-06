using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Outline.Mcp.Server.Helpers;

namespace Outline.Mcp.Server.Prompts;

public static partial class OutlinePrompts
{
    [McpServerPrompt, Description("Document feature with use cases, technical implementation and tests")]
    public static GetPromptResult DocFeature(
        [Description("Feature name")] string featureName,
        [Description("Collection ID (optional)")] string? collectionId = null,
        [Description("Parent document ID (optional)")] string? parentDocumentId = null)
    {
        var localTemplate = EmbeddedResourceHelper.ReadEmbeddedResource("feature-documentation.md");

        var promptContent = $@"Technical writer documenting feature '{featureName}'.

## CRITICAL Guardrails
⚠️ DO NOT create generic/placeholder examples
⚠️ MUST read actual implementation code
⚠️ Use real file names, function names, API endpoints
⚠️ If code not found, ask user for location - don't invent

## Steps
1. If collection not provided: list_collections → ask user which to use
2. search_documents to maintain consistency with existing docs
3. **Search for feature code:** Find files related to '{featureName}' in workspace
4. **Read implementation:** Read actual source files that implement this feature
5. Ask user: description, problem solved, modified components, APIs, how to test
6. Analyze actual code/tests → create_document with REAL implementation details (parentDocumentId: {parentDocumentId ?? "null"})
{GetTemplateSearchInstruction("FeatureDocumentation", localTemplate)}

Professional language, examples from actual code, Mermaid for real flows, document how and why with specifics.";

        return new GetPromptResult
        {
            Description = $"Document feature '{featureName}' with use cases, technical implementation and test guides",
            Messages = new[]
            {
                OutlinePrompts.CreatePromptMessage(Role.User, promptContent)
            }
        };
    }
}
