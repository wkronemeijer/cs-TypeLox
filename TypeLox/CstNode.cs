namespace TypeLox;

/// <summary>
/// Node of the (C)oncrete (S)yntax (T)ree.
/// Called "Concrete" because it includes unneeded details like newlines, comments, groupings, etc.
/// Used for formatting.
/// </summary>
public abstract record class CstNode {
    // TODO: I don't like that AstNode and CstNode differ by 1 letter
    // Maybe call them AbstractNode and ConcreteNode?

    public interface IVisitor<R> {

    }

    public abstract R Accept<R>(IVisitor<R> visitor);

    // Empty for now
}

public static class LowerConcreteNode {
    private class Lowerer : CstNode.IVisitor<AstNode> {

    }

    public static AstNode ToAstNode(this CstNode self) => self.Accept(new Lowerer());
}
