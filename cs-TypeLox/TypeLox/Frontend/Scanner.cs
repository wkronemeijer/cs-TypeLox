namespace TypeLox;

using static TokenKind;

public sealed class Scanner {
    private const char TERMINATOR = '\0';

    private readonly Source source;
    private readonly DiagnosticList diagnostics;
    private readonly string sourceText;

    private int startIndex = 0;
    private int currentIndex = 0;

    private Scanner(Source source, DiagnosticList diagnostics) {
        this.source = source;
        this.diagnostics = diagnostics;
        sourceText = source.Code;
    }

    ///////////////
    // Utilities //
    ///////////////

    string CurrentLexeme => sourceText[startIndex..currentIndex];
    SourceRange CurrentLocation => new(source, startIndex, currentIndex);

    Token CreateToken(TokenKind kind) {
        return new(kind, CurrentLocation);
    }

    Token? CreateError(string message) {
        diagnostics.AddError(CurrentLocation, message);
        return null;
    }

    char Advance() => sourceText[currentIndex++];

    char Peek(int offset) {
        var index = currentIndex + offset;
        if (0 <= index && index < sourceText.Length) {
            return sourceText[index];
        } else {
            return TERMINATOR;
        }
    }

    char Previous => Peek(-1);
    char Current => Peek(0);
    char Next => Peek(1);

    bool IsAtEnd => Current is TERMINATOR;

    void AdvanceWhile(Func<char, bool> predicate) {
        Requires(predicate(TERMINATOR) is false, "predicate must return false for terminator");
        while (predicate(Current)) {
            Advance();
        }
    }

    void AdvanceUntil(char c) {
        Requires(c is not TERMINATOR, "cannot advance to terminator");
        while (!(Current == c || IsAtEnd)) {
            Advance();
        }
    }

    bool Match(char c) {
        if (Current == c) {
            Advance();
            return true;
        } else {
            return false;
        }
    }

    static bool IsAsciiDigit(char c) => (c is
        >= '0' and <= '9'
    );

    static bool IsAsciiAlpha(char c) => (c is
        >= 'A' and <= 'Z' or
        >= 'a' and <= 'z' or
        '_' or '$'
    );

    static bool IsAsciiAlphaNum(char c) => IsAsciiDigit(c) || IsAsciiAlpha(c);

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
        if (Current == '.' && IsAsciiDigit(Next)) {
            Advance();
            AdvanceWhile(IsAsciiDigit);
        }
        return CreateToken(NUMBER);
    }

    Token? ContinueString() {
        AdvanceUntil('"');
        if (!IsAtEnd) {
            Advance();
            return CreateToken(STRING);
        } else {
            return CreateError("unterminated string");
        }
    }

    Token? ContinueIdentifierOrKeyword() {
        AdvanceWhile(IsAsciiAlphaNum);
        return CreateToken(keywords.TryGetValue(CurrentLexeme, out var kind) ? kind : IDENTIFIER);
    }

    Token? ScanToken() {
        startIndex = currentIndex;
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
                    Advance();
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
                } else if (char.IsHighSurrogate(c) && char.IsLowSurrogate(Current)) {
                    var hi = c;
                    var lo = Advance();
                    var codepoint = char.ConvertToUtf32(hi, lo);
                    return CreateError($"unexpected character '{hi}{lo}' (U+{codepoint:X5})");
                } else {
                    var codepoint = (ushort)c;
                    return CreateError($"unexpected character '{c}' (U+{codepoint:X4})");
                }
            }
        }
    }

    List<Token> ScanAllTokens() {
        var tokens = new List<Token>();
        while (!IsAtEnd) {
            var item = ScanToken();
            tokens.AddNotNull(item);
        }
        tokens.Add(CreateToken(EOF)); // TODO: Remove silly EOF token
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
