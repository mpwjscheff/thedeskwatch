using NetArchTest.Rules;
using System.Reflection;

namespace TheDeskWatch.Architecture.Tests;

/// <summary>
/// Enforces the layering rules documented in CLAUDE.md.
///
/// Allowed references:
///   MobileApp          → Application, MobileApp.Contracts
///   Application        → MobileApp.Contracts
///   Persistence        → Domain, MobileApp.Contracts
///   Domain             → (none)
///   MobileApp.Contracts → (none)
///
/// MobileApp ViewModel rules (ViewModels must not reference Persistence or Domain)
/// are NOT tested here because MobileApp targets net10.0-android/ios only and
/// cannot be referenced from this net10.0 test project.
/// </summary>
public class LayerDependencyTests
{
    // Update these anchors if the placeholder types are replaced.
    private static readonly Assembly ApplicationAssembly = typeof(TheDeskWatch.Application.Class1).Assembly;
    private static readonly Assembly DomainAssembly = typeof(TheDeskWatch.Domain.Class1).Assembly;
    private static readonly Assembly PersistenceAssembly = typeof(TheDeskWatch.Persistence.Class1).Assembly;

    [Fact]
    public void Application_DoesNotDependOn_Persistence()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOn("TheDeskWatch.Persistence")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    [Fact]
    public void Application_DoesNotDependOn_Domain()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOn("TheDeskWatch.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    [Fact]
    public void Persistence_DoesNotDependOn_Application()
    {
        var result = Types.InAssembly(PersistenceAssembly)
            .ShouldNot().HaveDependencyOn("TheDeskWatch.Application")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    [Theory]
    [InlineData("TheDeskWatch.Application")]
    [InlineData("TheDeskWatch.Persistence")]
    [InlineData("TheDeskWatch.MobileApp")]
    [InlineData("TheDeskWatch.MobileApp.Contracts")]
    public void Domain_DoesNotDependOnAnyOtherLayer(string ns)
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot().HaveDependencyOn(ns)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatViolations(result));
    }

    private static string FormatViolations(TestResult result) =>
        $"Violating types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}";
}
