using Microsoft.Extensions.Logging;
using Pastebin.Services.Implementations;
using Pastebin.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Pastebin.Services.Interfaces;
using Pastebin.Models;
using Pastebin.Exceptions.Like;

namespace Pastebin.Tests.UnitTests;

public class LikeServiceTests
{
    private readonly Mock<ILogger<LikeService>> _loggerMock = new();
    private readonly Mock<IPasteService> _pasteServiceMock = new();
    private readonly LikeService _likeService;
    private readonly AppDbContext _dbContext;

    public LikeServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);

        _pasteServiceMock
            .Setup(x => x.PasteExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _likeService = new(_dbContext, _pasteServiceMock.Object, _loggerMock.Object);
    }


    [Fact]
    public async Task LikePasteAsync_ShouldCreateLike_WhenLikeDoesNotExist()
    {
        // Arrange
        var pasteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _likeService.LikePasteAsync(userId, pasteId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(userId);
        result.PasteId.Should().Be(pasteId);

        var like = await _dbContext.Likes.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        like.Should().NotBeNull();
    }

    [Fact]
    public async Task LikePasteAsync_ShouldThrow_WhenLikeExists()
    {
        // Arrange
        var pasteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _dbContext.Likes.AddAsync(new Like
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PasteId = pasteId,
            CreatedAt = DateTime.UtcNow
        }, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act & Assert
        await Assert.ThrowsAsync<LikeAlreadyExistsException>(() =>
            _likeService.LikePasteAsync(userId, pasteId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteLikeAsync_ShouldDeleteLike_WhenLikeExists()
    {
        // Arrange
        var pasteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _dbContext.Likes.AddAsync(new Like
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PasteId = pasteId,
            CreatedAt = DateTime.UtcNow
        }, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _likeService.DeleteLikeAsync(userId, pasteId, TestContext.Current.CancellationToken);

        // Assert
        (await _dbContext.Likes.AnyAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteLikeAsync_ShouldThrow_WhenLikeDoesNotExist()
    {
        // Arrange
        var pasteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<LikeNotFoundException>(() =>
            _likeService.DeleteLikeAsync(userId, pasteId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLikesByPasteIdAsync_ShouldReturnPaginatedLikes()
    {
        // Arrange
        var pasteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (int i = 0; i < 15; i++)
        {
            await _dbContext.Likes.AddAsync(new Like
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PasteId = pasteId,
                CreatedAt = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);
        }
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _likeService.GetLikesByPasteIdAsync(pasteId, 1, 10, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(10);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetLikesByUserIdAsync_ShouldReturnPaginatedLikes()
    {
        // Arrange
        var pasteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (int i = 0; i < 15; i++)
        {
            await _dbContext.Likes.AddAsync(new Like
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PasteId = pasteId,
                CreatedAt = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);
        }
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _likeService.GetLikesByUserIdAsync(userId, 1, 10, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().HaveCount(10);
        result.HasNextPage.Should().BeTrue();
    }
}
