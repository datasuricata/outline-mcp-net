namespace Outline.Mcp.Shared.Mermaid;

public class MermaidDiagramBuilder
{
    public FlowchartBuilder CreateFlowchart(FlowchartDirection direction = FlowchartDirection.TopToBottom)
    {
        return new FlowchartBuilder(direction);
    }

    public ClassDiagramBuilder CreateClassDiagram()
    {
        return new ClassDiagramBuilder();
    }

    public SequenceDiagramBuilder CreateSequenceDiagram()
    {
        return new SequenceDiagramBuilder();
    }

    public EntityRelationshipBuilder CreateEntityRelationshipDiagram()
    {
        return new EntityRelationshipBuilder();
    }
}
