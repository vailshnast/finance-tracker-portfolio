using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Budgets.Create;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Budgets.Create;

public sealed class CreateBudgetCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_With_Created_Budget()
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
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        dbContext.Budgets.Should().HaveCount(1);
    }
}
