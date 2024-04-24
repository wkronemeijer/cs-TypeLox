namespace TypeLox;

using System.Diagnostics;

using static DiagnosticKind;

public enum DiagnosticKind {
    Info,
    Warning,
    Error,
}

public static class DiagnosticKindMethods {
    public static bool GetIsOK(this DiagnosticKind self) => self switch {
        Error => false,
        _ => true,
    };

    public static AnsiSgrPair GetColor(this DiagnosticKind self) => self switch {
        Info => AnsiSgrPair.BlueLetter,
        Warning => AnsiSgrPair.BrightYellowLetter,
        Error => AnsiSgrPair.RedLetter,
        _ => throw new UnreachableException(),
    };
}

public record class Diagnostic(
    DiagnosticKind Kind,
    SourceRange Location,
    string Message
) : IBuildable {
    public bool IsOk { get; } = Kind.GetIsOK();

    public void Format(StringBuilder b) => b.Wrap(Kind.GetColor(), () => {
        b.Wrap(AnsiSgrPair.Inverted, () => {
            b.Append(' ');
            b.Append(Kind.ToString().ToUpperInvariant());
            b.Append(' ');
        });
        b.Append('\uE0B0');
        b.Append(" at ");
        b.Append(Location.ToString());
        b.Append(": ");
        b.AppendLine();
        b.Append(Message);
    });
}

public class DiagnosticFormatter {
    public record class StyleOptions {
        public bool MultiLine { get; init; }
    }
    public StyleOptions Style { get; init; }

    public DiagnosticFormatter() => Style = new();
    public DiagnosticFormatter(StyleOptions style) => Style = style;

    private readonly StringBuilder builder = new();

    public void Append(char c) => builder.Append(c);
    public void Append(string s) => builder.Append(s);

    public void AppendLine() => builder.AppendLine();
    public void AppendLine(string s) => builder.AppendLine(s);

    public void AppendAnsiCode(int code) {
        Append('\x1B');
        Append('[');
        Append(code.ToString());
        Append('m');
    }

    public override string ToString() {
        return builder.ToString();
    }
}
