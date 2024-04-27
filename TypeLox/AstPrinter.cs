namespace TypeLox;

public abstract partial record class AstNode : IDebug {
    private class Visitor(NestedStringBuilder builder) : IVisitor<Unit> {
        private void Handle(object? o) {
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
                WrapArray(list.Cast<object?>().ToArray());
            } else {
                throw new Exception($"unexpected {o.GetType()}");
            }
        }

        private Unit WrapArray(object?[] objects) {
            builder.Append('[');
            builder.Indent();
            builder.AppendLine();
            foreach (var o in objects) {
                Handle(o);
                builder.AppendLine();
            }
            builder.Dedent();
            builder.Append(']');
            return unit;
        }

        private Unit Wrap(string name, params object?[] objects) {
            builder.Append('(');
            builder.Append(name);
            foreach (var o in objects) {
                builder.Append(' ');
                Handle(o);
            }
            builder.Append(')');
            return unit;
        }

        public Unit Visit(Expr.Assign node) => Wrap("assign", node.Name, node.Value);
        public Unit Visit(Expr.Binary node) => Wrap(node.Operator.Lexeme, node.Left, node.Right);
        public Unit Visit(Expr.Call node) => Wrap("call", node.Callee, node.Arguments);
        public Unit Visit(Expr.Get node) => Wrap("get", node.Target, node.Name);
        public Unit Visit(Expr.Grouping node) => Wrap("group", node.Inner);
        public Unit Visit(Expr.Literal node) => Wrap("literal", node.Value.ToLoxDebugString());
        public Unit Visit(Expr.Logical node) => Wrap(node.Operator.Lexeme, node.Left, node.Right);
        public Unit Visit(Expr.Set node) => Wrap("set", node.Target, node.Name, node.Value);
        public Unit Visit(Expr.Super node) => Wrap("super", node.Name);
        public Unit Visit(Expr.This node) => Wrap("this");
        public Unit Visit(Expr.Unary node) => Wrap(node.Operator.Lexeme, node.Right);
        public Unit Visit(Expr.Variable node) => Wrap("var", node.Name);
        public Unit Visit(Stmt.Block node) => Wrap("block", node.Statements);
        public Unit Visit(Stmt.Class node) => Wrap("class", node.Name, node.Superclass, node.Methods);
        public Unit Visit(Stmt.Expression node) => Wrap("expr", node.Expr);
        public Unit Visit(Stmt.Function node) => Wrap("fun", node.Name, node.Parameters, node.Body);
        public Unit Visit(Stmt.If node) => Wrap("if", node.IfTrue, node.IfFalse);
        public Unit Visit(Stmt.Print node) => Wrap("print", node.Expr);
        public Unit Visit(Stmt.Return node) => Wrap("return", node.Expr);
        public Unit Visit(Stmt.Var node) => Wrap("var", node.Name, node.Initializer);
        public Unit Visit(Stmt.While node) => Wrap("while", node.Body);
    }

    public string ToDebugString() {
        var builder = new NestedStringBuilder();
        Accept(new Visitor(builder));
        return builder.ToString();
    }
}
