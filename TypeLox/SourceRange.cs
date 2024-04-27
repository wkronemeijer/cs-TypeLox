namespace TypeLox;

/// <summary>
/// Tracks a substring of a <see cref="TypeLox.Source"/> object.
/// </summary>
public sealed record SourceRange(
    Source Source,
    int Start,
    int End
) : IDisplay {
    public string GetLexeme() => Source.Code[Start..End];

    public void FormatHeader(IFormatter f) {
        var coords = Source.Code.GetLnCol(Start);
        f.Append(Source.Uri.ToString());
        f.Append($":{coords.Ln}:{coords.Col}");
    }

    public void FormatPreview(IFormatter f) {
        f.IncludePreview(Source.Code, Start, End);
    }

    public void Format(IFormatter f) => FormatHeader(f);
    public override string ToString() => this.FormatToString();
}
