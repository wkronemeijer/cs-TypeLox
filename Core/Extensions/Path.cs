namespace Core;

using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Adds extensions equivalents to Node's <c>pathToFileUrl</c> and <c>fileUrlToPath</c>.
/// </summary>
public static partial class PathExtensions {
    public static string ToFilePath(this Uri uri) {
        Assert(uri.IsFile, () => $"{uri} is not a file uri");
        return uri.LocalPath;
    }

    [GeneratedRegex("[^A-Za-z:/]")]
    private static partial Regex Pattern();

    // How do other people not run into this issue?
    // File URLs as normalized paths is one of the most useful things
    public static Uri ToFileUri(this string filePath) {
        var normalizedAbsoluteFilePath = Path.GetFullPath(filePath).Replace('\\', '/');
        var urlPath = Pattern().Replace(normalizedAbsoluteFilePath, match => {
            Assert(match.Length == 1);
            return Uri.HexEscape(match.Value[0]);
        });
        return new("file:///" + urlPath, UriKind.Absolute);
    }
}
