using Outline.Mcp.Shared.Api;
using Spectre.Console;

namespace Outline.Mcp.Client.Commands;

public static class GetDocumentCommand
{
    public static async Task ExecuteAsync(IOutlineApiClient client, string documentId)
    {
        try
        {
            var document = await AnsiConsole.Status()
                .StartAsync($"Fetching document {documentId}...", async ctx =>
                {
                    return await client.GetDocumentAsync(documentId);
                });

            var panel = new Panel($"""
                [bold]Title:[/] {document.Title}
                [bold]ID:[/] {document.Id}
                [bold]Collection:[/] {document.CollectionId}
                [bold]Created:[/] {document.CreatedAt:yyyy-MM-dd HH:mm}
                [bold]Updated:[/] {document.UpdatedAt:yyyy-MM-dd HH:mm}
                [bold]Revision:[/] {document.Revision}
                [bold]Icon:[/] {document.Icon ?? "-"}
                """)
            {
                Header = new PanelHeader("Document Info"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Content:[/]");
            AnsiConsole.WriteLine(document.Text.Length > 500 
                ? document.Text.Substring(0, 500) + "..." 
                : document.Text);

            AnsiConsole.MarkupLine($"\n[green]Document retrieved successfully[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }
}
