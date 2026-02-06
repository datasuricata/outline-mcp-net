using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Outline.Mcp.Server.Helpers;

namespace Outline.Mcp.Server.Prompts;

public static partial class OutlinePrompts
{
    [McpServerPrompt, Description("Document project structure and architecture following professional corporate template")]
    public static GetPromptResult DocProject(
        [Description("Project name")] string projectName,
        [Description("Collection ID (optional)")] string? collectionId = null,
        [Description("Focus areas comma-separated (optional)")] string? focusAreas = null)
    {
        var localTemplate = EmbeddedResourceHelper.ReadEmbeddedResource("project-documentation.md");

        var promptContent = $@"Software architect documenting project '{projectName}'.

## CRITICAL Guardrails
⚠️ DO NOT create generic/example documentation
⚠️ MUST analyze actual workspace files and code
⚠️ Use real project structure, not placeholders
⚠️ If uncertain about something, ask user - don't invent

## Steps
1. If collection not provided: list_collections → ask user which to use
2. **Analyze workspace:** Search for package.json/.csproj/go.mod/etc to understand tech stack
3. Ask: ""Use icons? (yes/no)"" → If yes: BOOK Overview, ARCH Architecture, FOLDER Structure, CONFIG Components, REFRESH Flows, PLUG APIs, ROCKET Deploy, TEST Tests, TOOL Troubleshooting
4. Ask: ""Dedicated page per section? (yes/no)""
   - Yes: create_document parent → create_document children with parentDocumentId
   - No: create_document single
5. Ask: ""What to document?"" (architecture/components/APIs/flows/all)
6. **Read actual files:** Search and read relevant source files based on user choice
7. Analyze code → create doc(s) following template with REAL information
{GetTemplateSearchInstruction("ProjectDocumentation", localTemplate)}

Professional language, practical examples from actual code, Mermaid for real architecture.";

        return new GetPromptResult
        {
            Description = $"Document project '{projectName}' focusing on architecture and technical implementation",
            Messages = new[]
            {
                OutlinePrompts.CreatePromptMessage(Role.User, promptContent)
            }
        };
    }
}
