using System.Text;

namespace Outline.Mcp.Shared.Mermaid;

public class SequenceDiagramBuilder
{
    private readonly StringBuilder _builder;
    private readonly List<string> _participants;
    private readonly List<string> _interactions;

    public SequenceDiagramBuilder()
    {
        _builder = new StringBuilder();
        _participants = new List<string>();
        _interactions = new List<string>();
    }

    public SequenceDiagramBuilder AddParticipant(string id, string? alias = null)
    {
        var sanitizedId = MermaidHelpers.SanitizeId(id);
        
        if (string.IsNullOrEmpty(alias))
        {
            _participants.Add($"    participant {sanitizedId}");
        }
        else
        {
            _participants.Add($"    participant {sanitizedId} as {MermaidHelpers.EscapeLabel(alias)}");
        }
        
        return this;
    }

    public SequenceDiagramBuilder AddMessage(string from, string to, string message)
    {
        var sanitizedFrom = MermaidHelpers.SanitizeId(from);
        var sanitizedTo = MermaidHelpers.SanitizeId(to);
        var escapedMessage = MermaidHelpers.EscapeLabel(message);
        _interactions.Add($"    {sanitizedFrom}->>+{sanitizedTo}: {escapedMessage}");
        return this;
    }

    public SequenceDiagramBuilder AddReturn(string from, string to, string? message = null)
    {
        var sanitizedFrom = MermaidHelpers.SanitizeId(from);
        var sanitizedTo = MermaidHelpers.SanitizeId(to);
        
        if (string.IsNullOrEmpty(message))
        {
            _interactions.Add($"    {sanitizedFrom}-->>-{sanitizedTo}: ");
        }
        else
        {
            _interactions.Add($"    {sanitizedFrom}-->>-{sanitizedTo}: {MermaidHelpers.EscapeLabel(message)}");
        }
        
        return this;
    }

    public SequenceDiagramBuilder AddNote(string participant, string note, bool right = true)
    {
        var sanitizedParticipant = MermaidHelpers.SanitizeId(participant);
        var position = right ? "right of" : "left of";
        var escapedNote = MermaidHelpers.EscapeLabel(note);
        _interactions.Add($"    Note {position} {sanitizedParticipant}: {escapedNote}");
        return this;
    }

    public SequenceDiagramBuilder AddLoop(string condition, Action<SequenceDiagramBuilder> buildLoop)
    {
        _interactions.Add($"    loop {MermaidHelpers.EscapeLabel(condition)}");
        var loopBuilder = new SequenceDiagramBuilder();
        buildLoop(loopBuilder);
        
        foreach (var interaction in loopBuilder._interactions)
        {
            _interactions.Add($"    {interaction}");
        }
        
        _interactions.Add("    end");
        return this;
    }

    public string Build()
    {
        _builder.Clear();
        _builder.AppendLine("sequenceDiagram");
        
        foreach (var participant in _participants)
        {
            _builder.AppendLine(participant);
        }
        
        if (_participants.Any() && _interactions.Any())
        {
            _builder.AppendLine();
        }
        
        foreach (var interaction in _interactions)
        {
            _builder.AppendLine(interaction);
        }
        
        return _builder.ToString().TrimEnd();
    }
}
