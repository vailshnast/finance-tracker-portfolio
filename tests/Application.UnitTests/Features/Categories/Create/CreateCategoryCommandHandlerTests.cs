using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Categories.Create;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.Categories.Create;

public sealed class CreateCategoryCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_With_Created_Category()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new CreateCategoryCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateCategoryCommand("test-category", CategoryType.Expense);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        dbContext.Categories.Should().HaveCount(1);
        result.Value.Name.Should().Be("test-category");
    }
}
