namespace TypeLox;

// TODO: There should be a 1:1 between error diagnostics and LoxExceptions
public sealed record class Diagnostic(
    DiagnosticKind Kind,
    SourceRange Location,
    string Message
) : IDisplay {
    public bool IsOk { get; } = Kind.GetIsOK();

    private const char POWERLINE = '\uE0B0';
    public void Format(IFormatter f) {
        f.Wrap(Kind.GetColor(), delegate {
            f.Wrap(AnsiStyle.Inverted, delegate {
                f.Append(' ');
                f.Append(Kind.ToString().ToUpperInvariant());
                f.Append(' ');
            });
            f.Append(POWERLINE);
            f.Append(' ');
            Location.FormatHeader(f);
            Location.FormatPreview(f);
            f.AppendLine();
            f.Append(Message);
        });
    }

    public LoxException ToException() => new(Location, Message);
    public override string ToString() => this.FormatToString();
}
