namespace TypeLox;

public sealed record class Diagnostic(
    DiagnosticKind Kind,
    SourceRange Location,
    string Message
) : IDisplay {
    public bool IsOk { get; } = Kind.GetIsOK();

    public void Format(IFormatter f) => f.Wrap(Kind.GetColor(), () => {
        f.Wrap(AnsiStyle.Inverted, () => {
            f.Append(' ');
            f.Append(Kind.ToString().ToUpperInvariant());
            f.Append(' ');
        });
        f.Append('\uE0B0');
        f.Append(' ');
        Location.FormatHeader(f);
        Location.FormatPreview(f);
        f.AppendLine();
        f.Append(Message);
    });

    public override string ToString() => this.FormatToString();
}
