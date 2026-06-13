using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Categories.Update;
using FinanceTracker.Domain.Common;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Categories.Update;

public sealed class UpdateCategoryCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_When_Category_Updated()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var entity = new Category { UserId = userId, Name = "category-name", Type = CategoryType.Income };
        dbContext.Categories.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new UpdateCategoryCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new UpdateCategoryCommand(entity.Id, "new-category-name", CategoryType.Expense);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);
        var category = await dbContext.Categories.FindAsync([entity.Id], TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Should().NotBeNull();
        category.Name.Should().Be("new-category-name");
        category.Type.Should().Be(CategoryType.Expense);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new UpdateCategoryCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new UpdateCategoryCommand(Guid.NewGuid(),"new-category-name", CategoryType.Expense);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
