namespace FinanceTracker.Application.Features.DbSetPlaceholder.Delete;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record DeleteFeatureTemplateCommand(Guid Id) : ICommand;
