namespace Outline.Mcp.Shared.Mermaid;

public enum MermaidDiagramType
{
    Flowchart,
    Sequence,
    ClassDiagram,
    EntityRelationship,
    Gantt,
    Pie,
    GitGraph
}

public enum NodeShape
{
    Rectangle,
    RoundedRectangle,
    Circle,
    Cylinder,
    Rhombus,
    Hexagon,
    Asymmetric,
    Trapezoid
}

public enum FlowchartDirection
{
    TopToBottom,
    BottomToTop,
    LeftToRight,
    RightToLeft
}
