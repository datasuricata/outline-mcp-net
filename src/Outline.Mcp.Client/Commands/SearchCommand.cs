using Outline.Mcp.Shared.Api;
using Spectre.Console;

namespace Outline.Mcp.Client.Commands;

public static class SearchCommand
{
    public static async Task ExecuteAsync(IOutlineApiClient client, string query, string? collectionId)
    {
        try
        {
            var results = await AnsiConsole.Status()
                .StartAsync($"Searching for '{query}'...", async ctx =>
                {
                    return await client.SearchDocumentsAsync(query, collectionId);
                });

            if (!results.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No documents found[/]");
                return;
            }

            var table = new Table();
            table.AddColumn("Ranking");
            table.AddColumn("Title");
            table.AddColumn("ID");
            table.AddColumn("Context");

            foreach (var result in results.Take(10))
            {
                table.AddRow(
                    result.Ranking.ToString("F2"),
                    result.Document.Title,
                    result.Document.Id,
                    result.Context.Length > 60 ? result.Context.Substring(0, 60) + "..." : result.Context
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[green]Found {results.Count} document(s) (showing top 10)[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }
}
