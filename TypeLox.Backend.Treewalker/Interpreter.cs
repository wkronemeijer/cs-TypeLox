namespace TypeLox.Backend.Treewalker;

using static TokenKind;

public class Interpreter : IInterpreter, AstNode.IVisitor<object?> {
    private readonly CompilerOptions options;
    private readonly LocalDepth locals = new();
    private readonly Env globalEnv = new();
    private Env currentEnv;

    public ICompilerHost Host { get; }
    public string Name => "treewalk";

    public Interpreter(ICompilerHost host) {
        Host = host;
        options = host.Options;
        currentEnv = globalEnv;
        StdLib.DefineGlobals(globalEnv);
    }

    // TODO: Split out a compiler class? The front is going to be re-used for different backends
    // ProgrammableCompilingPipeline
    public Stmt.Module? Compile(Source source, DiagnosticList allDiagnostics) {
        var sourceDiagnostics = new DiagnosticList();
        // ref bool? out bool? out TokenList?
        var tokens = Scanner.Scan(source, sourceDiagnostics);
        var module = Parser.Parse(tokens, sourceDiagnostics);
        // TODO: Skip check if parsing failed
        var locals = Resolver.Resolve(module, sourceDiagnostics);

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

        if (options.PrintLocals) {
            Host.WriteLine($"Start of locals".Header());
            Host.WriteLine(locals.FormatToString().Trim());
            Host.WriteLine($"End of locals".Header());
        }

        allDiagnostics.AddAll(sourceDiagnostics);
        if (sourceDiagnostics.IsOk) {
            this.locals.Merge(locals);
            return module;
        } else {
            return null;
        }
    }

    public void Run(Source source) {
        var list = new DiagnosticList();
        var root = Compile(source, list);
        if (root is not null) {
            try {
                Execute(root);
            } catch (LoxRuntimeException e) {
                Host.WriteLine(e.ToString());
            }
        } else {
            Host.WriteLine(list.FormatToString().Trim());
        }
    }

    //////////////////
    // Interpreting //
    //////////////////

    public object? Evaluate(Expr expr) => expr.Accept(this);
    public void Execute(Stmt stmt) => stmt.Accept(this);

    public void ExecuteBlock(IEnumerable<Stmt> stmts, Env temporaryEnv) {
        var oldEnv = currentEnv;
        try {
            currentEnv = temporaryEnv;
            foreach (var stmt in stmts) {
                Execute(stmt);
            }
        } finally {
            currentEnv = oldEnv;
        }
    }

    object? LookupVariable(Token name, Expr expr) {
        if (locals.GetDepth(expr) is int depth) {
            return currentEnv.GetAt(name, depth);
        } else {
            return globalEnv.Get(name);
        }
    }

    /////////////////
    // Expressions //
    /////////////////

    public object? Visit(Expr.Assign node) {
        var value = Evaluate(node.Value);
        if (locals.GetDepth(node) is int d) {
            currentEnv.AssignAt(node.Name, d, value);
        } else {
            globalEnv.Assign(node.Name, value);
        }
        return value;
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

    public object? Visit(Expr.This node) => LookupVariable(node.Keyword, node);

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

    public object? Visit(Expr.Variable node) => LookupVariable(node.Name, node);

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
        ExecuteBlock(node.Statements, new Env(currentEnv));
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
        // Closure: function takes its lexical environment with it
        currentEnv.Define(node.Name, new LoxFunction(node, currentEnv));
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
        var env = node.Kind.IsIsolated() ? new Env(currentEnv) : currentEnv;
        ExecuteBlock(node.Statements, env);
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
        currentEnv.Define(node.Name, value);
        return null;
    }

    public object? Visit(Stmt.While node) {
        while (Evaluate(node.Condition).IsLoxTruthy()) {
            Execute(node.Body);
        }
        return null;
    }
}
