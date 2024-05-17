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

    public abstract IEnumerable<AstNode> GetChildren();

    // AstNode must not be cyclic
    // If it is, then this function will overflow the callstack
    public List<AstNode> GetDescendants() {
        var result = new HashSet<AstNode>(ReferenceEqualityComparer.Instance);
        var frontier = new Queue<AstNode>();
        frontier.Enqueue(this);
        while (frontier.TryDequeue(out var node)) {
            result.Add(node);
            foreach (var child in node.GetChildren()) {
                if (!result.Contains(child)) {
                    frontier.Enqueue(child);
                }
            }
        }
        result.Remove(this);
        return result.ToList();
    }
}

public abstract record class Expr : AstNode {
    public record class Assign(Token Name, Expr Value) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Value;
        }
    }

    public record class Binary(Expr Left, Token Operator, Expr Right) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Left;
            yield return Right;
        }
    }

    public record class Call(Expr Callee, Token Paren, List<Expr> Arguments) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Callee;
            foreach (var arg in Arguments) {
                yield return arg;
            }
        }
    }

    public record class GetProperty(Expr Target, Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Target;
        }
    }

    // Needed to disallow `(variable) = 10;` (note that C allows this)
    public record class Grouping(Expr Inner) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Inner;
        }
    }

    public record class Literal(object? Value) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() { yield break; }

        public static Literal FromToken(Token token) {
            return new(token.GetNativeValue());
        }
    }

    public record class Logical(Expr Left, Token Operator, Expr Right) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Left;
            yield return Right;
        }
    }

    public record class SetProperty(Expr Target, Token Name, Expr Value) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Target;
            yield return Value;
        }
    }

    public record class Super(Token Keyword, Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() { yield break; }
    }

    public record class This(Token Keyword) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() { yield break; }
    }

    public record class Unary(Token Operator, Expr Operand) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Operand;
        }
    }

    public record class Variable(Token Name) : Expr {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() { yield break; }
    }
}

public abstract record class Stmt() : AstNode {
    public record class Assert(Token Keyword, Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Expr;
        }
    }

    public record class Block(List<Stmt> Statements) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            foreach (var stmt in Statements) {
                yield return stmt;
            }
        }
    }

    public record class Class(Token Name, Expr.Variable? Superclass, List<Function> Methods) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            if (Superclass is not null) {
                yield return Superclass;
            }
            foreach (var method in Methods) {
                yield return method;
            }
        }
    }

    public record class Expression(Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Expr;
        }
    }

    public record class Function(
        FunctionKind Kind,
        Token Name,
        List<Token> Parameters,
        List<Stmt> Statements
    ) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            foreach (var stmt in Statements) {
                yield return stmt;
            }
        }
    }

    public record class If(Expr Condition, Stmt IfTrue, Stmt? IfFalse) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Condition;
            yield return IfTrue;
            if (IfFalse is not null) {
                yield return IfFalse;
            }
        }
    }

    public record class Module(
        ModuleKind Kind,
        Uri Uri,
        List<Stmt> Statements
    ) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            foreach (var stmt in Statements) {
                yield return stmt;
            }
        }
    }

    public record class Print(Expr Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Expr;
        }
    }

    public record class Return(Token Keyword, Expr? Expr) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            if (Expr is not null) {
                yield return Expr;
            }
        }
    }

    public record class Var(Token Name, Expr? Initializer) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            if (Initializer is not null) {
                yield return Initializer;
            }
        }
    }

    public record class While(Expr Condition, Stmt Body) : Stmt {
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);

        public override IEnumerable<AstNode> GetChildren() {
            yield return Condition;
            yield return Body;
        }
    }
}

// Interesting observation:
// Most expressions, and all declarations carry a Token around
// Token has a location, so you skip some of the turboboring location-tracking code
// Very nice, Bob!
