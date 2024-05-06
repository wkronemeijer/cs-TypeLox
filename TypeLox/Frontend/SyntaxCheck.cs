namespace TypeLox;

public sealed class SyntaxCheck(Stmt.Module module, IDiagnosticLog log)
: AstNode.IVisitor<Unit> {
    // AstNode's are record, so have custom Equals and HashCode
    // we explicitly want object identity
    private Dictionary<AstNode, bool> resolved = new(ReferenceEqualityComparer.Instance);

    private FunctionKind currentFunction = FunctionKind.None;
    private ClassKind currentClass = ClassKind.None;

    public void Test() {
        module.ToString();
        log.ToString();
        resolved.ToString();
    }

    public void Check() {

    }

    private Unit CheckExpr(Expr expr) => expr.Accept(this);
    private Unit CheckStmt(Stmt stmt) => stmt.Accept(this);

    private Unit CheckBlock(IList<Stmt> statements) {
        foreach (var stmt in statements) {
            stmt.Accept(this);
        }
        return unit;
    }

    public static void Temp<T>(
        ref T current,
        T temp,
        Action body
    ) where T : notnull {
        var old = current;
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        current = temp;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        // current can be closed over by the action, so this assignment is in fact visible
        body();
        current = old;
    }

    ////////////////
    // : IVisitor //
    ////////////////

    public Unit Visit(Expr.Assign node) => CheckExpr(node.Value);

    public Unit Visit(Expr.Binary node) {
        CheckExpr(node.Left);
        CheckExpr(node.Right);
        return unit;
    }

    public Unit Visit(Expr.Call node) {
        CheckExpr(node.Callee);
        foreach (var arg in node.Arguments) {
            CheckExpr(arg);
        }
        return unit;
    }

    public Unit Visit(Expr.GetProperty node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Expr.Grouping node) {
        return CheckExpr(node.Inner);
    }

    public Unit Visit(Expr.Literal node) {
        // TODO: Check for NaN or escape sequences here?
        return unit;
    }

    public Unit Visit(Expr.Logical node) {
        CheckExpr(node.Left);
        CheckExpr(node.Right);
        return unit;
    }

    public Unit Visit(Expr.SetProperty node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Expr.Super node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Expr.This node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Expr.Unary node) {
        CheckExpr(node.Operand);
        return unit;
    }

    public Unit Visit(Expr.Variable node) {
        // Resolving is done later (?)
        return unit;
    }

    public Unit Visit(Stmt.Assert node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.Block node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.Class node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.Expression node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.Function node) {
        Temp(ref currentFunction, FunctionKind.Function, delegate {
            if (currentFunction is FunctionKind.Initializer) {

            }
        });


        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.If node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.Module node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.Print node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.Return node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.Var node) {
        throw new NotImplementedException();
    }

    public Unit Visit(Stmt.While node) {
        throw new NotImplementedException();
    }
}
