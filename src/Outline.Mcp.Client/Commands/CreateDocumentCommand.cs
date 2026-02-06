using Outline.Mcp.Shared.Api;
using Outline.Mcp.Shared.Models;
using Spectre.Console;

namespace Outline.Mcp.Client.Commands;

public static class CreateDocumentCommand
{
    public static async Task ExecuteAsync(
        IOutlineApiClient client, 
        string title, 
        string text, 
        string collectionId, 
        bool publish, 
        string? icon)
    {
        try
        {
            var request = new CreateDocumentRequest
            {
                Title = title,
                Text = text,
                CollectionId = collectionId,
                Publish = publish,
                Icon = icon
            };

            var document = await AnsiConsole.Status()
                .StartAsync("Creating document...", async ctx =>
                {
                    return await client.CreateDocumentAsync(request);
                });

            var panel = new Panel($"""
                [bold]Title:[/] {document.Title}
                [bold]ID:[/] {document.Id}
                [bold]URL ID:[/] {document.UrlId}
                [bold]Collection:[/] {document.CollectionId}
                [bold]Published:[/] {(document.PublishedAt.HasValue ? "Yes" : "No")}
                [bold]Created:[/] {document.CreatedAt:yyyy-MM-dd HH:mm}
                """)
            {
                Header = new PanelHeader("Document Created"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.MarkupLine($"\n[green]Document created successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }
}
