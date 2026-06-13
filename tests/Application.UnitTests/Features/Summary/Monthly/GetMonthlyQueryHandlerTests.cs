using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Summary.Monthly;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Summary.Monthly;

public sealed class GetMonthlyQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithNoTransactions_ReturnsEmptyResponse()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("user-1");
        var handler = new GetMonthlyQueryHandler(db, currentUser);

        // Act
        var result = await handler.HandleAsync(new GetMonthlySummaryQuery(6, 2025), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CategorySummaries.Should().BeEmpty();
        result.Value.TotalIncome.Should().Be(0);
        result.Value.TotalExpenses.Should().Be(0);
        result.Value.NetBalance.Should().Be(0);
        result.Value.Month.Should().Be(6);
        result.Value.Year.Should().Be(2025);
    }

    [Fact]
    public async Task HandleAsync_WithExpenseTransactions_ReturnsCategorySummary()
    {
        // Arrange
        var userId = "user-1";
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Food", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        db.Transactions.AddRange(
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 50m, Date = new DateOnly(2025, 6, 1) },
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 30m, Date = new DateOnly(2025, 6, 15) });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetMonthlyQueryHandler(db, currentUser);

        // Act
        var result = await handler.HandleAsync(new GetMonthlySummaryQuery(6, 2025), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalExpenses.Should().Be(80m);
        result.Value.TotalIncome.Should().Be(0);
        result.Value.NetBalance.Should().Be(-80m);
        result.Value.CategorySummaries.Should().HaveCount(1);
        result.Value.CategorySummaries[0].CategoryName.Should().Be("Food");
        result.Value.CategorySummaries[0].Total.Should().Be(80m);
        result.Value.CategorySummaries[0].TransactionAmount.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WithIncomeAndExpense_CalculatesNetBalanceCorrectly()
    {
        // Arrange
        var userId = "user-1";
        await using var db = TestDbContextFactory.Create();

        var income = new Category { Name = "Salary", Type = CategoryType.Income, UserId = userId };
        var expense = new Category { Name = "Rent", Type = CategoryType.Expense, UserId = userId };
        db.Categories.AddRange(income, expense);
        db.Transactions.AddRange(
            new Transaction { CategoryId = income.Id, UserId = userId, Amount = 3000m, Date = new DateOnly(2025, 6, 1) },
            new Transaction { CategoryId = expense.Id, UserId = userId, Amount = 1200m, Date = new DateOnly(2025, 6, 5) });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetMonthlyQueryHandler(db, currentUser);

        // Act
        var result = await handler.HandleAsync(new GetMonthlySummaryQuery(6, 2025), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalIncome.Should().Be(3000m);
        result.Value.TotalExpenses.Should().Be(1200m);
        result.Value.NetBalance.Should().Be(1800m);
        result.Value.CategorySummaries.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WithTransactionsFromDifferentMonth_OnlyReturnsRequestedMonth()
    {
        // Arrange
        var userId = "user-1";
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Food", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        db.Transactions.AddRange(
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 50m, Date = new DateOnly(2025, 6, 1) },
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 200m, Date = new DateOnly(2025, 5, 1) },
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 100m, Date = new DateOnly(2024, 6, 1) });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetMonthlyQueryHandler(db, currentUser);

        // Act
        var result = await handler.HandleAsync(new GetMonthlySummaryQuery(6, 2025), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalExpenses.Should().Be(50m);
    }

    [Fact]
    public async Task HandleAsync_WithTransactionsFromDifferentUser_OnlyReturnsCurrentUserData()
    {
        // Arrange
        var userId = "user-1";
        var otherUserId = "user-2";
        await using var db = TestDbContextFactory.Create();

        var category = new Category { Name = "Food", Type = CategoryType.Expense, UserId = userId };
        db.Categories.Add(category);
        db.Transactions.AddRange(
            new Transaction { CategoryId = category.Id, UserId = userId, Amount = 50m, Date = new DateOnly(2025, 6, 1) },
            new Transaction { CategoryId = category.Id, UserId = otherUserId, Amount = 200m, Date = new DateOnly(2025, 6, 1) });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetMonthlyQueryHandler(db, currentUser);

        // Act
        var result = await handler.HandleAsync(new GetMonthlySummaryQuery(6, 2025), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalExpenses.Should().Be(50m);
    }
}
