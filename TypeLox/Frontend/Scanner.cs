namespace TypeLox;

using static TokenKind;

public sealed class Scanner {
    private readonly Source source;
    private readonly DiagnosticList diagnostics;
    private readonly string sourceText;

    private int start = 0;
    private int current = 0;

    private Scanner(Source source, DiagnosticList diagnostics) {
        this.source = source;
        this.diagnostics = diagnostics;
        sourceText = source.Code;
    }

    ///////////////
    // Utilities //
    ///////////////

    string CurrentLexeme => sourceText[start..current];
    SourceRange CurrentLocation => new(source, start, current);

    bool IsValid(int index) {
        return 0 <= index && index < sourceText.Length;
    }

    bool IsAtEnd => !IsValid(current);

    char Peek(int offset = 0) {
        var index = current + offset;
        return IsValid(index) ? sourceText[index] : '\0';
    }

    char Advance() {
        return sourceText[current++];
    }

    void AdvanceWhile(Func<char, bool> predicate) {
        while (predicate(Peek())) {
            Advance();
        }
    }

    void AdvanceUntil(char c) {
        while (Peek() != c && IsValid(current)) {
            Advance();
        }
    }

    bool Match(char c) {
        Requires(c != '\0');
        if (Peek() == c) {
            Advance();
            return true;
        } else {
            return false;
        }
    }

    Token CreateToken(TokenKind kind) {
        return new Token(kind, CurrentLocation);
    }

    static bool IsAsciiDigit(char c) => (
        '0' <= c && c <= '9'
    );

    static bool IsAsciiAlpha(char c) => (
        'A' <= c && c <= 'Z' ||
        'a' <= c && c <= 'z' ||
        c == '_'
    );

    static bool IsAsciiAlphaNum(char c) {
        return IsAsciiDigit(c) || IsAsciiAlpha(c);
    }

    static readonly Dictionary<string, TokenKind> keywords = [];
    static Scanner() {
        foreach (var kind in Enum.GetValues<TokenKind>()) {
            if (kind.GetIsKeyword()) {
                keywords[kind.GetLexeme()] = kind;
            }
        }
    }

    //////////////
    // Matching //
    //////////////

    Token? ContinueNumber() {
        AdvanceWhile(IsAsciiDigit);
        if (Peek(0) == '.' && IsAsciiDigit(Peek(1))) {
            Advance();
            AdvanceWhile(IsAsciiDigit);
        }
        return CreateToken(NUMBER);
    }

    Token? ContinueString() {
        AdvanceUntil('"');
        if (IsAtEnd) {
            diagnostics.AddError(CurrentLocation, "unterminated string");
            return null;
        } else {
            Advance();
            return CreateToken(STRING);
        }
    }

    Token? ContinueIdentifierOrKeyword() {
        AdvanceWhile(IsAsciiAlphaNum);
        return CreateToken(keywords.TryGetValue(CurrentLexeme, out var kind) ? kind : IDENTIFIER);
    }

    Token? ScanToken() {
        var c = Advance();
        switch (c) {
            // Whitespace
            case ' ':
            case '\t':
            case '\v':
            case '\n':
            case '\r':
                return null;

            // Brackets
            case '(':
                return CreateToken(LEFT_PAREN);
            case ')':
                return CreateToken(RIGHT_PAREN);
            case '{':
                return CreateToken(LEFT_BRACE);
            case '}':
                return CreateToken(RIGHT_BRACE);

            // Single characters
            case ',':
                return CreateToken(COMMA);
            case '.':
                return CreateToken(DOT);
            case ';':
                return CreateToken(SEMICOLON);
            case '+':
                return CreateToken(PLUS);
            case '-':
                return CreateToken(MINUS);
            case '*':
                return CreateToken(STAR);

            // 1 or 2 chars
            case '!':
                return CreateToken(Match('=') ? BANG_EQUAL : BANG);
            case '=':
                return CreateToken(Match('=') ? EQUAL_EQUAL : EQUAL);
            case '<':
                return CreateToken(Match('=') ? LESS_EQUAL : LESS);
            case '>':
                return CreateToken(Match('=') ? GREATER_EQUAL : GREATER);

            // Complex

            case '/':
                if (Match('/')) {
                    AdvanceUntil('\n');
                    return null;
                } else {
                    return CreateToken(SLASH);
                }
            case '"':
                return ContinueString();

            default: {
                if (IsAsciiDigit(c)) {
                    return ContinueNumber();
                } else if (IsAsciiAlpha(c)) {
                    return ContinueIdentifierOrKeyword();
                } else {
                    // TODO: check for surrogates
                    diagnostics.AddError(CurrentLocation, $"unexpected character '{c}' (U+{(ushort)c:X4})");
                    return null;
                }
            }
        }
    }

    List<Token> ScanAllTokens() {
        var tokens = new List<Token>();
        while (IsValid(current)) {
            var item = ScanToken();
            tokens.AddNotNull(item);
            start = current;
        }
        tokens.Add(CreateToken(EOF));
        return tokens;
    }

    /////////
    // Fin //
    /////////

    public static List<Token> Scan(
        Source source,
        DiagnosticList diagnostics
    ) => new Scanner(source, diagnostics).ScanAllTokens();
}
