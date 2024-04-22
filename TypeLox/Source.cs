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
public interface ISourceRepository {
    public Source CreateSourceFromFile(Uri uri);
    public Source CreateSourceFromSnippet(string code);
}

/// <summary>
/// Source repo backed by the file system.
/// </summary>
public class FileSystemSourceRepository : ISourceRepository {
    public Source CreateSourceFromFile(Uri uri) {
        // TODO: This is where CompilerHost comes into play
        var code = File.ReadAllText(uri.ToFilePath(), Encoding.UTF8);
        return new Source(uri, code);
    }

    private int id;
    public Source CreateSourceFromSnippet(string code) {
        var uri = new Uri($"lox:/snippet/{id++}");
        return new Source(uri, code);
    }
}
