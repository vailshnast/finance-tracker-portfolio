using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Abstractions.Messaging;

namespace FinanceTracker.Application.Features.Identity.Login;

using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<TokenResponse>>;
