using System.Reflection;
using TheDeskWatch.Application;

namespace TheDeskWatch.Application.Tests;

/// <summary>
/// Enforces the CLAUDE.md rule that every command, query, and feature service in the
/// Application layer has a corresponding test type in this project.
///
/// A "matching test" is any type in this test assembly whose name starts with the
/// source type's name and ends with "Tests" — e.g. SaveSettingsCommand is satisfied by
/// SaveSettingsCommandTests or SaveSettingsCommandHandlerTests. Folder mirroring
/// (CLAUDE.md) is a stronger convention reviewed by hand; this checks existence so a
/// command/query/service can never ship with zero coverage.
///
/// Passes cleanly when no such source types exist yet.
/// </summary>
public class TestCoverageTests
{
    // Update this anchor if Class1 is replaced with a real Application type.
    private static readonly Assembly ApplicationAssembly = typeof(Class1).Assembly;
    private static readonly Assembly TestAssembly = typeof(TestCoverageTests).Assembly;

    [Fact]
    public void EveryCommand_HasATest() =>
        AssertCovered(FindSourceTypes(
            namespaceSuffix: ".Commands",
            predicate: t => !t.Name.EndsWith("Handler", StringComparison.Ordinal)));

    [Fact]
    public void EveryQuery_HasATest() =>
        AssertCovered(FindSourceTypes(
            namespaceSuffix: ".Queries",
            predicate: t => !t.Name.EndsWith("Handler", StringComparison.Ordinal)
                            && !t.Name.EndsWith("Response", StringComparison.Ordinal)));

    [Fact]
    public void EveryFeatureService_HasATest() =>
        AssertCovered(FindSourceTypes(
            namespaceSuffix: ".Services",
            // Concrete service implementations only — interfaces aren't unit-tested directly.
            predicate: t => t is { IsClass: true, IsAbstract: false }));

    private static IEnumerable<Type> FindSourceTypes(string namespaceSuffix, Func<Type, bool> predicate) =>
        ApplicationAssembly.GetTypes()
            .Where(t => t is { IsPublic: true, IsClass: true } || (t.IsNotPublic && t.IsClass))
            .Where(t => (t.Namespace ?? string.Empty)
                .Contains(".Features.", StringComparison.Ordinal))
            .Where(t => (t.Namespace ?? string.Empty)
                .EndsWith(namespaceSuffix, StringComparison.Ordinal))
            .Where(t => !t.Name.Contains('<', StringComparison.Ordinal)) // skip compiler-generated
            .Where(predicate);

    private static void AssertCovered(IEnumerable<Type> sourceTypes)
    {
        var testTypeNames = TestAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Tests", StringComparison.Ordinal))
            .Select(t => t.Name)
            .ToArray();

        var uncovered = sourceTypes
            .Where(src => !testTypeNames.Any(name =>
                name.StartsWith(src.Name, StringComparison.Ordinal)))
            .Select(src => src.FullName)
            .ToArray();

        Assert.True(
            uncovered.Length == 0,
            $"Application types missing a matching *Tests type: {string.Join(", ", uncovered)}");
    }
}
