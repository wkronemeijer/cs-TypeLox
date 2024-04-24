namespace TypeLox;

public abstract partial record class AstNode {
    public interface IVisitor<R> {
        R Visit(Expr.Assign node);
        R Visit(Expr.Binary node);
        R Visit(Expr.Call node);
        R Visit(Expr.Get node);
        R Visit(Expr.Grouping node);
        R Visit(Expr.Literal node);
        R Visit(Expr.Logical node);
        R Visit(Expr.Set node);
        R Visit(Expr.Super node);
        R Visit(Expr.This node);
        R Visit(Expr.Unary node);
        R Visit(Expr.Variable node);
        R Visit(Stmt.Block node);
        R Visit(Stmt.Class node);
        R Visit(Stmt.Expression node);
        R Visit(Stmt.Function node);
        R Visit(Stmt.If node);
        R Visit(Stmt.Print node);
        R Visit(Stmt.Return node);
        R Visit(Stmt.Var node);
        R Visit(Stmt.While node);
    }

    public abstract R Accept<R>(IVisitor<R> visitor);
}

public abstract record class Expr : AstNode {
    public record class Assign(Token Name, Expr Value) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Binary(Expr Left, Token Operator, Expr Right) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Call(Expr Callee, Token Paren, List<Expr> Arguments) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Get(Expr Target, Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    // CST needs this, but AST doesn't
    public record class Grouping(Expr Inner) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Literal(object? Value) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Logical(Expr Left, Token Operator, Expr Right) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Set(Expr Target, Token Name, Expr Value) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Super(Token Keyword, Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class This(Token Keyword) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Unary(Token Operator, Expr Right) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Variable(Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public static Literal FromToken(Token token) {
        switch (token.Kind) {
            case TokenKind.NIL:
                return new(null);
            case TokenKind.FALSE:
                return new(false);
            case TokenKind.TRUE:
                return new(true);
            case TokenKind.NUMBER:
                return new(double.Parse(token.Lexeme));
            case TokenKind.STRING:
                // slice off the "" at both ends
                return new(token.Lexeme[1..^1]);
        }
        throw new ArgumentException($"{token.Kind} is not a literal token");
    }
}

public abstract record class Stmt() : AstNode {
    public record class Block(List<Stmt> Statements) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Class(Token Name, Expr.Variable? Superclass, List<Function> Methods) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Expression(Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Function(Token Name, List<Token> Parameters, Block Body) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class If(Expr Condition, Stmt IfTrue, Stmt IfFalse) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Print(Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Return(Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Var(Token Name, Expr? Initializer) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class While(Expr Condition, Stmt Body) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
}

// Interesting observation:
// Most expressions, and all declarations carry a Token around
// Token has a location, so you skip some of the turboboring location-tracking code
// Very nice, Bob!
