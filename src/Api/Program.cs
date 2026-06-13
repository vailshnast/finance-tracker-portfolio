using FinanceTracker.Api.Endpoints;
using FinanceTracker.Api.Extensions;
using FinanceTracker.Application;
using FinanceTracker.ServiceDefaults;
using Serilog;
using FinanceTracker.Infrastructure;
using FinanceTracker.Infrastructure.Persistence;

// Serilog bootstrap logger for early logging during app startup
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddAspireServiceDefaults();

    builder.Host.UseSerilog((context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration));

    // Aspire-managed PostgreSQL and Redis
    builder.AddNpgsqlDbContext<AppDbContext>("db");
    builder.AddRedisDistributedCache("redis-cache");

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddOpenApiWithJwtSecurity();

    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
        app.MapOpenApiEndpoints();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSerilogRequestLogging();

    app.MapIdentityEndpoints();
    app.MapCategoryEndpoints();
    app.MapBudgetEndpoints();
    app.MapTransactionEndpoints();
    app.MapSummaryEndpoints();
    app.MapAspireDefaultEndpoints();

    if (app.Environment.IsDevelopment())
        await AppDbSeeder.SeedAsync(app.Services);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
