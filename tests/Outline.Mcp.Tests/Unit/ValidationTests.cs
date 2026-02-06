using FluentAssertions;
using Outline.Mcp.Shared.Exceptions;
using Outline.Mcp.Shared.Models;
using Outline.Mcp.Shared.Validation;
using Xunit;

namespace Outline.Mcp.Tests.Unit;

public class ValidationTests
{
    [Fact]
    public void OutlineConstants_Permissions_ShouldValidateCorrectly()
    {
        // Valid permissions
        OutlineConstants.Permissions.IsValid("read").Should().BeTrue();
        OutlineConstants.Permissions.IsValid("read_write").Should().BeTrue();
        OutlineConstants.Permissions.IsValid(null).Should().BeTrue();

        // Invalid permissions
        OutlineConstants.Permissions.IsValid("write").Should().BeFalse();
        OutlineConstants.Permissions.IsValid("admin").Should().BeFalse();
    }

    [Theory]
    [InlineData("#FF5733", true)]
    [InlineData("#000000", true)]
    [InlineData("#FFFFFF", true)]
    [InlineData("#ff5733", true)]  // lowercase should be valid
    [InlineData(null, true)]  // null is valid
    [InlineData("", true)]  // empty is valid
    [InlineData("FF5733", false)]  // missing #
    [InlineData("#FF57", false)]  // too short
    [InlineData("#FF573333", false)]  // too long
    [InlineData("#GGGGGG", false)]  // invalid chars
    public void OutlineConstants_Colors_ShouldValidateCorrectly(string? color, bool expectedValid)
    {
        OutlineConstants.Colors.IsValid(color).Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("Valid Name", true)]
    [InlineData("A", true)]  // min length
    [InlineData("", false)]  // empty
    [InlineData(null, false)]  // null
    [InlineData("   ", false)]  // whitespace only
    public void OutlineConstants_Names_ShouldValidateCorrectly(string? name, bool expectedValid)
    {
        OutlineConstants.Names.IsValid(name).Should().Be(expectedValid);
    }

    [Fact]
    public void OutlineConstants_Icons_ShouldRejectNamedIcons()
    {
        // Named icons (uppercase) should be invalid
        OutlineConstants.Icons.IsValid("BEAKER").Should().BeFalse();
        OutlineConstants.Icons.IsValid("BOOK").Should().BeFalse();
        OutlineConstants.Icons.IsValid("FOLDER").Should().BeFalse();

        // Null should be valid
        OutlineConstants.Icons.IsValid(null).Should().BeTrue();
        OutlineConstants.Icons.IsValid("").Should().BeTrue();

        // Emojis (mixed case/symbols) should be valid
        OutlineConstants.Icons.IsValid("📚").Should().BeTrue();
        OutlineConstants.Icons.IsValid("🔥").Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(25, 25)]
    [InlineData(100, 100)]
    [InlineData(0, 25)]  // below min -> default
    [InlineData(-1, 25)]  // negative -> default
    [InlineData(150, 100)]  // above max -> max
    [InlineData(null, 25)]  // null -> default
    public void OutlineConstants_Search_ShouldNormalizeLimits(int? input, int expected)
    {
        OutlineConstants.Search.NormalizeLimit(input).Should().Be(expected);
    }

    [Fact]
    public void RequestValidator_CreateCollection_ShouldThrowOnInvalidName()
    {
        // Arrange
        var request = new CreateCollectionRequest
        {
            Name = "",  // Invalid: empty
            Permission = "read_write"
        };

        // Act
        Action act = () => RequestValidator.ValidateCreateCollection(request);

        // Assert
        act.Should().Throw<OutlineValidationException>()
            .Which.ValidationErrors.Should().ContainKey("name");
    }

    [Fact]
    public void RequestValidator_CreateCollection_ShouldThrowOnInvalidPermission()
    {
        // Arrange
        var request = new CreateCollectionRequest
        {
            Name = "Valid Name",
            Permission = "admin"  // Invalid permission
        };

        // Act
        Action act = () => RequestValidator.ValidateCreateCollection(request);

        // Assert
        act.Should().Throw<OutlineValidationException>()
            .Which.ValidationErrors.Should().ContainKey("permission");
    }

    [Fact]
    public void RequestValidator_CreateCollection_ShouldThrowOnInvalidColor()
    {
        // Arrange
        var request = new CreateCollectionRequest
        {
            Name = "Valid Name",
            Color = "InvalidColor"  // Invalid color format
        };

        // Act
        Action act = () => RequestValidator.ValidateCreateCollection(request);

        // Assert
        act.Should().Throw<OutlineValidationException>()
            .Which.ValidationErrors.Should().ContainKey("color");
    }

    [Fact]
    public void RequestValidator_CreateCollection_ShouldThrowOnNamedIcon()
    {
        // Arrange
        var request = new CreateCollectionRequest
        {
            Name = "Valid Name",
            Icon = "BEAKER"  // Invalid: named icon not supported
        };

        // Act
        Action act = () => RequestValidator.ValidateCreateCollection(request);

        // Assert
        act.Should().Throw<OutlineValidationException>()
            .Which.ValidationErrors.Should().ContainKey("icon");
    }

    [Fact]
    public void RequestValidator_CreateCollection_ShouldPassWithValidData()
    {
        // Arrange
        var request = new CreateCollectionRequest
        {
            Name = "Valid Collection",
            Description = "Valid Description",
            Permission = "read_write",
            Color = "#FF5733",
            Icon = null
        };

        // Act
        Action act = () => RequestValidator.ValidateCreateCollection(request);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RequestValidator_CreateDocument_ShouldThrowOnEmptyTitle()
    {
        // Arrange
        var request = new CreateDocumentRequest
        {
            Title = "",  // Invalid
            Text = "Content",
            CollectionId = "col-123"
        };

        // Act
        Action act = () => RequestValidator.ValidateCreateDocument(request);

        // Assert
        act.Should().Throw<OutlineValidationException>()
            .Which.ValidationErrors.Should().ContainKey("title");
    }

    [Fact]
    public void RequestValidator_CreateDocument_ShouldThrowOnMissingCollectionId()
    {
        // Arrange
        var request = new CreateDocumentRequest
        {
            Title = "Valid Title",
            Text = "Content",
            CollectionId = ""  // Invalid: missing
        };

        // Act
        Action act = () => RequestValidator.ValidateCreateDocument(request);

        // Assert
        act.Should().Throw<OutlineValidationException>()
            .Which.ValidationErrors.Should().ContainKey("collectionId");
    }

    [Fact]
    public void RequestValidator_CreateDocument_ShouldPassWithValidData()
    {
        // Arrange
        var request = new CreateDocumentRequest
        {
            Title = "Valid Document",
            Text = "# Content\n\nValid markdown content",
            CollectionId = "col-123",
            Icon = "📄",
            Color = "#4E5C6E"
        };

        // Act
        Action act = () => RequestValidator.ValidateCreateDocument(request);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RequestValidator_UpdateDocument_ShouldThrowOnMissingDocumentId()
    {
        // Arrange
        var request = new UpdateDocumentRequest
        {
            Title = "Updated Title"
        };

        // Act
        Action act = () => RequestValidator.ValidateUpdateDocument("", request);

        // Assert
        act.Should().Throw<OutlineValidationException>()
            .Which.ValidationErrors.Should().ContainKey("documentId");
    }

    [Fact]
    public void RequestValidator_Search_ShouldThrowOnEmptyQuery()
    {
        // Arrange
        var request = new SearchDocumentsRequest
        {
            Query = ""  // Invalid
        };

        // Act
        Action act = () => RequestValidator.ValidateSearch(request);

        // Assert
        act.Should().Throw<OutlineValidationException>()
            .Which.ValidationErrors.Should().ContainKey("query");
    }

    [Fact]
    public void RequestValidator_Search_ShouldNormalizeLimit()
    {
        // Arrange
        var request = new SearchDocumentsRequest
        {
            Query = "test",
            Limit = 150  // Above max
        };

        // Act
        RequestValidator.ValidateSearch(request);

        // Assert
        request.Limit.Should().Be(100);  // Normalized to max
    }
}
