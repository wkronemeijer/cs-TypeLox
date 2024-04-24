namespace TypeLox;

using System.IO;

/// <summary>
/// Combines a Uri and the actual text content. 
/// Abstracts over code from files, and code snippets passed directly.
/// </summary>
public record class Source(Uri Uri, string Code) {
    public override string ToString() {
        return Uri.ToString();
    }
}

/// <summary>
/// Tracks a substring of a <see cref="TypeLox.Source"/> object.
/// </summary>
public record class SourceRange(Source Source, int Start, int End) {
    private (int Line, int Column) ComputeLnCol() {
        // TODO: Calculate line numbers
        // TS has getTextPreview, take a gander at that
        throw new NotImplementedException();
    }

    public string GetLexeme() {
        return Source.Code[Start..End];
    }

    public override string ToString() {
        return $"{Source} ({Start}..<{End})";
    }
}

/// <summary>
/// Links Uri with contents. Useful so it can be swapped out during testing.
/// </summary>
public interface ICompilerHost {
    public Source CreateSourceFromFile(Uri uri);
    public Source CreateSourceFromSnippet(string code);

    // TODO: Add:
    // public Uri GetCurrentDirectory();

    public void WriteLine(string s);
    public void WriteLine() => WriteLine("");
}

/// <summary>
/// Compiler host backed by the file system and stdout.
/// </summary>
public class RealCompilerHost : ICompilerHost {
    public Source CreateSourceFromFile(Uri uri) {
        // TODO: This is where CompilerHost comes into play
        var code = File.ReadAllText(uri.ToFilePath(), Encoding.UTF8);
        return new Source(uri, code);
    }

    public Source CreateSourceFromSnippet(string code) {
        var uri = new Uri($"lox:/snippet/{code.GetHashCode():x8}");
        return new Source(uri, code);
    }

    public void WriteLine(string s) => Console.WriteLine(s);
    public void WriteLine() => Console.WriteLine();
}
