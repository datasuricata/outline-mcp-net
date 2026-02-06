using FluentAssertions;
using Moq;
using Moq.Protected;
using Outline.Mcp.Shared.Api;
using Outline.Mcp.Shared.Exceptions;
using Outline.Mcp.Shared.Models;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Outline.Mcp.Tests.Unit;

public class ExceptionTests
{
    [Fact]
    public async Task OutlineApiClient_ShouldThrowAuthenticationException_On401()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var errorResponse = new
        {
            ok = false,
            error = "authentication_required",
            message = "Bad Authorization header format"
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "invalid-api-key");

        // Act
        Func<Task> act = async () => await client.ListCollectionsAsync();

        // Assert
        await act.Should().ThrowAsync<OutlineAuthenticationException>()
            .Where(ex => ex.StatusCode == 401);
    }

    [Fact]
    public async Task OutlineApiClient_ShouldThrowNotFoundException_On404()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var errorResponse = new
        {
            ok = false,
            error = "not_found",
            message = "Document not found"
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        // Act
        Func<Task> act = async () => await client.GetDocumentAsync("non-existent-id");

        // Assert
        await act.Should().ThrowAsync<OutlineNotFoundException>()
            .Where(ex => ex.StatusCode == 404);
    }

    [Fact]
    public async Task OutlineApiClient_ShouldThrowRateLimitException_On429()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var errorResponse = new
        {
            ok = false,
            error = "rate_limit_exceeded",
            message = "Too many requests",
            retryAfter = 60
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)429,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        // Act
        Func<Task> act = async () => await client.ListCollectionsAsync();

        // Assert
        await act.Should().ThrowAsync<OutlineRateLimitException>()
            .Where(ex => ex.StatusCode == 429 && ex.RetryAfterSeconds == 60);
    }

    [Fact]
    public async Task OutlineApiClient_ShouldThrowValidationException_On400()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var errorResponse = new
        {
            ok = false,
            error = "validation_error",
            message = "Invalid request data"
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        // Act
        Func<Task> act = async () => await client.CreateCollectionAsync(new CreateCollectionRequest
        {
            Name = "Test",
            Permission = "read_write"
        });

        // Assert
        await act.Should().ThrowAsync<OutlineValidationException>()
            .Where(ex => ex.StatusCode == 400);
    }

    [Fact]
    public void OutlineValidationException_ShouldContainValidationErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            ["name"] = new[] { "Name is required" },
            ["color"] = new[] { "Invalid color format" }
        };

        // Act
        var exception = new OutlineValidationException("Validation failed", errors);

        // Assert
        exception.ValidationErrors.Should().ContainKey("name");
        exception.ValidationErrors.Should().ContainKey("color");
        exception.ValidationErrors!["name"].Should().Contain("Name is required");
    }

    [Fact]
    public async Task OutlineApiClient_CreateDocument_ShouldThrowValidationException_OnInvalidData()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        var invalidRequest = new CreateDocumentRequest
        {
            Title = "",  // Invalid: empty title
            Text = "Content",
            CollectionId = "col-123"
        };

        // Act
        Func<Task> act = async () => await client.CreateDocumentAsync(invalidRequest);

        // Assert
        await act.Should().ThrowAsync<OutlineValidationException>()
            .Where(ex => ex.ValidationErrors != null && ex.ValidationErrors.ContainsKey("title"));
    }

    [Fact]
    public async Task OutlineApiClient_CreateCollection_ShouldThrowValidationException_OnInvalidIcon()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        var invalidRequest = new CreateCollectionRequest
        {
            Name = "Test Collection",
            Icon = "BEAKER",  // Invalid: named icon not supported
            Permission = "read_write"
        };

        // Act
        Func<Task> act = async () => await client.CreateCollectionAsync(invalidRequest);

        // Assert
        await act.Should().ThrowAsync<OutlineValidationException>()
            .Where(ex => ex.ValidationErrors != null && ex.ValidationErrors.ContainsKey("icon"));
    }
}
