using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Summary.BudgetStatus;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Summary.BudgetStatus;

public sealed class GetBudgetSummaryQueryHandlerTests
{
    private static readonly int Year = DateTime.UtcNow.Year;
    private static readonly int Month = DateTime.UtcNow.Month;

    [Fact]
    public async Task HandleAsync_WithNoBudgets_ReturnsEmptyList()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("user-1");
        var handler = new GetBudgetSummaryQueryHandler(currentUser, db);

        // Act
        var result = await handler.HandleAsync(new GetBudgetSummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Budgets.Should().BeEmpty();
        result.Value.Year.Should().Be(Year);
        result.Value.Month.Should().Be(Month);
    }

    [Fact]
    public async Task HandleAsync_WithBudgetAndNoTransactions_ReturnsZeroSpent()
    {
        // Arrange
        var userId = "user-1";
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Food", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        db.Budgets.Add(new Budget { CategoryId = category.Id, UserId = userId, Limit = 500m, Month = Month, Year = Year });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetBudgetSummaryQueryHandler(currentUser, db);

        // Act
        var result = await handler.HandleAsync(new GetBudgetSummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Budgets.Should().HaveCount(1);
        var summary = result.Value.Budgets[0];
        summary.Limit.Should().Be(500m);
        summary.Spent.Should().Be(0m);
        summary.Remaining.Should().Be(500m);
        summary.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithTransactionsInBudgetMonth_CalculatesSpentCorrectly()
    {
        // Arrange
        var userId = "user-1";
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Transport", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        db.Budgets.Add(new Budget { CategoryId = category.Id, UserId = userId, Limit = 500m, Month = Month, Year = Year });
        db.Transactions.AddRange(
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 120m, Date = new DateOnly(Year, Month, 1) },
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 80m, Date = new DateOnly(Year, Month, 15) });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetBudgetSummaryQueryHandler(currentUser, db);

        // Act
        var result = await handler.HandleAsync(new GetBudgetSummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Value!.Budgets[0];
        summary.Spent.Should().Be(200m);
        summary.Remaining.Should().Be(300m);
        summary.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenSpentExceedsLimit_IsExceededIsTrue()
    {
        // Arrange
        var userId = "user-1";
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Entertainment", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        db.Budgets.Add(new Budget { CategoryId = category.Id, UserId = userId, Limit = 100m, Month = Month, Year = Year });
        db.Transactions.Add(
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 150m, Date = new DateOnly(Year, Month, 1) });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetBudgetSummaryQueryHandler(currentUser, db);

        // Act
        var result = await handler.HandleAsync(new GetBudgetSummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var summary = result.Value!.Budgets[0];
        summary.Spent.Should().Be(150m);
        summary.IsExceeded.Should().BeTrue();
        summary.Remaining.Should().Be(-50m);
    }

    [Fact]
    public async Task HandleAsync_OnlyReturnsCurrentUserBudgets()
    {
        // Arrange
        var userId = "user-1";
        var otherUserId = "user-2";
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Food", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        db.Budgets.AddRange(
            new Budget { CategoryId = category.Id, UserId = userId, Limit = 500m, Month = Month, Year = Year },
            new Budget { CategoryId = category.Id, UserId = otherUserId, Limit = 300m, Month = Month, Year = Year });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetBudgetSummaryQueryHandler(currentUser, db);

        // Act
        var result = await handler.HandleAsync(new GetBudgetSummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Budgets.Should().HaveCount(1);
        result.Value.Budgets[0].Limit.Should().Be(500m);
    }

    [Fact]
    public async Task HandleAsync_ExcludesTransactionsFromOtherMonths()
    {
        // Arrange
        var userId = "user-1";
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Food", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        db.Budgets.Add(new Budget { CategoryId = category.Id, UserId = userId, Limit = 500m, Month = Month, Year = Year });

        var previousMonth = Month == 1 ? 12 : Month - 1;
        var previousYear = Month == 1 ? Year - 1 : Year;
        db.Transactions.AddRange(
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 100m, Date = new DateOnly(Year, Month, 1) },
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 999m, Date = new DateOnly(previousYear, previousMonth, 1) });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetBudgetSummaryQueryHandler(currentUser, db);

        // Act
        var result = await handler.HandleAsync(new GetBudgetSummaryQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Budgets[0].Spent.Should().Be(100m);
    }
}