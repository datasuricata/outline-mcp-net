namespace Outline.Mcp.Shared.Validation;

/// <summary>
/// Constants for valid Outline values
/// </summary>
public static class OutlineConstants
{
    /// <summary>
    /// Valid permission levels for collections
    /// </summary>
    public static class Permissions
    {
        public const string Read = "read";
        public const string ReadWrite = "read_write";

        public static readonly string[] All = { Read, ReadWrite };

        public static bool IsValid(string? permission)
        {
            return permission == null || All.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Color format validation (hex colors)
    /// </summary>
    public static class Colors
    {
        public static bool IsValid(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return true;

            // Must be hex format: #RRGGBB
            if (!color.StartsWith("#") || color.Length != 7)
                return false;

            return color[1..].All(c => 
                (c >= '0' && c <= '9') || 
                (c >= 'A' && c <= 'F') || 
                (c >= 'a' && c <= 'f'));
        }

        public static string Normalize(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return string.Empty;

            return color.StartsWith("#") ? color.ToUpper() : $"#{color.ToUpper()}";
        }
    }

    /// <summary>
    /// Validation for document/collection names
    /// </summary>
    public static class Names
    {
        public const int MinLength = 1;
        public const int MaxLength = 255;

        public static bool IsValid(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return name.Trim().Length >= MinLength && name.Trim().Length <= MaxLength;
        }
    }

    /// <summary>
    /// Validation for document text content
    /// </summary>
    public static class Content
    {
        public const int MaxLength = 10_000_000; // 10MB in characters

        public static bool IsValid(string? text)
        {
            if (text == null)
                return true;

            return text.Length <= MaxLength;
        }
    }

    /// <summary>
    /// Valid icons for Outline (based on Outline's IconLibrary)
    /// Note: Outline uses emoji or null, not icon names like "BEAKER"
    /// </summary>
    public static class Icons
    {
        /// <summary>
        /// Validates if the icon is a valid emoji or null
        /// Outline doesn't support icon names, only emojis or null
        /// </summary>
        public static bool IsValid(string? icon)
        {
            // Null is valid (no icon)
            if (string.IsNullOrWhiteSpace(icon))
                return true;

            // If it's trying to use an icon name (uppercase letters), it's invalid
            if (icon.All(char.IsUpper) || icon.All(char.IsLetterOrDigit))
                return false;

            // Otherwise assume it's an emoji (Outline will validate)
            return true;
        }

        public static string? GetRecommendation()
        {
            return "Use null for no icon, or provide a single emoji character. Named icons like 'BEAKER' are not supported.";
        }
    }

    /// <summary>
    /// Search limits
    /// </summary>
    public static class Search
    {
        public const int MinLimit = 1;
        public const int MaxLimit = 100;
        public const int DefaultLimit = 25;

        public static int NormalizeLimit(int? limit)
        {
            if (limit == null || limit < MinLimit)
                return DefaultLimit;

            return Math.Min(limit.Value, MaxLimit);
        }
    }
}
