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
    //                            = BIG
    static TokenKind firstKeyword = IF;
    static TokenKind lastKeyword = TRUE;

    public static bool GetIsKeyword(this TokenKind self) =>
        (int)firstKeyword <= (int)self &&
        (int)self <= (int)lastKeyword
    ;
}

public record class Token(
    TokenKind Kind,
    SourceRange Location
) {
    // RE: Why make this lazy?
    // Identifiers, numbers and strings are evaluated like this
    // But most literal tokens like !, + and the like are not
    // Maybe we should check some data
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

    public override string ToString() {
        return $"{Kind,-15} [{Location.Start,4}..<{Location.End,4}] |{Lexeme}|";
    }
}
