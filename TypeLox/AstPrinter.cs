namespace TypeLox;

/*
Note:
use reflection on this class
however
because we have aot things enabled, certain reflections features arent available
exercise: find out how we can retain those specific reflection features we use
*/

// TODO: Maybe just put this in AstNode.cs?
public static class AstPrinter {
    private class Visitor(StringBuilder builder) : AstNode.IVisitor<Unit> {
        private Unit Handle(object?[] objects) {
            foreach (var o in objects) {
                builder.Append(' ');
                if (o is null) {
                    builder.Append("null");
                } else if (o is string s) {
                    builder.Append(s);
                } else if (o is Token token) {
                    builder.Append('|');
                    builder.Append(token.Lexeme);
                    builder.Append('|');
                } else if (o is AstNode node) {
                    node.Accept(this);
                } else if (o is IList list) {
                    // Matching on generic types is funny
                    Handle(list.Cast<object?>().ToArray());
                } else {
                    throw new Exception($"unexpected {o.GetType()}");
                }
            }
            return unit;
        }

        private Unit Wrap(string name, params object?[] objects) {
            builder.Append('(');
            builder.Append(name);
            Handle(objects);
            builder.Append(')');
            return unit;
        }

        public Unit Visit(Expr.Assign node) => Wrap("assign", node.Name, node.Value);
        public Unit Visit(Expr.Binary node) => Wrap(node.Operator.Lexeme, node.Left, node.Right);
        public Unit Visit(Expr.Call node) => Wrap("call", node.Callee, node.Arguments);
        public Unit Visit(Expr.Get node) => Wrap("get", node.Target, node.Name);
        public Unit Visit(Expr.Grouping node) => Wrap("group", node.Inner);
        public Unit Visit(Expr.Literal node) => Wrap("literal", node.Value.ToString());
        public Unit Visit(Expr.Logical node) => Wrap(node.Operator.Lexeme, node.Left, node.Right);
        public Unit Visit(Expr.Set node) => Wrap("set", node.Target, node.Name, node.Value);
        public Unit Visit(Expr.Super node) => Wrap("super", node.Name);
        public Unit Visit(Expr.This node) => Wrap("this");
        public Unit Visit(Expr.Unary node) => Wrap(node.Operator.Lexeme, node.Right);
        public Unit Visit(Expr.Variable node) => Wrap("var", node.Name);
        public Unit Visit(Stmt.Block node) => Wrap("block", node.Stmts);
        public Unit Visit(Stmt.Class node) => Wrap("class", node.Name, node.Superclass, node.Methods);
        public Unit Visit(Stmt.Expression node) => Wrap("expr", node.Expr);
        public Unit Visit(Stmt.Function node) => Wrap("fun", node.Name, node.Parameters);
        public Unit Visit(Stmt.If node) => Wrap("if", node.IfTrue, node.IfFalse);
        public Unit Visit(Stmt.Print node) => Wrap("print", node.Expr);
        public Unit Visit(Stmt.Return node) => Wrap("return", node.Expr);
        public Unit Visit(Stmt.Var node) => Wrap("var", node.Name, node.Initializer);
        public Unit Visit(Stmt.While node) => Wrap("while", node.Body);

        public void AppendNode(AstNode node) {
            node.Accept(this);
        }
    }

    public static string ToRepr(this AstNode self) {
        var builder = new StringBuilder();
        new Visitor(builder).AppendNode(self);
        return builder.ToString();
    }
}
