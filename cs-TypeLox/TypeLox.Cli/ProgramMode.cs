namespace TypeLox.Cli;

public abstract record class ProgramMode() {
    public interface IVisitor {
        int Visit(Repl value);
        int Visit(ExecuteFile value);
        int Visit(TestDirectory value);
    }

    public abstract int Accept(IVisitor visitor);

    /// <summary>
    /// Runs the program as an interactive prompt.
    /// </summary>
    public record class Repl() : ProgramMode() {
        public override int Accept(IVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Reads the file, then executes it, then terminates.
    /// </summary>
    public record class ExecuteFile(Uri FileUri) : ProgramMode() {
        public override int Accept(IVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Finds all files ./**/*.test.lox and runs them for as tests.
    /// </summary>
    public record class TestDirectory() : ProgramMode() {
        public override int Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
