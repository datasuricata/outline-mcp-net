namespace Outline.Mcp.Shared.Mermaid;

public static class MermaidHelpers
{
    public static string WrapInMarkdownCodeBlock(string mermaidCode)
    {
        return $"```mermaid\n{mermaidCode}\n```";
    }

    public static string GetShapeSyntax(string nodeId, string label, NodeShape shape)
    {
        return shape switch
        {
            NodeShape.Rectangle => $"{nodeId}[\"{label}\"]",
            NodeShape.RoundedRectangle => $"{nodeId}(\"{label}\")",
            NodeShape.Circle => $"{nodeId}((\"{label}\"))",
            NodeShape.Cylinder => $"{nodeId}[(\"{label}\")]",
            NodeShape.Rhombus => $"{nodeId}{{\"{label}\"}}",
            NodeShape.Hexagon => $"{nodeId}{{{{\"{label}\"}}}}",
            NodeShape.Asymmetric => $"{nodeId}>\"{label}\"]",
            NodeShape.Trapezoid => $"{nodeId}[\\\"{label}\"/]",
            _ => $"{nodeId}[\"{label}\"]"
        };
    }

    public static string GetDirectionCode(FlowchartDirection direction)
    {
        return direction switch
        {
            FlowchartDirection.TopToBottom => "TB",
            FlowchartDirection.BottomToTop => "BT",
            FlowchartDirection.LeftToRight => "LR",
            FlowchartDirection.RightToLeft => "RL",
            _ => "TB"
        };
    }

    public static string EscapeLabel(string label)
    {
        return label.Replace("\"", "\\\"");
    }

    public static string SanitizeId(string id)
    {
        return id.Replace(" ", "_")
                 .Replace("-", "_")
                 .Replace(".", "_")
                 .Replace("/", "_")
                 .Replace("\\", "_");
    }
}
