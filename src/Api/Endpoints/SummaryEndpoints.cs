using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Application.Features.Summary.BudgetStatus;
using FinanceTracker.Application.Features.Summary.Monthly;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Api.Endpoints;

public static class SummaryEndpoints
{
    public static void MapSummaryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/summary")
            .WithTags("Summary")
            .RequireAuthorization();

        group.MapGet("/monthly", GetMonthlySummary)
            .WithName("GetSummary")
            .WithSummary("Get monthly summary of income and expenses")
            .WithDescription("Returns a summary of total income, total expenses, and net balance for the specified month and year. " +
                             "Use `month` and `year` query parameters to specify the period. " +
                             "Defaults to the current month and year if not provided.")
            .Produces<GetMonthlySummaryResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/budgets", GetBudgetSummary)
            .WithName("GetBudgets")
            .WithSummary("Returns a list of budgets for current month and year.")
            .WithDescription("Returns a list of budgets for the current month and year, including the amount spent and remaining for each budget category.")
            .Produces<GetBudgetSummaryResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

    }

    private static async Task<IResult> GetMonthlySummary(
        int? month, int? year,
        IQueryHandler<GetMonthlySummaryQuery, Result<GetMonthlySummaryResponse>> handler,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var result = await handler.HandleAsync(new GetMonthlySummaryQuery(month ?? now.Month, year ?? now.Year), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetBudgetSummary(
        IQueryHandler<GetBudgetSummaryQuery, Result<GetBudgetSummaryResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetBudgetSummaryQuery(), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}
