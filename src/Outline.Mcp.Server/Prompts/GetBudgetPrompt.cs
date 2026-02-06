using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Outline.Mcp.Server.Helpers;

namespace Outline.Mcp.Server.Prompts;

public static partial class OutlinePrompts
{
    [McpServerPrompt, Description("Create technical-commercial proposal with roadmap, estimates and risk analysis")]
    public static GetPromptResult GetBudget(
        [Description("Client name")] string clientName,
        [Description("Project name")] string projectName,
        [Description("Collection ID (optional)")] string? collectionId = null,
        [Description("Requirements document ID (optional)")] string? requirementsDocId = null)
    {
        var localTemplate = EmbeddedResourceHelper.ReadEmbeddedResource("budget-proposal.md");

        var promptContent = $@"Solutions architect creating proposal for '{clientName}' - Project '{projectName}'.

## CRITICAL Guardrails
⚠️ DO NOT use generic estimates without analysis
⚠️ MUST base estimates on actual requirements or detailed questions
⚠️ If workspace has related project, analyze its complexity for reference
⚠️ Clearly mark assumptions vs confirmed requirements

## Steps
1. If collection not provided: list_collections → ask user which to use
2. If requirementsDocId '{requirementsDocId ?? "not provided"}': get_document → analyze requirements in detail
3. Otherwise: ask SPECIFIC questions:
   - Business objectives and success metrics
   - Priority features with acceptance criteria
   - Non-functional requirements (performance, scalability, security)
   - Tech stack preferences and constraints
   - Team size and skill level
   - Timeline and budget constraints
4. **Analyze workspace if available:** Search for similar projects/code to inform estimates
5. Classify complexity (Low/Medium/High) with justification → structure in realistic phases
6. Estimate hours based on actual requirements (not generic ranges)
7. Identify SPECIFIC risks, assumptions, dependencies → create_document
{GetTemplateSearchInstruction("BudgetProposal", localTemplate)}

Consultive tone, realistic estimates with justification, highlight ROI with metrics, Mermaid for actual architecture/roadmap, detailed tables for estimates/risks.";

        return new GetPromptResult
        {
            Description = $"Create technical-commercial proposal for '{clientName}' - Project '{projectName}' with roadmap and estimates",
            Messages = new[]
            {
                OutlinePrompts.CreatePromptMessage(Role.User, promptContent)
            }
        };
    }
}
