namespace TypeLox;

using static TypeLox.TokenKind;

public enum TokenKind {
    EOF,
    // Single characters
    LEFT_BRACE,
    RIGHT_BRACE,
    LEFT_PAREN,
    RIGHT_PAREN,
    MINUS,
    STAR,
    SLASH,
    SEMICOLON,
    COMMA,
    DOT,
    PLUS,

    // One or two
    BANG,
    BANG_EQUAL,
    EQUAL,
    EQUAL_EQUAL,
    LESS,
    LESS_EQUAL,
    GREATER,
    GREATER_EQUAL,

    // Keywords
    IF, // must be first keyword
    ELSE,
    WHILE,
    FOR,
    RETURN,
    PRINT,
    THIS,
    SUPER,
    AND,
    OR,

    VAR,
    FUN,
    CLASS,

    NIL,
    FALSE,
    TRUE, // must be last keyword

    // Literals
    NUMBER,
    STRING,
    IDENTIFIER,
}

public static class TokenKindMethods {
    //                                            = BIG
    private static readonly TokenKind firstKeyword = IF;
    private static readonly TokenKind lastKeyword = TRUE;

    // Name is open for improvements
    public static int GetMaximumTokenKindLength() =>
        Enum.GetValues<TokenKind>().Max(k => k.ToString().Length)
    ;

    public static bool GetIsKeyword(this TokenKind self) =>
        (int)firstKeyword <= (int)self &&
        (int)self <= (int)lastKeyword
    ;
}

public sealed class Token(TokenKind kind, SourceRange location) {
    public TokenKind Kind => kind;
    public SourceRange Location => location;

    // Lazy because most tokens don't need their lexeme
    // TODO: Collect some performance data on this
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

    private static readonly int maxLength = TokenKindMethods.GetMaximumTokenKindLength();

    public override string ToString() {
        return $"{Kind.ToString().PadRight(maxLength)} [{Location.Start,4}..<{Location.End,4}] |{Lexeme}|";
    }
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
