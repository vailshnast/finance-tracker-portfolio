using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Categories.Update;

public sealed record UpdateCategoryCommand(Guid Id, string Name, CategoryType Type) : ICommand;
