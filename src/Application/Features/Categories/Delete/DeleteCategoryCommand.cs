using FinanceTracker.Application.Abstractions.Messaging;

namespace FinanceTracker.Application.Features.Categories.Delete;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
