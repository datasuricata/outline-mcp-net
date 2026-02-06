namespace Outline.Mcp.Shared.Exceptions;

/// <summary>
/// Base exception for Outline API errors
/// </summary>
public class OutlineException : Exception
{
    public int? StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ApiResponse { get; set; }

    public OutlineException(string message) : base(message) { }

    public OutlineException(string message, Exception innerException) 
        : base(message, innerException) { }

    public OutlineException(string message, int statusCode, string? errorCode = null, string? apiResponse = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ApiResponse = apiResponse;
    }
}

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class OutlineAuthenticationException : OutlineException
{
    public OutlineAuthenticationException(string message, string? apiResponse = null)
        : base(message, 401, "authentication_failed", apiResponse) { }
}

/// <summary>
/// Exception thrown when a resource is not found
/// </summary>
public class OutlineNotFoundException : OutlineException
{
    public OutlineNotFoundException(string resourceType, string resourceId, string? apiResponse = null)
        : base($"{resourceType} '{resourceId}' not found", 404, "not_found", apiResponse) { }
}

/// <summary>
/// Exception thrown when input validation fails
/// </summary>
public class OutlineValidationException : OutlineException
{
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public OutlineValidationException(string message, Dictionary<string, string[]>? errors = null)
        : base(message, 400, "validation_error", null)
    {
        ValidationErrors = errors;
    }
}

/// <summary>
/// Exception thrown when rate limit is exceeded
/// </summary>
public class OutlineRateLimitException : OutlineException
{
    public int? RetryAfterSeconds { get; set; }

    public OutlineRateLimitException(string message, int? retryAfter = null)
        : base(message, 429, "rate_limit_exceeded", null)
    {
        RetryAfterSeconds = retryAfter;
    }
}

/// <summary>
/// Exception thrown when API request fails
/// </summary>
public class OutlineApiException : OutlineException
{
    public OutlineApiException(string message, int statusCode, string? errorCode = null, string? apiResponse = null)
        : base(message, statusCode, errorCode, apiResponse) { }
}
