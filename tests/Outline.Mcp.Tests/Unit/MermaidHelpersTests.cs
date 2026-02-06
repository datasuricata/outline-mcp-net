using FluentAssertions;
using Outline.Mcp.Shared.Mermaid;
using Xunit;

namespace Outline.Mcp.Tests.Unit;

public class MermaidHelpersTests
{
    [Theory]
    [InlineData("my-id", "my_id")]
    [InlineData("my id", "my_id")]
    [InlineData("my.id", "my_id")]
    [InlineData("my/id", "my_id")]
    [InlineData("my\\id", "my_id")]
    public void SanitizeId_ShouldReplaceSpecialCharacters(string input, string expected)
    {
        // Act
        var result = MermaidHelpers.SanitizeId(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void EscapeLabel_ShouldEscapeQuotes()
    {
        // Arrange
        var label = "Label with \"quotes\"";

        // Act
        var result = MermaidHelpers.EscapeLabel(label);

        // Assert
        result.Should().Contain("\\\"");
    }

    [Fact]
    public void WrapInMarkdownCodeBlock_ShouldWrapCorrectly()
    {
        // Arrange
        var mermaidCode = "graph TB\n    A --> B";

        // Act
        var result = MermaidHelpers.WrapInMarkdownCodeBlock(mermaidCode);

        // Assert
        result.Should().StartWith("```mermaid\n");
        result.Should().EndWith("\n```");
        result.Should().Contain("graph TB");
    }

    [Theory]
    [InlineData(FlowchartDirection.TopToBottom, "TB")]
    [InlineData(FlowchartDirection.BottomToTop, "BT")]
    [InlineData(FlowchartDirection.LeftToRight, "LR")]
    [InlineData(FlowchartDirection.RightToLeft, "RL")]
    public void GetDirectionCode_ShouldReturnCorrectCode(FlowchartDirection direction, string expected)
    {
        // Act
        var result = MermaidHelpers.GetDirectionCode(direction);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("node1", "Label", NodeShape.Rectangle, "node1[\"Label\"]")]
    [InlineData("node2", "Label", NodeShape.RoundedRectangle, "node2(\"Label\")")]
    [InlineData("node3", "Label", NodeShape.Circle, "node3((\"Label\"))")]
    [InlineData("node4", "Label", NodeShape.Rhombus, "node4{\"Label\"}")]
    public void GetShapeSyntax_ShouldReturnCorrectSyntax(string nodeId, string label, NodeShape shape, string expected)
    {
        // Act
        var result = MermaidHelpers.GetShapeSyntax(nodeId, label, shape);

        // Assert
        result.Should().Be(expected);
    }
}
