namespace TypeLox;

using static TypeLox.TokenKind;

public sealed class Token(
    TokenKind kind,
    SourceRange location
) : IDisplay {
    public TokenKind Kind => kind;
    public SourceRange Location => location;

    private string? lexeme;
    public string Lexeme => lexeme ??= Location.GetLexeme();
    internal bool IsExpanded => lexeme is not null;

    // Maybe have the scanner put the value in here?
    public object? GetNativeValue() => Kind switch {
        NIL => null,
        FALSE => false,
        TRUE => true,
        NUMBER => double.Parse(Lexeme),
        STRING => Lexeme[1..^1],// slice off the "" at both ends
        _ => throw new ArgumentException($"{Kind} is not a literal token"),
    };

    private static readonly int maxLength = Enum.GetValues<TokenKind>().Max(k => k.ToString().Length);

    private const char verticalBar = '\u2502'; // the box drawing kind

    public void FormatLexeme(IFormatter f) {
        f.Append(verticalBar);
        f.Append(Lexeme);
        f.Append(verticalBar);
    }

    public void Format(IFormatter f) {
        f.Append(Kind.ToString().PadRight(maxLength));
        FormatLexeme(f);
    }

    public override string ToString() => this.FormatToString();
}

// Rather than a full TokenList class (which wouldn't do enough)
// We extend IEnumerable with DebugString
public static class TokenListMethods {
    public static string ToDebugString(this IEnumerable<Token> tokens) {
        var result = new StringBuilder();
        foreach (var token in tokens) {
            result.AppendLine(token.ToString());
        }
        return result.ToString().Trim();
    }
}
