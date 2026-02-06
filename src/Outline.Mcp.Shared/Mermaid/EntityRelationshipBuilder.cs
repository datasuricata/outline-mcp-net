using System.Text;

namespace Outline.Mcp.Shared.Mermaid;

public enum Cardinality
{
    ZeroOrOne,
    ExactlyOne,
    ZeroOrMany,
    OneOrMany
}

public class EntityRelationshipBuilder
{
    private readonly StringBuilder _builder;
    private readonly List<string> _entities;
    private readonly List<string> _relationships;

    public EntityRelationshipBuilder()
    {
        _builder = new StringBuilder();
        _entities = new List<string>();
        _relationships = new List<string>();
    }

    public EntityRelationshipBuilder AddEntity(string entityName, Dictionary<string, string>? attributes = null)
    {
        var sanitizedName = MermaidHelpers.SanitizeId(entityName);
        var entityDef = new StringBuilder();
        entityDef.AppendLine($"    {sanitizedName} {{");
        
        if (attributes != null && attributes.Any())
        {
            foreach (var (attrType, attrName) in attributes)
            {
                entityDef.AppendLine($"        {attrType} {attrName}");
            }
        }
        
        entityDef.AppendLine("    }");
        _entities.Add(entityDef.ToString());
        return this;
    }

    public EntityRelationshipBuilder AddRelationship(
        string entity1,
        string entity2,
        Cardinality cardinality1,
        Cardinality cardinality2,
        string label)
    {
        var sanitizedEntity1 = MermaidHelpers.SanitizeId(entity1);
        var sanitizedEntity2 = MermaidHelpers.SanitizeId(entity2);
        var card1 = GetCardinalitySymbol(cardinality1);
        var card2 = GetCardinalitySymbol(cardinality2);
        var escapedLabel = MermaidHelpers.EscapeLabel(label);
        
        _relationships.Add($"    {sanitizedEntity1} {card1}--{card2} {sanitizedEntity2} : \"{escapedLabel}\"");
        return this;
    }

    private string GetCardinalitySymbol(Cardinality cardinality)
    {
        return cardinality switch
        {
            Cardinality.ZeroOrOne => "|o",
            Cardinality.ExactlyOne => "||",
            Cardinality.ZeroOrMany => "}o",
            Cardinality.OneOrMany => "}|",
            _ => "||"
        };
    }

    public string Build()
    {
        _builder.Clear();
        _builder.AppendLine("erDiagram");
        
        foreach (var entity in _entities)
        {
            _builder.Append(entity);
        }
        
        if (_entities.Any() && _relationships.Any())
        {
            _builder.AppendLine();
        }
        
        foreach (var relationship in _relationships)
        {
            _builder.AppendLine(relationship);
        }
        
        return _builder.ToString().TrimEnd();
    }
}
