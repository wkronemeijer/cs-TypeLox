namespace TypeLox;

/// <summary>
/// Combines a Uri and the actual text content. 
/// Abstracts over code from files, and code snippets passed directly.
/// </summary>
public record class Source(Uri Uri, string Code) {
    public override string ToString() => Uri.ToString();
}
