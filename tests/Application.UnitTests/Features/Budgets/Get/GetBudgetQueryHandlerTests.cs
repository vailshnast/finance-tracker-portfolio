using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Budgets.Get;
using FinanceTracker.Domain.Common;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Budgets.Get;

public sealed class GetBudgetQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Budget_When_Found()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var entity = new Budget { UserId = userId };
        dbContext.Budgets.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetBudgetQueryHandler(dbContext, currentUser, new PassthroughHybridCache());

        // Act
        var result = await handler.HandleAsync(new GetBudgetQuery(entity.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new GetBudgetQueryHandler(dbContext, currentUser, new PassthroughHybridCache());

        // Act
        var result = await handler.HandleAsync(new GetBudgetQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
