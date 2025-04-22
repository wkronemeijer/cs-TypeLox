namespace TypeLox;

using static TokenKind;

public enum FunctionKind {
    Function,
    Initializer,
    Method,
}

public enum ClassKind {
    Class,
    SubClass,
}

// TODO: Try out a Pratt Parser
public sealed class Parser {
    private const string INITIALIZER_NAME = "init";

    private readonly List<Token> tokens;
    private readonly DiagnosticList diagnostics;

    private int current = 0;

    private Parser(List<Token> tokens, DiagnosticList diagnostics) {
        this.tokens = tokens;
        this.diagnostics = diagnostics;
    }

    ///////////////
    // Utilities //
    ///////////////

    bool IsValid(int index) {
        return 0 <= index && index < tokens.Count;
    }

    // Beginning to feel that that EOF token is more trouble that it is worth
    bool IsAtEnd => !IsValid(current);

    Token Advance() => tokens[current++];

    Token Peek(int offset) => tokens[current + offset];

    Token Previous => Peek(-1);
    Token Current => Peek(0);

    Token Consume(TokenKind kind, string description) {
        var actual = Current.Kind;
        if (actual == kind) {
            return Advance();
        } else {
            throw Error($"received {actual}, expected {description}");
        }
    }

    bool Check(TokenKind kind) {
        if (IsValid(current)) {
            return Current.Kind == kind;
        } else {
            return false;
        }
    }

    bool Match(TokenKind kind) {
        if (Current.Kind == kind) {
            Advance();
            return true;
        } else {
            return false;
        }
    }

    bool MatchAny(params TokenKind[] kinds) {
        foreach (var kind in kinds) {
            if (Match(kind)) {
                return true;
            }
        }
        return false;
    }

    private class Recovery() : Exception() {
        public static Recovery Instance { get; } = new();
    }

    Recovery Error(string message, SourceRange? location = null) {
        diagnostics.AddError(location ?? Current.Location, message);
        return Recovery.Instance;
    }

    static bool CanSynchronizeAfter(TokenKind self) => self switch {
        SEMICOLON => true,
        _ => false,
    };

    static bool CanSynchronizeBefore(TokenKind self) => self switch {
        VAR or FUN or CLASS => true,
        IF or WHILE or FOR => true,
        PRINT or RETURN => true,
        EOF => true,
        _ => false,
    };

    void Synchronize() {
        while (!IsAtEnd) {
            if (CanSynchronizeBefore(Current.Kind)) { return; }
            Advance();
            if (CanSynchronizeAfter(Previous.Kind)) { return; }
        }
    }

    //////////////
    // Matching //
    //////////////

    Expr Assignment() {
        var lhs = Or();
        if (Match(EQUAL)) { // TODO: This is where := would go
            var op = Previous;
            var rhs = Assignment(); // recursion!

            if (lhs is Expr.Variable variable) {
                return new Expr.Assign(variable.Name, rhs);
            } else if (lhs is Expr.GetProperty get) {
                return new Expr.SetProperty(get.Target, get.Name, rhs);
            } else {
                Error("invalid assignment target", op.Location);
            }
        }
        return lhs;
    }

    Expr Or() {
        var lhs = And();
        while (Match(OR)) {
            var op = Previous;
            var rhs = And();
            lhs = new Expr.Logical(lhs, op, rhs);
        }
        return lhs;
    }

    Expr And() {
        var lhs = Equality();
        while (Match(AND)) {
            var op = Previous;
            var rhs = Equality();
            lhs = new Expr.Logical(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Equality() {
        var lhs = Comparison();
        while (MatchAny(BANG_EQUAL, EQUAL_EQUAL)) {
            var op = Previous;
            var rhs = Comparison();
            lhs = new Expr.Binary(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Comparison() {
        var lhs = Term();
        while (MatchAny(LESS, LESS_EQUAL, GREATER, GREATER_EQUAL)) {
            var op = Previous;
            var rhs = Term();
            lhs = new Expr.Binary(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Term() {
        var lhs = Factor();
        while (MatchAny(PLUS, MINUS)) {
            var op = Previous;
            var rhs = Factor();
            lhs = new Expr.Binary(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Factor() {
        var lhs = Unary();
        while (MatchAny(STAR, SLASH)) {
            var op = Previous;
            var rhs = Unary();
            lhs = new Expr.Binary(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Unary() {
        if (MatchAny(BANG, MINUS)) {
            var op = Previous;
            var rhs = Unary();
            return new Expr.Unary(op, rhs);
        }
        return Call();
    }

    Expr.Call FinishCallExpr(Expr expr) {
        var opening = Previous;
        var args = new List<Expr>();
        if (!Check(RIGHT_PAREN)) {
            do {
                args.Add(Expression());
            } while (Match(COMMA));
        }
        Consume(RIGHT_PAREN, "')' after argument list");
        return new Expr.Call(expr, opening, args);
    }

    Expr Call() {
        var expr = Primary();
        // This loop is fascinating...
        while (true) {
            if (Match(LEFT_PAREN)) {
                expr = FinishCallExpr(expr);
            } else if (Match(DOT)) {
                var name = Consume(IDENTIFIER, "property name");
                expr = new Expr.GetProperty(expr, name);
            } else {
                break;
            }
        }
        return expr;
    }

    Expr.Super SuperExpression() {
        var keyword = Previous;
        Consume(DOT, "'.' after super");
        var method = Consume(IDENTIFIER, "name of super method");
        return new(keyword, method);
    }

    Expr.Grouping GroupingExpression() {
        var expr = Expression();
        Consume(RIGHT_PAREN, "')' after parenthesized expression");
        return new(expr);
    }

    Expr Primary() {
        if (MatchAny(NIL, FALSE, TRUE, NUMBER, STRING)) {
            return Expr.Literal.FromToken(Previous);
        } else if (Match(SUPER)) {
            return SuperExpression();
        } else if (Match(THIS)) {
            return new Expr.This(Previous);
        } else if (Match(IDENTIFIER)) {
            return new Expr.Variable(Previous);
        } else if (Match(LEFT_PAREN)) {
            return GroupingExpression();
        } else {
            throw Error("expected expression");
        }
    }

    Expr Expression() {
        return Assignment();
    }

    ////////////////
    // Statements //
    ////////////////

    Stmt.If IfStatement() {
        Consume(LEFT_PAREN, "'(' before condition");
        var condition = Expression();
        Consume(RIGHT_PAREN, "')' after condition");

        var trueBranch = Statement();
        Stmt? falseBranch = null;
        if (Match(ELSE)) {
            falseBranch = Statement();
        }
        return new(condition, trueBranch, falseBranch);
    }

    Stmt.While WhileStatement() {
        Consume(LEFT_PAREN, "'(' before condition");
        var condition = Expression();
        Consume(RIGHT_PAREN, "')' after condition");
        var body = Statement();
        return new(condition, body);
    }

    // Foreshadowing...I want to add iterators
    Stmt ForTripleStatement() {
        Consume(LEFT_PAREN, "'(' before condition");

        // var i = 0;
        Stmt? initializer;
        if (Match(SEMICOLON)) {
            initializer = null;
        } else if (Match(VAR)) {
            initializer = VariableDeclaration();
        } else {
            initializer = ExpressionStatement();
        }

        // i < len
        Expr condition;
        if (!Check(SEMICOLON)) {
            condition = Expression();
        } else {
            condition = new Expr.Literal(true);
        }
        Consume(SEMICOLON, "';' after condition");

        // i = i + 1
        Expr? increment = null;
        if (!Check(RIGHT_PAREN)) {
            increment = Expression();
        }
        Consume(RIGHT_PAREN, "')' after clauses");

        var body = Statement();

        if (increment is not null) {
            body = new Stmt.Block([
                body,
                new Stmt.Expression(increment)
            ]);
        }

        body = new Stmt.While(condition, body);

        if (initializer is not null) {
            body = new Stmt.Block([
                initializer,
                body
            ]);
        }

        return body;
    }

    Stmt.Print PrintStatement() {
        var value = Expression();
        Consume(SEMICOLON, "';' after to be printed value");
        return new Stmt.Print(value);
    }

    Stmt AssertStatement() {
        var keyword = Previous;
        var value = Expression();
        Consume(SEMICOLON, "';' after inspected value");
        if (value is Expr.Binary binary && binary.Operator.Kind == EQUAL_EQUAL) {
            return new Stmt.AssertEqual(keyword, binary.Left, binary.Right);
        }
        return new Stmt.Assert(keyword, value);
    }

    Stmt.Return ReturnStatement() {
        var keyword = Previous;
        Expr? value = null;
        if (!Check(SEMICOLON)) {
            value = Expression();
        }
        Consume(SEMICOLON, "';' after return value");
        return new(keyword, value);
    }

    Stmt.Expression ExpressionStatement() {
        var result = new Stmt.Expression(Expression());
        Consume(SEMICOLON, "';' after expression");
        return result;
    }

    /// <summary>
    /// Note: Consumes a '}' at the end.
    /// </summary>
    Stmt.Block Block() {
        var statements = new List<Stmt>();
        while (!Check(RIGHT_BRACE) && !IsAtEnd) {
            statements.AddNotNull(DeclarationOrStatement());
        }
        Consume(RIGHT_BRACE, "'}' after block.");
        return new Stmt.Block(statements);
    }

    Stmt Statement() {
        if (Match(FOR)) {
            return ForTripleStatement();
        } else if (Match(IF)) {
            return IfStatement();
        } else if (Match(PRINT)) {
            return PrintStatement();
        } else if (Match(ASSERT)) {
            return AssertStatement();
        } else if (Match(WHILE)) {
            return WhileStatement();
        } else if (Match(RETURN)) {
            return ReturnStatement();
        } else if (Match(LEFT_BRACE)) {
            return Block();
        } else {
            return ExpressionStatement();
        }
    }

    //////////////////
    // Declarations //
    //////////////////

    Stmt.Var VariableDeclaration() {
        var name = Consume(IDENTIFIER, "variable name");
        var init = Match(EQUAL) ? Expression() : null;
        Consume(SEMICOLON, "';' after variable declaration");
        return new Stmt.Var(name, init);
    }

    Stmt.Function FunctionDeclaration(FunctionKind kind) {
        var functionOrMethod = kind.ToString().ToLowerInvariant();
        var name = Consume(IDENTIFIER, $"{functionOrMethod} name");
        Consume(LEFT_PAREN, $"'(' after {functionOrMethod} name");

        var parameters = new List<Token>();
        // TODO: Allow trailing comma's
        if (!Check(RIGHT_PAREN)) {
            do {
                parameters.Add(Consume(IDENTIFIER, $"parameter name"));
            } while (Match(COMMA));
        }
        Consume(RIGHT_PAREN, "')' after parameters");
        Consume(LEFT_BRACE, $"'{{' before {functionOrMethod} body");
        var body = Block().Statements;
        if (kind is FunctionKind.Method && name.Lexeme == INITIALIZER_NAME) {
            kind = FunctionKind.Initializer;
        }
        return new Stmt.Function(kind, name, parameters, body);
    }

    Stmt.Class ClassDeclaration() {
        var name = Consume(IDENTIFIER, "class name");

        Expr.Variable? superclass = null;
        if (Match(LESS)) {
            Consume(IDENTIFIER, "superclass name");
            superclass = new Expr.Variable(Previous);
        }
        Consume(LEFT_BRACE, "opening brace at the start of class body");
        var methods = new List<Stmt.Function>();
        while (!Check(RIGHT_BRACE) && !IsAtEnd) {
            methods.Add(FunctionDeclaration(FunctionKind.Method));
        }
        Consume(RIGHT_BRACE, "closing brace at end of class body");
        return new Stmt.Class(name, superclass, methods);
    }

    Stmt? DeclarationOrStatement() {
        try {
            if (Match(CLASS)) {
                return ClassDeclaration();
            } else if (Match(FUN)) {
                return FunctionDeclaration(FunctionKind.Function);
            } else if (Match(VAR)) {
                return VariableDeclaration();
            } else {
                return Statement();
            }
        } catch (Recovery) {
            Synchronize();
            return null;
        }
    }

    // TODO: Remove this when TokenList exists
    static Source GetSource(IList<Token> tokens) => tokens[0].Location.Source;

    Stmt.Module ParseFile() {
        var statements = new List<Stmt>();
        try {
            while (Current.Kind != EOF) {
                statements.AddNotNull(DeclarationOrStatement());
            }
            Consume(EOF, "end of file");
        } catch (Recovery) {
            // To ensure Recovery doesn't leave this file
        }
        var source = GetSource(tokens);
        return new(ModuleKind.SourceFile, source.Uri, statements);
    }

    public static Stmt.Module Parse(
        List<Token> tokens,
        DiagnosticList diagnostics
    ) => new Parser(tokens, diagnostics).ParseFile();
}
