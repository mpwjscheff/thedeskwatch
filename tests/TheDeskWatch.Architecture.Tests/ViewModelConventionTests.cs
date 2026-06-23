using System.IO;
using System.Text.RegularExpressions;

namespace TheDeskWatch.Architecture.Tests;

/// <summary>
/// Heuristic source scan of the presentation ViewModels enforcing the CLAUDE.md rule
/// that ViewModels depend only on the Application layer — never on Persistence, Domain,
/// or a repository type. NetArchTest cannot express this because the test project cannot
/// reference the MAUI MobileApp assembly (net10.0-android/ios), so we scan the .cs text.
/// </summary>
public class ViewModelConventionTests
{
    private static IEnumerable<string> ViewModelFiles() =>
        SourceTree.MobileAppFiles("*.cs")
            .Where(p =>
            {
                var rel = SourceTree.Rel(p).Replace('\\', '/');
                return rel.Contains("/ViewModels/") && rel.EndsWith(".cs", StringComparison.Ordinal);
            });

    [Fact]
    public void ViewModels_DoNotDependOnPersistenceOrDomain()
    {
        var forbidden = new Regex(
            @"using\s+TheDeskWatch\.(Persistence|Domain)\b");

        var violations = new List<string>();
        foreach (var file in ViewModelFiles())
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (forbidden.IsMatch(lines[i]))
                    violations.Add($"{SourceTree.Rel(file)}:{i + 1}  {lines[i].Trim()}");
            }
        }

        Assert.True(violations.Count == 0,
            "ViewModels may depend only on the Application layer — not Persistence or Domain "
            + "(CLAUDE.md / app-logic):\n" + string.Join("\n", violations));
    }
}
