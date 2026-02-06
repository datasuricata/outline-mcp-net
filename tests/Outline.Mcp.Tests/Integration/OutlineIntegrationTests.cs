using FluentAssertions;
using Outline.Mcp.Shared.Api;
using Outline.Mcp.Shared.Models;
using Xunit;

namespace Outline.Mcp.Tests.Integration;

/// <summary>
/// Integration tests that require a running Outline instance.
/// Set OUTLINE_BASE_URL and OUTLINE_API_KEY environment variables before running.
/// </summary>
public class OutlineIntegrationTests : IDisposable
{
    private readonly IOutlineApiClient? _client;
    private readonly bool _skipTests;
    private readonly List<string> _createdDocumentIds = new();

    public OutlineIntegrationTests()
    {
        var baseUrl = Environment.GetEnvironmentVariable("OUTLINE_BASE_URL");
        var apiKey = Environment.GetEnvironmentVariable("OUTLINE_API_KEY");

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
        {
            _skipTests = true;
            return;
        }

        _client = new OutlineApiClient(baseUrl, apiKey);
    }

    [Fact]
    public async Task ListCollections_ShouldReturnCollections()
    {
        if (_skipTests) return;

        // Act
        var collections = await _client!.ListCollectionsAsync();

        // Assert
        collections.Should().NotBeNull();
        collections.Should().HaveCountGreaterThan(0);
        collections[0].Id.Should().NotBeNullOrEmpty();
        collections[0].Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAndGetDocument_ShouldWorkCorrectly()
    {
        if (_skipTests) return;

        // Arrange
        var collections = await _client!.ListCollectionsAsync();
        var collectionId = collections.First().Id;

        var request = new CreateDocumentRequest
        {
            Title = $"Test Document {DateTime.UtcNow:yyyyMMddHHmmss}",
            Text = "# Test\n\nThis is a test document created by automated tests.",
            CollectionId = collectionId,
            Publish = false
        };

        // Act - Create
        var created = await _client.CreateDocumentAsync(request);
        _createdDocumentIds.Add(created.Id);

        // Assert - Create
        created.Should().NotBeNull();
        created.Id.Should().NotBeNullOrEmpty();
        created.Title.Should().Be(request.Title);

        // Act - Get
        var retrieved = await _client.GetDocumentAsync(created.Id);

        // Assert - Get
        retrieved.Should().NotBeNull();
        retrieved.Id.Should().Be(created.Id);
        retrieved.Title.Should().Be(created.Title);
    }

    [Fact]
    public async Task UpdateDocument_ShouldUpdateContent()
    {
        if (_skipTests) return;

        // Arrange
        var collections = await _client!.ListCollectionsAsync();
        var collectionId = collections.First().Id;

        var createRequest = new CreateDocumentRequest
        {
            Title = $"Test Document {DateTime.UtcNow:yyyyMMddHHmmss}",
            Text = "# Original Content",
            CollectionId = collectionId,
            Publish = false
        };

        var created = await _client.CreateDocumentAsync(createRequest);
        _createdDocumentIds.Add(created.Id);

        // Act
        var updateRequest = new UpdateDocumentRequest
        {
            Title = "Updated Title",
            Text = "# Updated Content"
        };

        var updated = await _client.UpdateDocumentAsync(created.Id, updateRequest);

        // Assert
        updated.Should().NotBeNull();
        updated.Title.Should().Be("Updated Title");
        updated.Revision.Should().BeGreaterThan(created.Revision);
    }

    [Fact]
    public async Task SearchDocuments_ShouldFindCreatedDocument()
    {
        if (_skipTests) return;

        // Arrange
        var collections = await _client!.ListCollectionsAsync();
        var collectionId = collections.First().Id;

        var uniqueTitle = $"SearchTest {Guid.NewGuid():N}";
        var createRequest = new CreateDocumentRequest
        {
            Title = uniqueTitle,
            Text = "# Searchable Content\n\nThis document should be found by search.",
            CollectionId = collectionId,
            Publish = true
        };

        var created = await _client.CreateDocumentAsync(createRequest);
        _createdDocumentIds.Add(created.Id);

        // Wait a bit for indexing
        await Task.Delay(2000);

        // Act
        var searchResults = await _client.SearchDocumentsAsync(uniqueTitle, collectionId);

        // Assert
        searchResults.Should().NotBeEmpty();
        searchResults.Should().Contain(r => r.Document.Id == created.Id);
    }

    public void Dispose()
    {
        if (_skipTests || _client == null) return;

        // Cleanup created documents
        foreach (var docId in _createdDocumentIds)
        {
            try
            {
                _client.DeleteDocumentAsync(docId, permanent: true).Wait();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
