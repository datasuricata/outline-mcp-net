using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Outline.Mcp.Server.Prompts;

/// <summary>
/// Base class for Outline MCP prompts that guide AI agents in creating professional documentation
/// </summary>
[McpServerPromptType]
public static partial class OutlinePrompts
{
    /// <summary>
    /// Helper method to format prompt messages
    /// </summary>
    public static PromptMessage CreatePromptMessage(Role role, string content)
    {
        return new PromptMessage
        {
            Role = role,
            Content = new TextContentBlock { Text = content }
        };
    }

    /// <summary>
    /// Get template instruction for agent to search in Outline first, fallback to provided template
    /// </summary>
    public static string GetTemplateSearchInstruction(string templateName, string localTemplate)
    {
        return $@"
## Template Source Priority
1. FIRST: search_documents ""{templateName}"" in collection ""Outline Skills"" → if found, get_document to retrieve customized template
2. FALLBACK: Use embedded template below

## Embedded Template (Fallback)
{localTemplate}";
    }
}
