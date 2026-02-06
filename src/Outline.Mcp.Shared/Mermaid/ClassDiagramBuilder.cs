using System.Text;

namespace Outline.Mcp.Shared.Mermaid;

public class ClassDiagramBuilder
{
    private readonly StringBuilder _builder;
    private readonly List<string> _classes;
    private readonly List<string> _relationships;

    public ClassDiagramBuilder()
    {
        _builder = new StringBuilder();
        _classes = new List<string>();
        _relationships = new List<string>();
    }

    public ClassDiagramBuilder AddClass(string className, List<string>? properties = null, List<string>? methods = null)
    {
        var sanitizedClassName = MermaidHelpers.SanitizeId(className);
        var classDef = new StringBuilder();
        classDef.AppendLine($"    class {sanitizedClassName} {{");
        
        if (properties != null && properties.Any())
        {
            foreach (var prop in properties)
            {
                classDef.AppendLine($"        {prop}");
            }
        }
        
        if (methods != null && methods.Any())
        {
            foreach (var method in methods)
            {
                classDef.AppendLine($"        {method}");
            }
        }
        
        classDef.AppendLine("    }");
        _classes.Add(classDef.ToString());
        return this;
    }

    public ClassDiagramBuilder AddInheritance(string baseClass, string derivedClass)
    {
        var sanitizedBase = MermaidHelpers.SanitizeId(baseClass);
        var sanitizedDerived = MermaidHelpers.SanitizeId(derivedClass);
        _relationships.Add($"    {sanitizedBase} <|-- {sanitizedDerived}");
        return this;
    }

    public ClassDiagramBuilder AddComposition(string container, string component)
    {
        var sanitizedContainer = MermaidHelpers.SanitizeId(container);
        var sanitizedComponent = MermaidHelpers.SanitizeId(component);
        _relationships.Add($"    {sanitizedContainer} *-- {sanitizedComponent}");
        return this;
    }

    public ClassDiagramBuilder AddAggregation(string whole, string part)
    {
        var sanitizedWhole = MermaidHelpers.SanitizeId(whole);
        var sanitizedPart = MermaidHelpers.SanitizeId(part);
        _relationships.Add($"    {sanitizedWhole} o-- {sanitizedPart}");
        return this;
    }

    public ClassDiagramBuilder AddAssociation(string classA, string classB, string? label = null)
    {
        var sanitizedA = MermaidHelpers.SanitizeId(classA);
        var sanitizedB = MermaidHelpers.SanitizeId(classB);
        
        if (string.IsNullOrEmpty(label))
        {
            _relationships.Add($"    {sanitizedA} --> {sanitizedB}");
        }
        else
        {
            _relationships.Add($"    {sanitizedA} --> {sanitizedB} : {MermaidHelpers.EscapeLabel(label)}");
        }
        
        return this;
    }

    public ClassDiagramBuilder AddDependency(string classA, string classB)
    {
        var sanitizedA = MermaidHelpers.SanitizeId(classA);
        var sanitizedB = MermaidHelpers.SanitizeId(classB);
        _relationships.Add($"    {sanitizedA} ..> {sanitizedB}");
        return this;
    }

    public string Build()
    {
        _builder.Clear();
        _builder.AppendLine("classDiagram");
        
        foreach (var classDef in _classes)
        {
            _builder.Append(classDef);
        }
        
        foreach (var relationship in _relationships)
        {
            _builder.AppendLine(relationship);
        }
        
        return _builder.ToString().TrimEnd();
    }
}
