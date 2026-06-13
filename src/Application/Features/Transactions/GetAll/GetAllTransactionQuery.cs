namespace FinanceTracker.Application.Features.Transactions.GetAll;

using Application.Abstractions.Messaging;
using Application.Features.Transactions.Get;
using Domain.Common;

public sealed record GetAllTransactionQuery(int Page = 1, int PageSize = 10) : IQuery<Result<PagedResult<TransactionDetailResponse>>>;
