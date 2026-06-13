using System.Security.Claims;
using FluentAssertions;
using FinanceTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FinanceTracker.Application.UnitTests.Infrastructure;

public sealed class AuditInterceptorTests
{
    private static AppDbContext CreateDbContext(string? userId)
    {
        var httpContext = new DefaultHttpContext();
        if (userId is not null)
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
        }

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var interceptor = new AuditInterceptor(httpContextAccessor);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Set_CreatedAt_And_CreatedBy_On_Added_Entity()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = CreateDbContext(userId);

        var category = new Category { Name = "Test", UserId = userId };
        dbContext.Categories.Add(category);

        // Act
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        category.CreatedAt.Should().NotBe(default);
        category.CreatedBy.Should().Be(userId);
        category.LastModifiedAt.Should().BeNull();
        category.LastModifiedBy.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Set_LastModifiedAt_And_LastModifiedBy_On_Modified_Entity()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = CreateDbContext(userId);

        var category = new Category { Name = "Test", UserId = userId };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        category.Name = "Updated";
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        category.LastModifiedAt.Should().NotBeNull();
        category.LastModifiedBy.Should().Be(userId);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Not_Overwrite_CreatedAt_On_Modified_Entity()
    {
        // Arrange
        var userId = "test-user-id";
        await using var dbContext = CreateDbContext(userId);

        var category = new Category { Name = "Test", UserId = userId };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var originalCreatedAt = category.CreatedAt;
        var originalCreatedBy = category.CreatedBy;

        // Act
        category.Name = "Updated";
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        category.CreatedAt.Should().Be(originalCreatedAt);
        category.CreatedBy.Should().Be(originalCreatedBy);
    }
}