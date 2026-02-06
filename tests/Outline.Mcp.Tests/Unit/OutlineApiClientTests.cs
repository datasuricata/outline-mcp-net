using FluentAssertions;
using Moq;
using Moq.Protected;
using Outline.Mcp.Shared.Api;
using Outline.Mcp.Shared.Models;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Outline.Mcp.Tests.Unit;

public class OutlineApiClientTests
{
    [Fact]
    public async Task ListCollectionsAsync_WithCollectionsField_ShouldReturnCollections()
    {
        // Arrange - Test response format with "collections" field
        var mockHandler = new Mock<HttpMessageHandler>();
        var collectionsResponse = new CollectionsData
        {
            Collections = new List<OutlineCollection>
            {
                new OutlineCollection { Id = "1", Name = "Test Collection" }
            }
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(collectionsResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        // Act
        var result = await client.ListCollectionsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("1");
        result[0].Name.Should().Be("Test Collection");
    }

    [Fact]
    public async Task ListCollectionsAsync_WithDataField_ShouldReturnCollections()
    {
        // Arrange - Test response format with "data" field (alternative API format)
        var mockHandler = new Mock<HttpMessageHandler>();
        var collectionsResponse = new CollectionsData
        {
            Data = new List<OutlineCollection>
            {
                new OutlineCollection { Id = "2", Name = "Another Collection" },
                new OutlineCollection { Id = "3", Name = "Third Collection" }
            }
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(collectionsResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        // Act
        var result = await client.ListCollectionsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("2");
        result[0].Name.Should().Be("Another Collection");
        result[1].Id.Should().Be("3");
        result[1].Name.Should().Be("Third Collection");
    }

    [Fact]
    public async Task ListCollectionsAsync_WithEmptyResponse_ShouldReturnEmptyList()
    {
        // Arrange - Test empty response
        var mockHandler = new Mock<HttpMessageHandler>();
        var collectionsResponse = new CollectionsData();

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(collectionsResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        // Act
        var result = await client.ListCollectionsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchDocumentsAsync_ShouldReturnSearchResults()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var searchResponse = new SearchResultsData
        {
            Data = new List<SearchResult>
            {
                new SearchResult 
                { 
                    Ranking = 1.5,
                    Context = "Test context",
                    Document = new OutlineDocument { Id = "1", Title = "Test Doc" }
                }
            }
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(searchResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        // Act
        var result = await client.SearchDocumentsAsync("test query");

        // Assert
        result.Should().HaveCount(1);
        result[0].Ranking.Should().Be(1.5);
        result[0].Context.Should().Be("Test context");
        result[0].Document.Title.Should().Be("Test Doc");
    }

    [Fact]
    public async Task CreateDocumentAsync_ShouldReturnCreatedDocument()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var documentResponse = new DocumentData
        {
            Document = new OutlineDocument
            {
                Id = "new-doc-id",
                Title = "New Document",
                Text = "# Content",
                CreatedAt = DateTime.UtcNow
            }
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(documentResponse))
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var client = new OutlineApiClient(httpClient, "test-api-key");

        var request = new CreateDocumentRequest
        {
            Title = "New Document",
            Text = "# Content",
            CollectionId = "collection-id"
        };

        // Act
        var result = await client.CreateDocumentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("new-doc-id");
        result.Title.Should().Be("New Document");
    }
}
