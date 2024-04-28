namespace TypeLox.Backend.Treewalker;

using static TokenKind;

public interface IInterpreter {
    /// <summary>
    /// Host environment, which provides file system and output abstraction.
    /// </summary>
    ICompilerHost Host { get; }

    /// <summary>
    /// Runs a piece of source code.
    /// </summary>
    void Run(Source source);

    /// <summary>
    /// Runs a source file.
    /// </summary>
    void RunFile(Uri uri) {
        Run(Host.CreateSourceFromFile(uri));
    }

    /// <summary>
    /// Runs an indepedent snippet of code.
    /// </summary>
    void RunSnippet(string snippet) {
        Run(Host.CreateSourceFromSnippet(snippet));
    }
}

class Interpreter : IInterpreter, AstNode.IVisitor<object?> {
    public ICompilerHost Host { get; }

    private readonly ProgramOptions options;
    private readonly Env globalEnvironment = new();
    private Env currentEnvironment;

    public Interpreter(ICompilerHost host, ProgramOptions programOptions) {
        Host = host;
        options = programOptions;
        currentEnvironment = globalEnvironment;
    }

    // TODO: Split out a compiler class? The front is going to be re-used for different backends
    // ProgrammableCompilingPipeline
    public List<Stmt> Compile(Source source, IDiagnosticLog log) {
        // Should we use `out DiagnosticLog log`?
        var tokens = new Scanner(source, log).ScanAll();
        var root = new Parser(tokens, log).Parse();
        if (options.PrintTokens) {
            Host.WriteLine($"Start of tokens".Header());
            Host.WriteLine(tokens.ToDebugString());
            Host.WriteLine($"End of tokens".Header());
        }
        if (options.PrintTree) {
            Host.WriteLine($"Start of tree".Header());
            Host.WriteLine(root.ToDebugString());
            Host.WriteLine($"End of tree".Header());
        }
        // TODO: Should we return null?
        // Empty block stmt is valid, but suprising perhaps
        // Alternative logic can get out of sync, however
        // Return a CST with a comment saying "compile went awry"
        return root;
    }

    public void Run(Source source) {
        // Idea: configurable run pipeline
        var log = new DiagnosticLog();
        var root = Compile(source, log);

        if (!log.IsOk) {
            Host.WriteLine(log.CreateReport());
            return;
        }
        try {
            ExecuteBlock(root, currentEnvironment);
        } catch (LoxRuntimeException e) {
            Host.WriteLine(e.ToString());
        }
    }

    //////////////////
    // Interpreting //
    //////////////////

    public object? Evaluate(Expr expr) => expr?.Accept(this);
    public void Execute(Stmt stmt) => stmt.Accept(this);

    public void ExecuteBlock(IList<Stmt> stmts, Env environment) {
        var oldEnvironment = currentEnvironment;
        try {
            currentEnvironment = environment;
            foreach (var stmt in stmts) {
                Execute(stmt);
            }
        } finally {
            currentEnvironment = oldEnvironment;
        }
    }

    public static bool IsTruthy(object? value) => value.IsLoxTruthy();
    public static string Stringify(object? value) => value.ToLoxString();

    /////////////////
    // Expressions //
    /////////////////

    public object? Visit(Expr.Assign node) {
        currentEnvironment.Assign(node.Name, Evaluate(node.Value));
        return null;
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

            case LESS when left is double l && right is double r:
                return l < r;
            case LESS_EQUAL when left is double l && right is double r:
                return l <= r;
            case GREATER when left is double l && right is double r:
                return l > r;
            case GREATER_EQUAL when left is double l && right is double r:
                return l >= r;
            // String
            case PLUS when left is string l && right is string r:
                return l + r;
        }
        throw new LoxRuntimeException(node.Operator.Location, $"invalid operands for {node.Operator.Lexeme}");
    }

    public object? Visit(Expr.Call node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.GetProperty node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.Grouping node) => Evaluate(node.Inner);

    public object? Visit(Expr.Literal node) => node.Value;

    public object? Visit(Expr.Logical node) {
        throw new NotImplementedException();
    }

    public object? Visit(Expr.SetProperty node) {
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

    public object? Visit(Expr.Variable node) => currentEnvironment.Get(node.Name);

    ////////////////
    // Statements //
    ////////////////

    public object? Visit(Stmt.Assert node) {
        var value = Evaluate(node.Expr);
        if (!value.IsLoxTruthy()) {
            throw new LoxRuntimeException(node.Keyword.Location, "assertion failed");
        }
        return null;
    }

    public object? Visit(Stmt.Block node) {
        ExecuteBlock(node.Statements, new Env(currentEnvironment));
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
        if (Evaluate(node.Condition).IsLoxTruthy()) {
            Execute(node.IfTrue);
        } else if (node.IfFalse is Stmt ifFalse) {
            Execute(ifFalse);
        }
        return null;
    }

    public object? Visit(Stmt.Print node) {
        var value = Evaluate(node.Expr);
        Host.WriteLine(Stringify(value));
        return null;
    }

    public object? Visit(Stmt.Return node) {
        object? value = null;
        if (node.Expr is not null) {
            value = Evaluate(node.Expr);
        }
        throw new Return(value);
    }

    public object? Visit(Stmt.Var node) {
        object? value = null;
        if (node.Initializer is not null) {
            value = Evaluate(node.Initializer);
        }
        currentEnvironment.Define(node.Name, value);
        return null;
    }

    public object? Visit(Stmt.While node) {
        while (Evaluate(node.Condition).IsLoxTruthy()) {
            Execute(node.Body);
        }
        return null;
    }
}
