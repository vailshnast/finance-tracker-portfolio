using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Transactions.Create;
using FinanceTracker.Domain.Common;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Transactions.Create;

public sealed class CreateTransactionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_With_Created_Transaction()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var category = new Category { UserId = userId, Name = "Groceries" };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new CreateTransactionCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateTransactionCommand(Date: new DateOnly(2023, 10, 1), Amount: 75m, Description: "Grocery shopping", CategoryId: category.Id);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        dbContext.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new CreateTransactionCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateTransactionCommand(Date: new DateOnly(2023, 10, 1), Amount: 75m, Description: "Grocery shopping", CategoryId: Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Transactions.Should().BeEmpty();
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
        var handler = new CreateTransactionCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateTransactionCommand(Date: new DateOnly(2023, 10, 1), Amount: 75m, Description: "Grocery shopping", CategoryId: category.Id);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        dbContext.Transactions.Should().BeEmpty();
    }
}