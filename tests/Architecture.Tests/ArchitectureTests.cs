using System.Reflection;
using FinanceTracker.Api.Endpoints;
using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Infrastructure;
using FluentAssertions;
using FluentValidation;
using NetArchTest.Rules;

namespace FinanceTracker.Architecture.Tests;

public sealed class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(FinanceTracker.Domain.Common.BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(FinanceTracker.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(TransactionEndpoints).Assembly;

    private const string DomainNamespace = "FinanceTracker.Domain";
    private const string ApplicationNamespace = "FinanceTracker.Application";
    private const string InfrastructureNamespace = "FinanceTracker.Infrastructure";
    private const string ApiNamespace = "FinanceTracker.Api";

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_Should_Be_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        var failing = result.FailingTypes?.Select(t => t.Name) ?? [];
        result.IsSuccessful.Should().BeTrue($"these handlers are not sealed: {string.Join(", ", failing)}");
    }

    [Fact]
    public void Validators_Should_Be_Sealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Validator")
            .Should()
            .BeSealed()
            .GetResult();

        var failing = result.FailingTypes?.Select(t => t.Name) ?? [];
        result.IsSuccessful.Should().BeTrue($"these validators are not sealed: {string.Join(", ", failing)}");
    }

    [Fact]
    public void Domain_Entities_Should_Be_Sealed()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("FinanceTracker.Domain.Entities")
            .Should()
            .BeSealed()
            .GetResult();

        var failing = result.FailingTypes?.Select(t => t.Name) ?? [];
        result.IsSuccessful.Should().BeTrue($"these domain entities are not sealed: {string.Join(", ", failing)}");
    }

    [Fact]
    public void Every_Handler_Interface_Used_In_Endpoints_Has_A_Concrete_Implementation()
    {
        var handlerOpenTypes = new HashSet<Type> { typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) };

        var usedHandlerInterfaces = ApiAssembly
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            .SelectMany(m => m.GetParameters())
            .Select(p => p.ParameterType)
            .Where(t => t.IsGenericType && handlerOpenTypes.Contains(t.GetGenericTypeDefinition()))
            .ToHashSet();

        var implementedHandlerInterfaces = ApplicationAssembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && handlerOpenTypes.Contains(i.GetGenericTypeDefinition()))
            .ToHashSet();

        var missing = usedHandlerInterfaces.Except(implementedHandlerInterfaces).ToList();

        missing.Should().BeEmpty(
            $"because these handler interfaces are injected in endpoints but have no concrete implementation: " +
            $"{string.Join(", ", missing.Select(t => t.ToString()))}");
    }

    [Fact]
    public void Every_Command_And_Query_Has_A_Handler()
    {
        var commandOpenType = typeof(ICommand<>);
        var queryOpenType = typeof(IQuery<>);
        var commandHandlerOpenType = typeof(ICommandHandler<,>);
        var queryHandlerOpenType = typeof(IQueryHandler<,>);
        var handlerOpenTypes = new HashSet<Type> { commandHandlerOpenType, queryHandlerOpenType };

        var implementedHandlerInterfaces = ApplicationAssembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && handlerOpenTypes.Contains(i.GetGenericTypeDefinition()))
            .ToHashSet();

        var missing = new List<string>();

        foreach (var type in ApplicationAssembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            foreach (var iface in type.GetInterfaces().Where(i => i.IsGenericType))
            {
                var def = iface.GetGenericTypeDefinition();
                var responseType = iface.GetGenericArguments()[0];

                Type expectedHandler;
                if (def == commandOpenType)
                    expectedHandler = commandHandlerOpenType.MakeGenericType(type, responseType);
                else if (def == queryOpenType)
                    expectedHandler = queryHandlerOpenType.MakeGenericType(type, responseType);
                else
                    continue;

                if (!implementedHandlerInterfaces.Contains(expectedHandler))
                    missing.Add($"{type.Name} → {expectedHandler.Name}<{type.Name}, {responseType.Name}>");
            }
        }

        missing.Should().BeEmpty(
            $"because every command and query must have a matching handler:\n{string.Join("\n", missing)}");
    }

    [Fact]
    public void Every_ValidationFilter_Used_In_Endpoints_Has_A_Validator()
    {
        var validationFilterOpenType = typeof(ValidationFilter<>);
        var iValidatorOpenType = typeof(IValidator<>);
        var module = ApiAssembly.ManifestModule;

        // Scan Api method bodies for AddEndpointFilter<ValidationFilter<T>>() calls.
        // Generic method calls emit MethodSpec tokens (table 0x2B), identifiable by the
        // high byte of the 4-byte token that follows a call (0x28) or callvirt (0x6F) opcode.
        var filteredTypes = new HashSet<Type>();

        foreach (var type in ApiAssembly.GetTypes())
        {
            var typeArgs = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var il = method.GetMethodBody()?.GetILAsByteArray();
                if (il is null) continue;

                var methodArgs = method.IsGenericMethod ? method.GetGenericArguments() : Type.EmptyTypes;

                for (var i = 0; i < il.Length - 4; i++)
                {
                    if ((il[i] != 0x28 && il[i] != 0x6F) || il[i + 4] != 0x2B) continue;

                    var token = BitConverter.ToInt32(il, i + 1);
                    try
                    {
                        if (module.ResolveMethod(token, typeArgs, methodArgs) is not MethodInfo { IsGenericMethod: true, Name: "AddEndpointFilter" } mi) continue;

                        var filterType = mi.GetGenericArguments()[0];
                        if (filterType.IsGenericType && filterType.GetGenericTypeDefinition() == validationFilterOpenType)
                            filteredTypes.Add(filterType.GetGenericArguments()[0]);
                    }
                    catch { }
                }
            }
        }

        var validatedTypes = ApplicationAssembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iValidatorOpenType)
            .Select(i => i.GetGenericArguments()[0])
            .ToHashSet();

        var missing = filteredTypes.Except(validatedTypes).ToList();
        missing.Should().BeEmpty(
            $"because these types use ValidationFilter in endpoints but have no IValidator<T>: " +
            $"{string.Join(", ", missing.Select(t => t.Name))}");
    }

    [Fact]
    public void Every_Validator_Validates_A_Type_From_The_Application_Assembly()
    {
        var iValidatorOpenType = typeof(IValidator<>);
        var applicationTypes = ApplicationAssembly.GetTypes().ToHashSet();

        var orphaned = ApplicationAssembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iValidatorOpenType)
                .Select(i => new { ValidatorType = t, ValidatedType = i.GetGenericArguments()[0] }))
            .Where(v => !applicationTypes.Contains(v.ValidatedType))
            .Select(v => $"{v.ValidatorType.Name} → {v.ValidatedType.Name}")
            .ToList();

        orphaned.Should().BeEmpty(
            $"because these validators validate types not found in Application: " +
            $"{string.Join(", ", orphaned)}");
    }
}
