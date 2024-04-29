namespace TypeLox.Cli;

public abstract record class ProgramMode() {
    public interface IVisitor {
        void Visit(Repl value);
        void Visit(ExecuteFile value);
        void Visit(TestDirectory value);
    }

    public abstract void Accept(IVisitor visitor);

    /// <summary>
    /// Runs the program as an interactive prompt.
    /// </summary>
    public record class Repl() : ProgramMode() {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Reads the file, then executes it, then terminates.
    /// </summary>
    public record class ExecuteFile(Uri FileUri) : ProgramMode() {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Reads the file, then executes it, then terminates.
    /// </summary>
    public record class TestDirectory(Uri? DirectoryUri) : ProgramMode() {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    ////////////
    // Parser //
    ////////////


}
