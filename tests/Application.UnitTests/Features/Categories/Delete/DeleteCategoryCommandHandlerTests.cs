using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Categories.Delete;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Categories.Delete;

public sealed class DeleteCategoryCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Delete_Category_When_Found()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var entity = new Category { UserId = userId, Name = "category-name" };
        dbContext.Categories.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new DeleteCategoryCommandHandler(dbContext, currentUser, new PassthroughHybridCache());

        // Act
        var result = await handler.HandleAsync(new DeleteCategoryCommand(entity.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        dbContext.Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new DeleteCategoryCommandHandler(dbContext, currentUser, new PassthroughHybridCache());

        // Act
        var result = await handler.HandleAsync(new DeleteCategoryCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
