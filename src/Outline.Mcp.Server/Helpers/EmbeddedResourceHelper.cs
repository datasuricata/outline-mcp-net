using System.Reflection;

namespace Outline.Mcp.Server.Helpers;

/// <summary>
/// Helper for reading embedded resources from the assembly.
/// Skills templates are embedded to avoid file system dependencies.
/// </summary>
public static class EmbeddedResourceHelper
{
    /// <summary>
    /// Reads an embedded resource by name.
    /// </summary>
    /// <param name="resourceName">Name of the resource (e.g., "project-documentation.md")</param>
    /// <returns>Content of the embedded resource</returns>
    /// <exception cref="FileNotFoundException">Thrown when resource is not found</exception>
    public static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
        
        if (fullName == null)
        {
            var availableResources = string.Join(", ", assembly.GetManifestResourceNames());
            throw new FileNotFoundException(
                $"Embedded resource not found: {resourceName}. Available resources: {availableResources}");
        }
        
        using var stream = assembly.GetManifestResourceStream(fullName);
        if (stream == null)
            throw new FileNotFoundException($"Could not open embedded resource stream: {resourceName}");
            
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
