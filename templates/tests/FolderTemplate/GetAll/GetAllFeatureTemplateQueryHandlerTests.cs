using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.DbSetPlaceholder.GetAll;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.DbSetPlaceholder.GetAll;

public sealed class GetAllFeatureTemplateQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Paged_Results()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.DbSetPlaceholder.AddRange(
            new EntityPlaceholder { UserId = userId },
            new EntityPlaceholder { UserId = userId },
            new EntityPlaceholder { UserId = userId });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetAllFeatureTemplateQueryHandler(dbContext, currentUser, new PassthroughHybridCache());
        var query = new GetAllFeatureTemplateQuery(Page: 1, PageSize: 10);

        // Act
        var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(3);
        result.Value.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Empty_When_No_FeatureTemplates()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new GetAllFeatureTemplateQueryHandler(dbContext, currentUser, new PassthroughHybridCache());
        var query = new GetAllFeatureTemplateQuery(Page: 1, PageSize: 10);

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
        dbContext.DbSetPlaceholder.AddRange(
            new EntityPlaceholder { UserId = userId },
            new EntityPlaceholder { UserId = userId },
            new EntityPlaceholder { UserId = userId },
            new EntityPlaceholder { UserId = userId },
            new EntityPlaceholder { UserId = userId });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new GetAllFeatureTemplateQueryHandler(dbContext, currentUser, new PassthroughHybridCache());
        var query = new GetAllFeatureTemplateQuery(Page: 2, PageSize: 2);

        // Act
        var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(5);
    }
}
