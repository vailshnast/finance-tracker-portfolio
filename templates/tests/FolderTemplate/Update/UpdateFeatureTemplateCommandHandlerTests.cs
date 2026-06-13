using FluentAssertions;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.DbSetPlaceholder.Update;
using FinanceTracker.Domain.Common;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Features.DbSetPlaceholder.Update;

public sealed class UpdateFeatureTemplateCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Success_When_FeatureTemplate_Updated()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = TestDbContextFactory.Create();
        var entity = new EntityPlaceholder { UserId = userId };
        dbContext.DbSetPlaceholder.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId);
        var handler = new UpdateFeatureTemplateCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new UpdateFeatureTemplateCommand(entity.Id);

        // Act
        await handler.HandleAsync(command, TestContext.Current.CancellationToken);
        var updatedEntity = await dbContext.DbSetPlaceholder.FindAsync([entity.Id], TestContext.Current.CancellationToken);

        // Assert
        updatedEntity.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user-id");
        var handler = new UpdateFeatureTemplateCommandHandler(dbContext, currentUser, new PassthroughHybridCache());
        var command = new UpdateFeatureTemplateCommand(Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
