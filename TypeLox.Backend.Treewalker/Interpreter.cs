namespace TypeLox.Backend.Treewalker;

using static TokenKind;

// Hmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm
// Does this make sense? 
// Should we put it somewhere else?
public interface IInterpreter : ICompiler {
    public object? Evaluate(Expr expr);
    public void Execute(Stmt stmt);
    public void ExecuteBlock(IList<Stmt> stmts, Env environment);
}

public class TreeWalkInterpreter : IInterpreter, AstNode.IVisitor<object?> {
    public string Name => "treewalk";

    public ICompilerHost Host { get; }

    private readonly CompilerOptions options;
    private readonly Env globalEnvironment = new();
    private Env currentEnvironment;

    public TreeWalkInterpreter(ICompilerHost host) {
        Host = host;
        options = host.Options;
        currentEnvironment = globalEnvironment;

        StdLib.DefineGlobals(globalEnvironment);
    }

    // TODO: Split out a compiler class? The front is going to be re-used for different backends
    // ProgrammableCompilingPipeline
    private Stmt.Module? Compile(Source source, out IDiagnosticLog log) {
        log = new DiagnosticLog();
        var tokens = new Scanner(source, log).Scan();
        var module = new Parser(tokens, log).Parse();
        new SyntaxCheck(module, log).Check();

        if (options.PrintTokens) {
            Host.WriteLine($"Start of tokens".Header());
            Host.WriteLine(tokens.ToDebugString());
            Host.WriteLine($"End of tokens".Header());
        }
        if (options.PrintTree) {
            Host.WriteLine($"Start of tree".Header());
            Host.WriteLine(module.ToDebugString());
            Host.WriteLine($"End of tree".Header());
        }

        return log.IsOk ? module : null;
    }

    private void Run(Source source, bool isolated) {
        // Idea: configurable run pipeline
        var root = Compile(source, out var log);
        if (root is not null) {
            try {
                var env = isolated ? new Env(currentEnvironment) : currentEnvironment;
                ExecuteBlock(root.Statements, env);
            } catch (LoxRuntimeException e) {
                Host.WriteLine(e.ToString());
            }
        } else {
            Host.WriteLine(log.FormatToString().Trim());
        }
    }

    public void RunAsModule(Source source) => Run(source, isolated: true);
    public void RunAsScript(Source source) => Run(source, isolated: false);

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

            // Any
            case EQUAL_EQUAL:
                return left.LoxEquals(right);
            case BANG_EQUAL:
                return !left.LoxEquals(right);
        }
        throw new LoxRuntimeException(
            node.Operator.Location,
            $"unsupported operation: {left?.GetType()} {node.Operator.Lexeme} {right?.GetType()}"
        );
    }

    public object? Visit(Expr.Call node) {
        var callee = Evaluate(node.Callee);
        // Does Select always evaluate in sequence?
        var arguments = node.Arguments.Select(Evaluate).ToList();
        var location = node.Paren.Location;
        if (callee is ILoxCallable callable) {
            return callable.Call(this, location, arguments);
        } else {
            throw new LoxRuntimeException(location, $"{callee?.GetType()} is not callable");
        }
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
        var operand = Evaluate(node.Operand);
        switch (node.Operator.Kind) {
            case MINUS when operand is double number:
                return -number;
            case BANG:
                return !operand.IsLoxTruthy();
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
        currentEnvironment.Define(node.Name, new LoxFunction(node));
        return null;
    }

    public object? Visit(Stmt.If node) {
        if (Evaluate(node.Condition).IsLoxTruthy()) {
            Execute(node.IfTrue);
        } else if (node.IfFalse is Stmt ifFalse) {
            Execute(ifFalse);
        }
        return null;
    }

    public object? Visit(Stmt.Module node) {
        var moduleTopLevel = new Env(currentEnvironment);
        ExecuteBlock(node.Statements, moduleTopLevel);
        // TODO: Register module 
        return null;
    }

    public object? Visit(Stmt.Print node) {
        var value = Evaluate(node.Expr);
        Host.WriteLine(value.ToLoxString());
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
