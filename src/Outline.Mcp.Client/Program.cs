using Outline.Mcp.Client.Commands;
using Outline.Mcp.Shared.Api;
using Spectre.Console;

var baseUrl = Environment.GetEnvironmentVariable("OUTLINE_BASE_URL") 
    ?? "http://localhost:3000";

var apiKey = Environment.GetEnvironmentVariable("OUTLINE_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    AnsiConsole.MarkupLine("[red]Error: OUTLINE_API_KEY environment variable is required[/]");
    return 1;
}

var outlineClient = new OutlineApiClient(baseUrl, apiKey);

if (args.Length == 0)
{
    AnsiConsole.MarkupLine("[yellow]Outline MCP Client - Test CLI[/]");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("Available commands:");
    AnsiConsole.MarkupLine("  list-collections");
    AnsiConsole.MarkupLine("  search --query <query> [--collection-id <id>]");
    AnsiConsole.MarkupLine("  get --id <id>");
    AnsiConsole.MarkupLine("  create --title <title> --text <text> --collection-id <id> [--icon <icon>]");
    AnsiConsole.MarkupLine("  update --id <id> [--title <title>] [--text <text>]");
    AnsiConsole.MarkupLine("  bootstrap [--validate-only]  - Valida instalação e cria collection MCP com documentação");
    return 0;
}

var command = args[0].ToLower();

try
{
    switch (command)
    {
        case "list-collections":
            await ListCollectionsCommand.ExecuteAsync(outlineClient);
            break;

        case "search":
            var query = GetArg(args, "--query");
            var collectionId = GetArgOrNull(args, "--collection-id");
            await SearchCommand.ExecuteAsync(outlineClient, query, collectionId);
            break;

        case "get":
            var docId = GetArg(args, "--id");
            await GetDocumentCommand.ExecuteAsync(outlineClient, docId);
            break;

        case "create":
            var title = GetArg(args, "--title");
            var text = GetArg(args, "--text");
            var createCollectionId = GetArg(args, "--collection-id");
            var icon = GetArgOrNull(args, "--icon");
            await CreateDocumentCommand.ExecuteAsync(outlineClient, title, text, createCollectionId, true, icon);
            break;

        case "update":
            var updateId = GetArg(args, "--id");
            var updateTitle = GetArgOrNull(args, "--title");
            var updateText = GetArgOrNull(args, "--text");
            await UpdateDocumentCommand.ExecuteAsync(outlineClient, updateId, updateTitle, updateText);
            break;

        case "bootstrap":
            var validateOnly = HasFlag(args, "--validate-only");
            await BootstrapCommand.ExecuteAsync(outlineClient, baseUrl, validateOnly);
            break;

        default:
            AnsiConsole.MarkupLine($"[red]Unknown command: {command}[/]");
            return 1;
    }

    return 0;
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
    return 1;
}

static string GetArg(string[] args, string name)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == name)
            return args[i + 1];
    }
    throw new ArgumentException($"Required argument {name} not found");
}

static string? GetArgOrNull(string[] args, string name)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == name)
            return args[i + 1];
    }
    return null;
}

static bool HasFlag(string[] args, string flag)
{
    return args.Contains(flag);
}
