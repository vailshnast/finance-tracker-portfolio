using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.DbSetPlaceholder.Create;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.DbSetPlaceholder.Create;

public sealed class CreateFeatureTemplateCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_With_Created_FeatureTemplate()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new CreateFeatureTemplateCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new CreateFeatureTemplateCommand();

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        dbContext.DbSetPlaceholder.Should().HaveCount(1);
    }
}
