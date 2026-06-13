using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Budgets.Update;
using FinanceTracker.Domain.Common;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Budgets.Update;

public sealed class UpdateBudgetCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_When_Budget_Updated()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var categoryId = Guid.NewGuid();
        var entity = new Budget { UserId = userId, Limit = 500m, Month = 6, Year = 2026, CategoryId = categoryId };
        dbContext.Budgets.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new UpdateBudgetCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new UpdateBudgetCommand(entity.Id, Limit: 1000m, Month: 6, Year: 2026, CategoryId: categoryId);

        // Act
        await handler.HandleAsync(command, TestContext.Current.CancellationToken);
        var updatedEntity = await dbContext.Budgets.FindAsync([entity.Id], TestContext.Current.CancellationToken);

        // Assert
        updatedEntity.Should().NotBeNull();
        updatedEntity!.Limit.Should().Be(1000m);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new UpdateBudgetCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new UpdateBudgetCommand(Guid.NewGuid(), Limit: 500m, Month: 6, Year: 2026, CategoryId: Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
