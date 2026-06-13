using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Transactions.Update;
using FinanceTracker.Domain.Common;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Transactions.Update;

public sealed class UpdateTransactionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_When_Transaction_Updated()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var categoryId = Guid.NewGuid();
        var entity = new Transaction { UserId = userId, Amount = 50m, Description = "Original", CategoryId = categoryId };
        dbContext.Transactions.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new UpdateTransactionCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new UpdateTransactionCommand(Date: new DateOnly(), Id: entity.Id, Amount: 120m, Description: "Updated", CategoryId: categoryId);

        // Act
        await handler.HandleAsync(command, TestContext.Current.CancellationToken);
        var updatedEntity = await dbContext.Transactions.FindAsync([entity.Id], TestContext.Current.CancellationToken);

        // Assert
        updatedEntity.Should().NotBeNull();
        updatedEntity!.Amount.Should().Be(120m);
        updatedEntity.Description.Should().Be("Updated");
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new UpdateTransactionCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new UpdateTransactionCommand(Date: new DateOnly(), Id: Guid.NewGuid(), Amount: 50m, Description: null, CategoryId: Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
