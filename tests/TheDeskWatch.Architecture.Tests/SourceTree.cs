using System.IO;

namespace TheDeskWatch.Architecture.Tests;

/// <summary>
/// Locates source files on disk for the heuristic, text-based convention scans
/// (XAML and ViewModel rules that NetArchTest cannot express because the test
/// project cannot reference the MAUI MobileApp assembly).
/// </summary>
internal static class SourceTree
{
    /// <summary>Repo root — the directory containing TheDeskWatch.slnx.</summary>
    public static string RepoRoot { get; } = FindRepoRoot();

    /// <summary>The MobileApp (presentation) project directory.</summary>
    public static string MobileAppDir { get; } =
        Path.Combine(RepoRoot, "src", "TheDeskWatch.MobileApp");

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TheDeskWatch.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate TheDeskWatch.slnx walking up from " + AppContext.BaseDirectory);
    }

    /// <summary>
    /// All files matching <paramref name="pattern"/> under the MobileApp project,
    /// excluding build output (bin/obj). Empty if the directory does not exist yet.
    /// </summary>
    public static IEnumerable<string> MobileAppFiles(string pattern)
    {
        if (!Directory.Exists(MobileAppDir))
            return [];

        return Directory.EnumerateFiles(MobileAppDir, pattern, SearchOption.AllDirectories)
            .Where(p => !IsBuildOutput(p));
    }

    private static bool IsBuildOutput(string path)
    {
        var rel = Path.GetRelativePath(MobileAppDir, path);
        return rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(seg => seg is "bin" or "obj");
    }

    /// <summary>Path relative to repo root, for readable failure messages.</summary>
    public static string Rel(string path) => Path.GetRelativePath(RepoRoot, path);
}
