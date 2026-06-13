namespace FinanceTracker.Application.Features.DbSetPlaceholder.Update;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record UpdateFeatureTemplateCommand(Guid Id) : ICommand;
