using Outline.Mcp.Shared.Api;
using Spectre.Console;

namespace Outline.Mcp.Client.Commands;

public static class ListCollectionsCommand
{
    public static async Task ExecuteAsync(IOutlineApiClient client)
    {
        try
        {
            var collections = await AnsiConsole.Status()
                .StartAsync("Fetching collections...", async ctx =>
                {
                    return await client.ListCollectionsAsync();
                });

            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Description");
            table.AddColumn("Icon");
            table.AddColumn("Permission");

            foreach (var collection in collections)
            {
                table.AddRow(
                    collection.Id,
                    collection.Name,
                    collection.Description ?? "-",
                    collection.Icon ?? "-",
                    collection.Permission ?? "-"
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[green]Found {collections.Count} collection(s)[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }
}
