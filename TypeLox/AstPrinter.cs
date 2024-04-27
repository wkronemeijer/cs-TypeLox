namespace TypeLox;

public static class AstNodePrinter {
    private class Visitor(IFormatter f) : AstNode.IVisitor<Unit> {
        private void Handle(object? o) {
            if (o is null) {
                f.Append("null");
            } else if (o is string s) {
                f.Append(s);
            } else if (o is Token token) {
                token.FormatLexeme(f);
            } else if (o is AstNode node) {
                node.Accept(this);
            } else if (o is IList list) {
                WrapArray(list.Cast<object?>().ToArray());
            } else {
                throw new Exception($"unexpected {o.GetType()}");
            }
        }

        private Unit WrapArray(object?[] objects) {
            f.Append('[');
            f.Indent();
            f.AppendLine();
            foreach (var o in objects) {
                Handle(o);
                f.AppendLine();
            }
            f.Dedent();
            f.Append(']');
            return unit;
        }

        private Unit Wrap(string name, params object?[] objects) {
            f.Append('(');
            f.Append(name);
            foreach (var o in objects) {
                f.Append(' ');
                Handle(o);
            }
            f.Append(')');
            return unit;
        }

        public Unit Visit(Expr.Assign node) => Wrap("assign", node.Name, node.Value);
        public Unit Visit(Expr.Binary node) => Wrap(node.Operator.Lexeme, node.Left, node.Right);
        public Unit Visit(Expr.Call node) => Wrap("call", node.Callee, node.Arguments);
        public Unit Visit(Expr.GetProperty node) => Wrap("get", node.Target, node.Name);
        public Unit Visit(Expr.Grouping node) => Wrap("group", node.Inner);
        public Unit Visit(Expr.Literal node) => Wrap("literal", node.Value.ToLoxDebugString());
        public Unit Visit(Expr.Logical node) => Wrap(node.Operator.Lexeme, node.Left, node.Right);
        public Unit Visit(Expr.SetProperty node) => Wrap("set", node.Target, node.Name, node.Value);
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

        public void Visit(IEnumerable<Stmt> statements) {
            foreach (var stmt in statements) {
                stmt.Accept(this);
                f.AppendLine();
            }
        }
    }

    public static string ToDebugString(this AstNode node) {
        var formatter = new Formatter();
        node.Accept(new Visitor(formatter));
        return formatter.ToString().Trim();
    }

    // Very tempting still to add a SourceFile node type
    // How about a ParsedSource class?
    // Reason why it needs to be seperate is because 
    // variables defined in earlier repl prompts need to be remembered
    public static string ToDebugString(this IEnumerable<Stmt> statements) {
        var formatter = new Formatter();
        var visitor = new Visitor(formatter);
        visitor.Visit(statements);
        return formatter.ToString().Trim();
    }
}
