namespace TypeLox;

using System.IO;

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
