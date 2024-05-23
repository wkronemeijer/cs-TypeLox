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
    public Stmt.Module? Compile(
        Source source,
        DiagnosticList allDiagnostics
    ) {
        var sourceDiagnostics = new DiagnosticList();
        Stmt.Module? result = null;

        var tokens = Scanner.Scan(source, sourceDiagnostics);
        if (!sourceDiagnostics.IsOk) { goto fail; }
        if (options.PrintTokens is true) {
            Host.WriteLine($"Start of tokens".Header());
            Host.WriteLine(tokens.ToDebugString());
            Host.WriteLine($"End of tokens".Header());
        }

        var module = Parser.Parse(tokens, sourceDiagnostics);
        if (!sourceDiagnostics.IsOk) { goto fail; }
        if (options.PrintTree is true) {
            Host.WriteLine($"Start of tree".Header());
            Host.WriteLine(module.ToDebugString());
            Host.WriteLine($"End of tree".Header());
        }

        var locals = Resolver.Resolve(module, sourceDiagnostics);
        if (!sourceDiagnostics.IsOk) { goto fail; }
        if (options.PrintLocals is true) {
            Host.WriteLine($"Start of locals".Header());
            Host.WriteLine(locals.FormatToString().Trim());
            Host.WriteLine($"End of locals".Header());
        }

        this.locals.Merge(locals);
        result = module;

    fail:
        allDiagnostics.AddAll(sourceDiagnostics);
        return result;
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
        using (ReplaceUsing(ref currentEnv, temporaryEnv)) {
            foreach (var stmt in stmts) {
                Execute(stmt);
            }
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

    public object? Visit(Expr.Grouping node) => Evaluate(node.Inner);

    public object? Visit(Expr.Literal node) => node.Value;

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
            case PLUS when left is string || right is string:
                return left.ToLoxString() + right.ToLoxString();

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

    public object? Visit(Expr.Logical node) {
        var left = Evaluate(node.Left);
        var leftIsTruthy = left.IsLoxTruthy();

        switch (node.Operator.Kind) {
            case AND:
                return leftIsTruthy ? Evaluate(node.Right) : left;
            case OR:
                return leftIsTruthy ? left : Evaluate(node.Right);
        }
        throw new LoxRuntimeException(
            node.Operator.Location,
            $"unsupported operation: ? {node.Operator.Lexeme} ?"
        );
    }

    public object? Visit(Expr.Assign node) {
        var value = Evaluate(node.Value);
        if (locals.GetDepth(node) is int d) {
            currentEnv.AssignAt(node.Name, d, value);
        } else {
            globalEnv.Assign(node.Name, value);
        }
        return value;
    }

    public object? Visit(Expr.Variable node) => LookupVariable(node.Name, node);

    public object? Visit(Expr.Call node) {
        var callee = Evaluate(node.Callee);
        var arguments = node.Arguments.Select(Evaluate).ToList();
        var location = node.Paren.Location;
        if (callee is ILoxCallable callable) {
            return callable.Call(this, location, arguments);
        } else {
            throw new LoxRuntimeException(location,
                $"{callee?.GetType()} is not callable"
            );
        }
    }

    public object? Visit(Expr.This node) => LookupVariable(node.Keyword, node);

    public object? Visit(Expr.Super node) {
        if (locals.GetDepth(node) is int distance) {
            var rawSuper = currentEnv.GetAt("super", distance);
            var rawInstance = currentEnv.GetAt("this", distance - 1);

            if (rawSuper is LoxClass super) {
                if (super.FindMethod(node.Name.Lexeme) is LoxFunction method) {
                    if (rawInstance is LoxInstance @this) {
                        return method.BindThis(@this);
                    } else {
                        throw new Exception("this must be an object");
                    }
                } else {
                    throw new LoxRuntimeException(node.Keyword.Location,
                        $"undefined method '{node.Name.Lexeme}'"
                    );
                }
            } else {
                throw new Exception("super must be a class");
            }
        } else {
            throw new Exception("unresolved super");
        }
    }

    public object? Visit(Expr.GetProperty node) {
        var target = Evaluate(node.Target);
        if (target is LoxInstance instance) {
            return instance.GetProperty(node.Name);
        } else {
            throw new LoxRuntimeException(node.Name.Location,
                $"cannot get property on {target.ToLoxDebugString()}"
            );
        }
    }

    public object? Visit(Expr.SetProperty node) {
        var target = Evaluate(node.Target);
        if (target is LoxInstance instance) {
            var value = Evaluate(node.Value);
            instance.SetProperty(node.Name, value);
            return value;
        } else {
            throw new LoxRuntimeException(node.Name.Location,
                $"cannot get property on {target.ToLoxDebugString()}"
            );
        }
    }

    ////////////////
    // Statements //
    ////////////////

    public object? Visit(Stmt.Expression node) {
        Evaluate(node.Expr);
        return null;
    }

    public object? Visit(Stmt.Assert node) {
        var expr = node.Expr;
        var value = Evaluate(expr);
        if (!value.IsLoxTruthy()) {
            throw new LoxRuntimeException(node.Keyword.Location, $"assertion failed");
        }
        return null;
    }

    public object? Visit(Stmt.AssertEqual node) {
        var lhs = Evaluate(node.Left);
        var rhs = Evaluate(node.Right);
        if (!lhs.LoxEquals(rhs)) {
            throw new LoxRuntimeException(node.Keyword.Location,
                $"assertion failed: {lhs.ToLoxDebugString()} != {rhs.ToLoxDebugString()}"
            );
        }
        return null;
    }

    public object? Visit(Stmt.Print node) {
        var value = Evaluate(node.Expr);
        if (options.DisablePrint is not true) {
            Host.WriteLine(value.ToLoxString());
        }
        return null;
    }

    public object? Visit(Stmt.Return node) {
        object? value = null;
        if (node.Expr is not null) {
            value = Evaluate(node.Expr);
        }
        throw new Return(value);
    }

    public object? Visit(Stmt.Block node) {
        ExecuteBlock(node.Statements, new Env(currentEnv));
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

    public object? Visit(Stmt.While node) {
        while (Evaluate(node.Condition).IsLoxTruthy()) {
            Execute(node.Body);
        }
        return null;
    }

    public object? Visit(Stmt.Var node) {
        object? value = null;
        if (node.Initializer is not null) {
            value = Evaluate(node.Initializer);
        }
        currentEnv.Define(node.Name, value);
        return null;
    }

    public object? Visit(Stmt.Function node) {
        currentEnv.Define(node.Name, new LoxFunction(node, currentEnv));
        return null;
    }

    public object? Visit(Stmt.Class node) {
        LoxClass? super = null;
        if (node.Superclass is not null) {
            var value = Evaluate(node.Superclass);
            if (value is LoxClass c) {
                super = c;
            } else {
                throw new LoxRuntimeException(node.Superclass.Name.Location,
                    $"cannot extend non-class {value.ToLoxDebugString()}"
                );
            }
        }

        currentEnv.Define(node.Name, null);

        List<LoxFunction> methodList = [];
        LoxFunction? initializer = null;
        using (ReplaceUsing(ref currentEnv, new Env(currentEnv))) {
            if (super is not null) {
                currentEnv.Define("super", super);
            }

            foreach (var decl in node.Methods) {
                var method = new LoxFunction(decl, currentEnv);
                if (method.Kind is FunctionKind.Initializer) {
                    initializer = method;
                } else {
                    methodList.Add(method);
                }
            }
        }

        var methodDict = methodList.Select(method => (method.Name, method)).ToDictionary();

        currentEnv.Assign(node.Name, new LoxClass(node.Name, super, initializer, methodDict));
        return null;
    }

    public object? Visit(Stmt.Module node) {
        var env = node.Kind.IsIsolated() ? new Env(currentEnv) : currentEnv;
        ExecuteBlock(node.Statements, env);
        return null;
    }
}
