namespace FinanceTracker.Application.Abstractions.Messaging;

using Domain.Common;

public interface ICommand : ICommand<Result>;

public interface ICommand<TResponse>;
