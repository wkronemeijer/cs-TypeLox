namespace TypeLox;

using static TokenKind;

public enum FunctionKind {
    None,
    Function,
    Initializer,
    Method,
}

public enum ClassKind {
    None,
    Class,
    SubClass,
}

// TODO: Try out a Pratt Parser
public class Parser(IList<Token> tokens, IDiagnosticLog log) {
    private int current = 0;

    ///////////////
    // Utilities //
    ///////////////

    bool IsValid(int index) {
        return 0 <= index && index < tokens.Count;
    }

    bool IsAtEnd => Peek().Kind == EOF;

    Token Advance() {
        return tokens[current++];
    }

    Token Peek(int offset = 0) {
        var index = current + offset;
        Assert(IsValid(index), "peek out of bounds");
        return tokens[index];
    }

    Token Previous() => Peek(-1);

    Token Consume(TokenKind kind, string description) {
        var actual = Peek().Kind;
        if (actual == kind) {
            return Advance();
        } else {
            throw Error($"received {actual}, expected {description}");
        }
    }

    bool Check(TokenKind kind) {
        if (IsValid(current)) {
            return Peek().Kind == kind;
        } else {
            return false;
        }
    }

    bool Match(TokenKind kind) {
        if (Peek().Kind == kind) {
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

    private class Recovery() : Exception {
        public static readonly Recovery start = new();
    }

    Recovery Error(string message, SourceRange? location = null) {
        log.Error(location ?? Peek().Location, message);
        return Recovery.start;
    }

    static bool CanSynchronize(TokenKind self) => self switch {
        VAR or FUN or CLASS => true,
        IF or WHILE or FOR => true,
        PRINT or RETURN => true,
        _ => false,
    };

    void Synchronize() {
        Advance(); // skip the offending token
        while (!IsAtEnd) {
            if (Previous().Kind == SEMICOLON) {
                return;
            } else if (CanSynchronize(Peek().Kind)) {
                return;
            } else {
                Advance();
            }
        }
    }

    //////////////
    // Matching //
    //////////////

    Expr Assignment() {
        var lhs = Or();
        if (Match(EQUAL)) { // TODO: This is where := would go
            var op = Previous();
            var rhs = Assignment(); // recursion!

            if (lhs is Expr.Variable variable) {
                return new Expr.Assign(variable.Name, rhs);
            } else if (lhs is Expr.Get get) {
                return new Expr.Set(get.Target, get.Name, rhs);
            } else {
                Error("invalid assignment target", op.Location);
            }
        }
        return lhs;
    }

    Expr Or() {
        var lhs = And();
        while (Match(OR)) {
            var op = Previous();
            var rhs = And();
            lhs = new Expr.Logical(lhs, op, rhs);
        }
        return lhs;
    }

    Expr And() {
        var lhs = Equality();
        while (Match(AND)) {
            var op = Previous();
            var rhs = Equality();
            lhs = new Expr.Logical(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Equality() {
        var lhs = Comparison();
        while (MatchAny(BANG_EQUAL, EQUAL_EQUAL)) {
            var op = Previous();
            var rhs = Comparison();
            lhs = new Expr.Binary(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Comparison() {
        var lhs = Term();
        while (MatchAny(LESS, LESS_EQUAL, GREATER, GREATER_EQUAL)) {
            var op = Previous();
            var rhs = Term();
            lhs = new Expr.Binary(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Term() {
        var lhs = Factor();
        while (MatchAny(PLUS, MINUS)) {
            var op = Previous();
            var rhs = Factor();
            lhs = new Expr.Binary(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Factor() {
        var lhs = Unary();
        while (MatchAny(STAR, SLASH)) {
            var op = Previous();
            var rhs = Unary();
            lhs = new Expr.Binary(lhs, op, rhs);
        }
        return lhs;
    }

    Expr Unary() {
        if (MatchAny(BANG, MINUS)) {
            var op = Previous();
            var rhs = Unary();
            return new Expr.Unary(op, rhs);
        }
        return Call();
    }

    Expr.Call FinishCallExpr(Expr expr) {
        var opening = Previous();
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
                expr = new Expr.Get(expr, name);
            } else {
                break;
            }
        }
        return expr;
    }

    Expr Primary() {
        if (MatchAny(NIL, FALSE, TRUE, NUMBER, STRING)) {
            return Expr.FromToken(Previous());
        } else if (Match(SUPER)) {
            throw new NotImplementedException();
        } else if (Match(THIS)) {
            return new Expr.This(Previous());
        } else if (Match(IDENTIFIER)) {
            return new Expr.Variable(Previous());
        } else if (Match(LEFT_PAREN)) {
            var expr = Expression();
            Consume(RIGHT_PAREN, "')' after parenthesized expression");
            return new Expr.Grouping(expr);
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

    Stmt.Print PrintStatement() {
        var value = Expression();
        Consume(SEMICOLON, "';' after to be printed value");
        return new Stmt.Print(value);
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
            throw new NotImplementedException();
        } else if (Match(IF)) {
            throw new NotImplementedException();
        } else if (Match(PRINT)) {
            return PrintStatement();
        } else if (Match(WHILE)) {
            throw new NotImplementedException();
        } else if (Match(RETURN)) {
            throw new NotImplementedException();
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
        Requires(kind is not FunctionKind.None);
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
        var body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    Stmt.Class ClassDeclaration() {
        var name = Consume(IDENTIFIER, "class name");

        Expr.Variable? superclass = null;
        if (Match(LESS)) {
            Consume(IDENTIFIER, "superclass name");
            superclass = new Expr.Variable(Previous());
        }

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

    public Stmt.Block Parse() {
        var stmts = new List<Stmt>();
        try {
            while (!IsAtEnd) {
                stmts.AddNotNull(DeclarationOrStatement());
            }
            Consume(EOF, "end of file");
        } catch (Recovery) {
            // To ensure Recovery doesn't leave this file
        }
        return new Stmt.Block(stmts);
    }
}
