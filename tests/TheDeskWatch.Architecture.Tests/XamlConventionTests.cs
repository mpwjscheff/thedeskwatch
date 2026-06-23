using System.IO;
using System.Text.RegularExpressions;

namespace TheDeskWatch.Architecture.Tests;

/// <summary>
/// Heuristic, text-based scans of the MobileApp XAML enforcing the CLAUDE.md UI rules
/// that no analyzer covers. These match on file text (not a parsed tree), so they are
/// deliberately conservative: Resources/Styles is the one place literals are allowed
/// and is excluded. Tune the allowlists below if a legitimate case trips a scan.
/// </summary>
public class XamlConventionTests
{
    private static IEnumerable<string> PageAndViewXaml() =>
        SourceTree.MobileAppFiles("*.xaml")
            // Resources/Styles is the single source of truth for literals — exempt it.
            .Where(p => !SourceTree.Rel(p).Replace('\\', '/').Contains("/Resources/Styles/"));

    [Fact]
    public void Xaml_HasNoInlineHexColors()
    {
        var hex = new Regex("#(?:[0-9a-fA-F]{8}|[0-9a-fA-F]{6}|[0-9a-fA-F]{3,4})\\b");

        var violations = ScanLines(PageAndViewXaml(), (line, _) => hex.IsMatch(line));

        Assert.True(violations.Count == 0,
            "Inline hex colors are not allowed outside Resources/Styles — use a "
            + "{StaticResource} color key (CLAUDE.md, UI Styling Rules):\n" + Join(violations));
    }

    [Fact]
    public void Xaml_HasNoInlineSizesOrSpacing()
    {
        // Styling attributes that must reference a token rather than a literal number.
        var attr = new Regex(
            "\\b(Margin|Padding|Spacing|RowSpacing|ColumnSpacing|FontSize|CornerRadius|"
            + "WidthRequest|HeightRequest)\\s*=\\s*\"([^\"]*)\"");

        var violations = ScanLines(PageAndViewXaml(), (line, _) =>
        {
            foreach (Match m in attr.Matches(line))
            {
                var value = m.Groups[2].Value.Trim();
                if (IsTokenOrZero(value))
                    continue;
                return true;
            }
            return false;
        });

        Assert.True(violations.Count == 0,
            "Inline sizes/spacing are not allowed — reference a token from Styles.xaml "
            + "via {StaticResource} (CLAUDE.md, UI Styling Rules):\n" + Join(violations));
    }

    [Fact]
    public void Xaml_BindingsDeclareCompiledDataType()
    {
        // {Binding ...} requires an x:DataType in scope; {TemplateBinding}/{StaticResource} do not.
        var binding = new Regex("\\{\\s*Binding[\\s}]");

        var violations = PageAndViewXaml()
            .Where(p => binding.IsMatch(File.ReadAllText(p)))
            .Where(p => !File.ReadAllText(p).Contains("x:DataType", StringComparison.Ordinal))
            .Select(p => SourceTree.Rel(p) + " — uses {Binding} but declares no x:DataType")
            .ToList();

        Assert.True(violations.Count == 0,
            "Every binding must be compiled with x:DataType (CLAUDE.md / ui-xaml):\n"
            + string.Join("\n", violations));
    }

    private static bool IsTokenOrZero(string value) =>
        value.StartsWith('{')                       // {StaticResource ...}, {AppThemeBinding ...}
        || value.Length == 0
        // All numeric components are zero (e.g. "0", "0,0,0,0") — harmless, allowed.
        || Regex.IsMatch(value, "^[0,\\s.]*$");

    private static List<string> ScanLines(IEnumerable<string> files, Func<string, int, bool> isViolation)
    {
        var result = new List<string>();
        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (isViolation(lines[i], i))
                    result.Add($"{SourceTree.Rel(file)}:{i + 1}  {lines[i].Trim()}");
            }
        }
        return result;
    }

    private static string Join(IEnumerable<string> violations) => string.Join("\n", violations);
}
