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
    ASSERT,
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
    //                                   = BIG
    private const TokenKind firstKeyword = IF;
    private const TokenKind lastKeyword = TRUE;

    public static bool GetIsKeyword(this TokenKind self) => (
        (int)firstKeyword <= (int)self &&
        (int)self <= (int)lastKeyword
    );

    public static string GetLexeme(this TokenKind self) {
        if (Enum.GetName(self) is string name) {
            if (self.GetIsKeyword()) {
                return name.ToLowerInvariant();
            } else {
                throw new Exception($"{name} is not a keyword");
            }
        } else {
            throw new Exception($"no name for {self}");
        }
    }
}
