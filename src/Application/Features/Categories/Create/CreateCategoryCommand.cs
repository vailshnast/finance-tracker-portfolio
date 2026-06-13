using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Categories.Create;

public sealed record CreateCategoryCommand(string Name, CategoryType Type) : ICommand<Result<CreateCategoryResponse>>;

public sealed record CreateCategoryResponse(Guid Id, string Name, CategoryType Type);
