using FluentAssertions;
using Outline.Mcp.Shared.Mermaid;
using Xunit;

namespace Outline.Mcp.Tests.Unit;

public class MermaidBuildersTests
{
    [Fact]
    public void FlowchartBuilder_ShouldGenerateValidMermaidSyntax()
    {
        // Arrange
        var builder = new FlowchartBuilder(FlowchartDirection.TopToBottom);

        // Act
        var result = builder
            .AddNode("start", "Início", NodeShape.RoundedRectangle)
            .AddNode("process", "Processar", NodeShape.Rectangle)
            .AddNode("end", "Fim", NodeShape.RoundedRectangle)
            .AddEdge("start", "process")
            .AddEdge("process", "end")
            .Build();

        // Assert
        result.Should().Contain("flowchart TB");
        result.Should().Contain("start");
        result.Should().Contain("process");
        result.Should().Contain("end");
        result.Should().Contain("-->");
    }

    [Fact]
    public void ClassDiagramBuilder_ShouldGenerateValidClassDiagram()
    {
        // Arrange
        var builder = new ClassDiagramBuilder();

        // Act
        var result = builder
            .AddClass("OutlineApiClient", 
                new List<string> { "string _baseUrl", "string _apiKey" },
                new List<string> { "Task<List> ListCollectionsAsync()" })
            .AddClass("IOutlineApiClient",
                null,
                new List<string> { "Task<List> ListCollectionsAsync()" })
            .AddDependency("OutlineApiClient", "IOutlineApiClient")
            .Build();

        // Assert
        result.Should().Contain("classDiagram");
        result.Should().Contain("class OutlineApiClient");
        result.Should().Contain("class IOutlineApiClient");
        result.Should().Contain("..>");
    }

    [Fact]
    public void SequenceDiagramBuilder_ShouldGenerateValidSequenceDiagram()
    {
        // Arrange
        var builder = new SequenceDiagramBuilder();

        // Act
        var result = builder
            .AddParticipant("client", "Client")
            .AddParticipant("server", "Server")
            .AddMessage("client", "server", "Request")
            .AddReturn("server", "client", "Response")
            .Build();

        // Assert
        result.Should().Contain("sequenceDiagram");
        result.Should().Contain("participant client");
        result.Should().Contain("participant server");
        result.Should().Contain("->>");
    }

    [Fact]
    public void EntityRelationshipBuilder_ShouldGenerateValidERDiagram()
    {
        // Arrange
        var builder = new EntityRelationshipBuilder();

        // Act
        var result = builder
            .AddEntity("Collection", new Dictionary<string, string> 
            { 
                { "uuid", "id" },
                { "string", "name" }
            })
            .AddEntity("Document", new Dictionary<string, string>
            {
                { "uuid", "id" },
                { "string", "title" },
                { "fk_uuid", "collectionId" }
            })
            .AddRelationship("Collection", "Document", Cardinality.ExactlyOne, Cardinality.ZeroOrMany, "contains")
            .Build();

        // Assert
        result.Should().Contain("erDiagram");
        result.Should().Contain("Collection");
        result.Should().Contain("Document");
        result.Should().Contain("--");
    }

    [Fact]
    public void MermaidHelpers_ShouldWrapInMarkdownCodeBlock()
    {
        // Arrange
        var mermaidCode = "graph TB\n    A --> B";

        // Act
        var result = MermaidHelpers.WrapInMarkdownCodeBlock(mermaidCode);

        // Assert
        result.Should().StartWith("```mermaid");
        result.Should().EndWith("```");
        result.Should().Contain(mermaidCode);
    }

    [Fact]
    public void MermaidHelpers_ShouldSanitizeIds()
    {
        // Arrange & Act
        var result1 = MermaidHelpers.SanitizeId("my-node-id");
        var result2 = MermaidHelpers.SanitizeId("my node id");
        var result3 = MermaidHelpers.SanitizeId("my.node.id");

        // Assert
        result1.Should().Be("my_node_id");
        result2.Should().Be("my_node_id");
        result3.Should().Be("my_node_id");
    }

    [Fact]
    public void MermaidHelpers_ShouldEscapeLabels()
    {
        // Arrange & Act
        var result = MermaidHelpers.EscapeLabel("Label with \"quotes\"");

        // Assert
        result.Should().Contain("\\\"");
    }
}
