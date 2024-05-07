namespace TypeLox;

public sealed class Resolver : AstNode.IVisitor<Unit> {
    enum VariableState {
        Declared,
        Defined,
    }

    private readonly DiagnosticList diagnostics;
    private readonly Stack<Dictionary<string, VariableState>> scopes = [];
    private readonly LocalDepth locals = new();
    private FunctionKind? currentFunction = null;
    private ClassKind? currentClass = null;

    private Resolver(DiagnosticList diagnostics) {
        this.diagnostics = diagnostics;
    }

    // TODO: Maybe put this in Core
    /// <summary>
    /// Assigns the value to the reference, and then returns the <b>old</b> value.
    /// </summary>
    public static T Replace<T>(ref T current, T newValue) {
        var oldValue = current;
        current = newValue;
        return oldValue;
    }

    ///////////////
    // Utitities //
    ///////////////

    void BeginScope() => scopes.Push([]);

    void EndScope() => scopes.Pop();

    class AutoEndScope(Resolver resolver) : IDisposable {
        bool hasDisposed = false;

        public void Dispose() {
            if (!hasDisposed) {
                resolver.EndScope();
                hasDisposed = true;
            }
        }
    }

    AutoEndScope Scope() {
        BeginScope();
        return new(this);
    }

    void Declare(Token name) {
        var lexeme = name.Lexeme;
        var scope = scopes.Peek();
        if (scope.ContainsKey(lexeme)) {
            diagnostics.AddError(name.Location,
                $"item named '{lexeme}' is already defined in this scope"
            );
        }
        scope[lexeme] = VariableState.Declared;
    }

    void Define(Token name) {
        scopes.Peek()[name.Lexeme] = VariableState.Defined;
    }

    Unit Resolve(Expr expr) => expr.Accept(this);
    Unit Resolve(Stmt stmt) => stmt.Accept(this);

    Unit Resolve(IEnumerable<Stmt> statements) {
        foreach (var stmt in statements) {
            stmt.Accept(this);
        }
        return unit;
    }

    void ResolveLocal(Expr expr, Token name) {
        var lexeme = name.Lexeme;
        var depth = 0;
        // Stack enumerates from top to bottom
        foreach (var scope in scopes) {
            if (scope.ContainsKey(lexeme)) {
                locals.ResolveLocal(expr, depth);
                return;
            } else {
                depth++;
            }
        }
    }

    ////////////////
    // : IVisitor //
    ////////////////

    public Unit Visit(Expr.Assign node) {
        Resolve(node.Value);
        ResolveLocal(node, node.Name);
        return unit;
    }

    public Unit Visit(Expr.Variable node) {
        if (
            scopes.TryPeek(out var scope) &&
            scope.TryGetValue(node.Name.Lexeme, out var state) &&
            state is VariableState.Declared
        ) {
            diagnostics.AddError(node.Name.Location,
                $"cannot access {node.Name.Lexeme} in its own initializer"
            );
        }
        ResolveLocal(node, node.Name);
        return unit;
    }

    public Unit Visit(Expr.Binary node) {
        Resolve(node.Left);
        Resolve(node.Right);
        return unit;
    }

    public Unit Visit(Expr.Call node) {
        Resolve(node.Callee);
        foreach (var arg in node.Arguments) {
            Resolve(arg);
        }
        return unit;
    }

    public Unit Visit(Expr.GetProperty node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Expr.SetProperty node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Expr.Grouping node) => Resolve(node.Inner);

    public Unit Visit(Expr.Literal node) => unit;

    public Unit Visit(Expr.Logical node) {
        Resolve(node.Left);
        Resolve(node.Right);
        return unit;
    }

    public Unit Visit(Expr.Super node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Expr.This node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Expr.Unary node) => Resolve(node.Operand);

    public Unit Visit(Stmt.Assert node) => Resolve(node.Expr);

    public Unit Visit(Stmt.Block node) {
        using (Scope()) {
            Resolve(node.Statements);
        }
        return unit;
    }

    public Unit Visit(Stmt.Class node) {
        var old = Replace(ref currentClass, ClassKind.Class);
        {
            Declare(node.Name);
            Define(node.Name);
        }
        currentClass = old;
        return unit;
    }

    public Unit Visit(Stmt.Expression node) => Resolve(node.Expr);

    public Unit Visit(Stmt.Function node) {
        var old = Replace(ref currentFunction, node.Kind);
        {
            Declare(node.Name);
            Define(node.Name);
            BeginScope();
            {
                foreach (var param in node.Parameters) {
                    Declare(param);
                    Define(param);
                }
                Resolve(node.Statements);
            }
            EndScope();
        }
        currentFunction = old;
        return unit;
    }

    public Unit Visit(Stmt.If node) {
        Resolve(node.Condition);
        Resolve(node.IfTrue);
        if (node.IfFalse is Stmt ifFalse) {
            Resolve(ifFalse);
        }
        return unit;
    }

    public Unit Visit(Stmt.Module node) {
        var isolate = node.Kind.IsIsolated();
        if (isolate) { BeginScope(); }
        Resolve(node.Statements);
        if (isolate) { EndScope(); }
        return unit;
    }

    public Unit Visit(Stmt.Print node) => Resolve(node.Expr);

    public Unit Visit(Stmt.Return node) {
        if (currentFunction is null) {
            diagnostics.AddError(node.Keyword.Location, $"cannot return outside of functions");
        }
        if (node.Expr is Expr e) {
            Resolve(e);
        }
        return unit;
    }

    public Unit Visit(Stmt.Var node) {
        Declare(node.Name);
        if (node.Initializer is Expr init) {
            Resolve(init);
        }
        Define(node.Name);
        return unit;
    }

    public Unit Visit(Stmt.While node) {
        Resolve(node.Condition);
        Resolve(node.Body);
        return unit;
    }

    /////////
    // Fin // 
    /////////

    public static LocalDepth Resolve(
        Stmt.Module module,
        DiagnosticList diagnostics
    ) {
        var resolver = new Resolver(diagnostics);
        resolver.Resolve(module);
        return resolver.locals;
    }
}