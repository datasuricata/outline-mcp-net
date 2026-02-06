using System.Text;

namespace Outline.Mcp.Shared.Mermaid;

public class FlowchartBuilder
{
    private readonly StringBuilder _builder;
    private readonly FlowchartDirection _direction;
    private readonly List<string> _nodes;
    private readonly List<string> _edges;
    private readonly List<string> _subgraphs;

    public FlowchartBuilder(FlowchartDirection direction = FlowchartDirection.TopToBottom)
    {
        _builder = new StringBuilder();
        _direction = direction;
        _nodes = new List<string>();
        _edges = new List<string>();
        _subgraphs = new List<string>();
    }

    public FlowchartBuilder AddNode(string id, string label, NodeShape shape = NodeShape.Rectangle)
    {
        var sanitizedId = MermaidHelpers.SanitizeId(id);
        var escapedLabel = MermaidHelpers.EscapeLabel(label);
        var nodeDef = MermaidHelpers.GetShapeSyntax(sanitizedId, escapedLabel, shape);
        _nodes.Add($"    {nodeDef}");
        return this;
    }

    public FlowchartBuilder AddEdge(string fromId, string toId, string? label = null)
    {
        var sanitizedFromId = MermaidHelpers.SanitizeId(fromId);
        var sanitizedToId = MermaidHelpers.SanitizeId(toId);
        
        if (string.IsNullOrEmpty(label))
        {
            _edges.Add($"    {sanitizedFromId} --> {sanitizedToId}");
        }
        else
        {
            var escapedLabel = MermaidHelpers.EscapeLabel(label);
            _edges.Add($"    {sanitizedFromId} -->|\"{escapedLabel}\"| {sanitizedToId}");
        }
        
        return this;
    }

    public FlowchartBuilder AddSubgraph(string id, string title, Action<FlowchartBuilder> buildSubgraph)
    {
        var sanitizedId = MermaidHelpers.SanitizeId(id);
        var subgraphBuilder = new FlowchartBuilder(_direction);
        buildSubgraph(subgraphBuilder);
        
        var subgraphContent = new StringBuilder();
        subgraphContent.AppendLine($"    subgraph {sanitizedId} [\"{MermaidHelpers.EscapeLabel(title)}\"]");
        
        foreach (var node in subgraphBuilder._nodes)
        {
            subgraphContent.AppendLine($"    {node}");
        }
        
        foreach (var edge in subgraphBuilder._edges)
        {
            subgraphContent.AppendLine($"    {edge}");
        }
        
        subgraphContent.AppendLine("    end");
        
        _subgraphs.Add(subgraphContent.ToString());
        return this;
    }

    public string Build()
    {
        _builder.Clear();
        _builder.AppendLine($"flowchart {MermaidHelpers.GetDirectionCode(_direction)}");
        
        foreach (var node in _nodes)
        {
            _builder.AppendLine(node);
        }
        
        foreach (var edge in _edges)
        {
            _builder.AppendLine(edge);
        }
        
        foreach (var subgraph in _subgraphs)
        {
            _builder.Append(subgraph);
        }
        
        return _builder.ToString().TrimEnd();
    }
}
