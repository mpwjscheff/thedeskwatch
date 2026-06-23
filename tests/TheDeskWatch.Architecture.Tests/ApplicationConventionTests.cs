using NetArchTest.Rules;
using System.Reflection;

namespace TheDeskWatch.Architecture.Tests;

/// <summary>
/// Enforces the Application layer naming and visibility conventions from CLAUDE.md:
///
///   Commands   (Features.*.Commands, non-handler)  → public sealed class
///   Handlers   (Features.*.Commands, *Handler)     → internal sealed class
///   Queries    (Features.*.Queries,  non-handler)  → public class (NOT sealed)
///   Query resp (Features.*.Queries,  *Response)    → public sealed record
///   Q.Handlers (Features.*.Queries,  *Handler)     → public sealed class
/// </summary>
public class ApplicationConventionTests
{
    private static readonly Assembly ApplicationAssembly = typeof(TheDeskWatch.Application.Features.Colleagues.Queries.GetColleaguesQuery).Assembly;

    private const string CommandsNamespacePattern = @"TheDeskWatch\.Application\.Features\..+\.Commands";
    private const string QueriesNamespacePattern = @"TheDeskWatch\.Application\.Features\..+\.Queries";

    [Fact]
    public void Commands_ArePublicAndSealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(CommandsNamespacePattern)
            .And().DoNotHaveNameEndingWith("Handler")
            .Should().BePublic()
            .And().BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    [Fact]
    public void CommandHandlers_AreInternalAndSealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(CommandsNamespacePattern)
            .And().HaveNameEndingWith("Handler")
            .Should().NotBePublic()
            .And().BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    [Fact]
    public void Queries_ArePublicAndNotSealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(QueriesNamespacePattern)
            .And().DoNotHaveNameEndingWith("Handler")
            .And().DoNotHaveNameEndingWith("Response")
            .And().DoNotHaveNameEndingWith("Dto")
            .Should().BePublic()
            .And().NotBeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    [Fact]
    public void QueryHandlers_ArePublicAndSealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(QueriesNamespacePattern)
            .And().HaveNameEndingWith("Handler")
            .Should().BePublic()
            .And().BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    [Fact]
    public void QueryResponses_ArePublicAndSealed()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching(QueriesNamespacePattern)
            .And().HaveNameEndingWith("Response")
            .Should().BePublic()
            .And().BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    private static string FormatViolations(TestResult result) =>
        $"Violating types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}";
}
