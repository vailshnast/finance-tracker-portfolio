using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Budgets.Create;
using FinanceTracker.Domain.Common;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Budgets.Create;

public sealed class CreateBudgetCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_With_Created_Budget()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var category = new Category { UserId = userId, Name = "Groceries" };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new CreateBudgetCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateBudgetCommand(Limit: 500m, Month: 6, Year: 2026, CategoryId: category.Id);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        dbContext.Budgets.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new CreateBudgetCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateBudgetCommand(Limit: 500m, Month: 6, Year: 2026, CategoryId: Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Budgets.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Category_Belongs_To_Different_User()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var category = new Category { UserId = "other-user-id", Name = "Groceries" };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new CreateBudgetCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateBudgetCommand(Limit: 500m, Month: 6, Year: 2026, CategoryId: category.Id);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Budgets.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Conflict_When_Budget_Already_Exists_For_Month()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var category = new Category { UserId = userId, Name = "Groceries" };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new CreateBudgetCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateBudgetCommand(Limit: 500m, Month: 6, Year: 2026, CategoryId: category.Id);

        await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Act — second budget for same category/month
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        dbContext.Budgets.Should().HaveCount(1);
    }
}