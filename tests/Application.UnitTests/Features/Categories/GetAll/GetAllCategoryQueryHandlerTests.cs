using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Categories.GetAll;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Categories.GetAll;

public sealed class GetAllCategoryQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Paged_Results()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Categories.AddRange(
            new Category { UserId = userId, Name = "category-1" },
            new Category { UserId = userId, Name = "category-2" },
            new Category { UserId = userId, Name = "category-3" });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetAllCategoryQueryHandler(dbContext, currentUser, new PassthroughHybridCache());
        var query = new GetAllCategoryQuery(Page: 1, PageSize: 10);

        // Act
        var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(3);
        result.Value.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Empty_When_No_Categories()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new GetAllCategoryQueryHandler(dbContext, currentUser, new PassthroughHybridCache());
        var query = new GetAllCategoryQuery(Page: 1, PageSize: 10);

        // Act
        var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_Should_Respect_Pagination()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Categories.AddRange(
            new Category { UserId = userId, Name = "category-1" },
            new Category { UserId = userId, Name = "category-2" },
            new Category { UserId = userId, Name = "category-3" },
            new Category { UserId = userId, Name = "category-4" },
            new Category { UserId = userId, Name = "category-5" });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetAllCategoryQueryHandler(dbContext, currentUser, new PassthroughHybridCache());
        var query = new GetAllCategoryQuery(Page: 2, PageSize: 2);

        // Act
        var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(5);
    }
}
