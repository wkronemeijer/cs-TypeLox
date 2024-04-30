namespace TypeLox;

/// <summary>
/// Combines a Uri and the actual text content. 
/// Abstracts over code from files, and code snippets passed directly.
/// </summary>
public record class Source(Uri Uri, string Code) {
    public override string ToString() => Uri.ToString();

    /// <summary>
    /// Synthesizes a "lox:" URI for a piece of code.
    /// </summary>
    public static Source FromString(string code) {
        var uri = new Uri($"lox:/snippet/{code.GetHashCode():x8}");
        return new Source(uri, code);
    }
}
