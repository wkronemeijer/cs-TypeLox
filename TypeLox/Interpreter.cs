namespace TypeLox;

using static TokenKind;

interface IInterpreter {
    ICompilerHost Host { get; } // needed to resolve `require` calls
    void Run(Source source);

    void RunFile(Uri uri) {
        Run(Host.CreateSourceFromFile(uri));
    }

    void RunSnippet(string snippet) {
        Run(Host.CreateSourceFromSnippet(snippet));
    }
}

class Interpreter(ICompilerHost host, ProgramOptions options) : IInterpreter, AstNode.IVisitor<object?> {
    public ICompilerHost Host => host;

    // TODO: Split out a compiler class? The front is going to be re-used for different backends
    // ProgrammableCompilingPipeline
    public Stmt.Block Compile(Source source, IDiagnosticLog log) {
        var tokens = new Scanner(source, log).ScanAll();
        var root = new Parser(tokens, log).Parse();

        if (options.PrintTokens) {
            foreach (var token in tokens) {
                host.WriteLine(token.ToString());
            }
        }
        if (options.PrintTree) {
            host.WriteLine(root.ToDebugString());
        }

        // TODO: Should we return null?
        // Empty block stmt is valid, but suprising perhaps
        // Alternative logic can get out of sync, however
        return root;
    }

    public void Run(Source source) {
        // Idea: configurable run pipeline
        var log = new DiagnosticLog();
        var root = Compile(source, log);

        if (!log.IsOk) {
            host.WriteLine(log.CreateReport());
            return;
        }

        try {
            root.Accept(this);
        } catch (LoxRuntimeException e) {
            host.WriteLine(e.ToString());
        }
    }

    //////////////////
    // Interpreting //
    //////////////////

    public object? Evaluate(Expr expr) => expr.Accept(this);
    public void Execute(Stmt stmt) => stmt.Accept(this);

    public static bool IsTruthy(object? value) => value.IsLoxTruthy();
    public static string Stringify(object? value) => value.ToLoxString();

    /////////////////
    // Expressions //
    /////////////////

    public object? Visit(Expr.Assign node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Binary node) {
        var left = Evaluate(node.Left);
        var right = Evaluate(node.Right);

        switch (node.Operator.Kind) {
            // Number
            case PLUS when left is double l && right is double r:
                return l + r;
            case MINUS when left is double l && right is double r:
                return l - r;
            case STAR when left is double l && right is double r:
                return l * r;
            case SLASH when left is double l && right is double r:
                return l / r;
            // String
            case PLUS when left is string l && right is string r:
                return l + r;
        }

        throw new LoxRuntimeException(node.Operator.Location, "invalid operand");
    }

    public object? Visit(Expr.Call node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Get node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Grouping node) => Evaluate(node.Inner);

    public object? Visit(Expr.Literal node) => node.Value;

    public object? Visit(Expr.Logical node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Set node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Super node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.This node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Unary node) {
        var right = Evaluate(node.Right);
        switch (node.Operator.Kind) {
            case MINUS when right is double d:
                return -d;
            case BANG:
                return !IsTruthy(right);
        }
        throw new LoxRuntimeException(node.Operator.Location, "invalid operand");
    }

    public object? Visit(Expr.Variable node) {
        throw new NotImplementedException();
    }

    ////////////////
    // Statements //
    ////////////////

    public object? Visit(Stmt.Block node) {
        foreach (var stmt in node.Statements) {
            Execute(stmt);
        }
        return null;
    }

    public object? Visit(Stmt.Class node) {
        throw new NotImplementedException();
    }

    public object? Visit(Stmt.Expression node) {
        Evaluate(node.Expr);
        return null;
    }

    public object? Visit(Stmt.Function node) {
        throw new NotImplementedException();
    }

    public object? Visit(Stmt.If node) {
        throw new NotImplementedException();
    }

    public object? Visit(Stmt.Print node) {
        var value = Evaluate(node.Expr);
        host.WriteLine(Stringify(value));
        return null;
    }

    public object? Visit(Stmt.Return node) {
        throw new NotImplementedException();
    }

    public object? Visit(Stmt.Var node) {
        throw new NotImplementedException();
    }

    public object? Visit(Stmt.While node) {
        throw new NotImplementedException();
    }
}
