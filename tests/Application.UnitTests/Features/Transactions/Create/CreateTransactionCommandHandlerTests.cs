using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Transactions.Create;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Transactions.Create;

public sealed class CreateTransactionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_With_Created_Transaction()
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
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        dbContext.Transactions.Should().HaveCount(1);
    }
}
