namespace TypeLox;

public abstract record class AstNode {
    public interface IVisitor<R> {
        R Visit(Expr.Assign node);
        R Visit(Expr.Binary node);
        R Visit(Expr.Call node);
        R Visit(Expr.GetProperty node);
        R Visit(Expr.Grouping node);
        R Visit(Expr.Literal node);
        R Visit(Expr.Logical node);
        R Visit(Expr.SetProperty node);
        R Visit(Expr.Super node);
        R Visit(Expr.This node);
        R Visit(Expr.Unary node);
        R Visit(Expr.Variable node);

        R Visit(Stmt.Assert node);
        R Visit(Stmt.Block node);
        R Visit(Stmt.Class node);
        R Visit(Stmt.Expression node);
        R Visit(Stmt.Function node);
        R Visit(Stmt.If node);
        R Visit(Stmt.Module node);
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
    public record class GetProperty(Expr Target, Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    // CST needs this, but AST doesn't
    public record class Grouping(Expr Inner) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Literal(object? Value) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public static Literal FromToken(Token token) {
            return new(token.GetNativeValue());
        }
    }
    public record class Logical(Expr Left, Token Operator, Expr Right) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class SetProperty(Expr Target, Token Name, Expr Value) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Super(Token Keyword, Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class This(Token Keyword) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Unary(Token Operator, Expr Operand) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Variable(Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
}

public abstract record class Stmt() : AstNode {
    public record class Assert(Token Keyword, Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Block(List<Stmt> Statements) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Class(Token Name, Expr.Variable? Superclass, List<Function> Methods) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Expression(Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public record class Function(
        Token Name,
        List<Token> Parameters,
        List<Stmt> Statements
    ) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public record class If(Expr Condition, Stmt IfTrue, Stmt? IfFalse) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    public record class Module(
        Uri Uri,
        List<Stmt> Statements
    ) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public record class Print(Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public record class Return(Token Keyword, Expr? Expr) : Stmt {
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
