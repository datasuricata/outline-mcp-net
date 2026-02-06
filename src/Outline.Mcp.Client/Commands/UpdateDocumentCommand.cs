using Outline.Mcp.Shared.Api;
using Outline.Mcp.Shared.Models;
using Spectre.Console;

namespace Outline.Mcp.Client.Commands;

public static class UpdateDocumentCommand
{
    public static async Task ExecuteAsync(
        IOutlineApiClient client, 
        string documentId, 
        string? title, 
        string? text)
    {
        try
        {
            var request = new UpdateDocumentRequest
            {
                Title = title,
                Text = text
            };

            var document = await AnsiConsole.Status()
                .StartAsync($"Updating document {documentId}...", async ctx =>
                {
                    return await client.UpdateDocumentAsync(documentId, request);
                });

            var panel = new Panel($"""
                [bold]Title:[/] {document.Title}
                [bold]ID:[/] {document.Id}
                [bold]Revision:[/] {document.Revision}
                [bold]Updated:[/] {document.UpdatedAt:yyyy-MM-dd HH:mm}
                """)
            {
                Header = new PanelHeader("Document Updated"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.MarkupLine($"\n[green]Document updated successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }
}
