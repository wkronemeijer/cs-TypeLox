namespace TypeLox;

using System.IO;

/// <summary>
/// Links Uri with contents. Useful so it can be swapped out during testing.
/// </summary>
public interface ICompilerHost {
    public CompilerOptions Options { get; }

    // Method, as to acknowledge this result can change at any time,
    // not just as a consequence of calling methods on this object.
    public Uri GetCurrentDirectory();
    public Source CreateSourceFromSnippet(string code);
    public Source ReadFile(Uri uri);
    public List<Source> ReadDirectory(Uri uri);

    public void WriteLine(string s);
    public void WriteLine() => WriteLine("");
}

/// <summary>
/// Compiler host backed by the file system and stdout.
/// </summary>
public class RealCompilerHost(CompilerOptions options) : ICompilerHost {
    public CompilerOptions Options => options;

    public Uri GetCurrentDirectory() => Environment.CurrentDirectory.ToFileUri();

    public Source ReadFile(Uri uri) {
        // TODO: This is where CompilerHost comes into play
        var code = File.ReadAllText(uri.ToFilePath(), Encoding.UTF8);
        return new Source(uri, code);
    }

    public List<Source> ReadDirectory(Uri uri) {
        throw new NotImplementedException();
    }

    public Source CreateSourceFromSnippet(string code) {
        var uri = new Uri($"lox:/snippet/{code.GetHashCode():x8}");
        return new Source(uri, code);
    }

    public void WriteLine(string s) => Console.WriteLine(s);
    public void WriteLine() => Console.WriteLine();
}
