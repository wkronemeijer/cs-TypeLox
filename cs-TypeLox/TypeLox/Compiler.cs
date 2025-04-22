namespace TypeLox;

// TODO: Technically this should output a TCompileResult, which : ICompileResult, which is essentially a wrapper around a string
// running code in-memory is already a funny feature
// 3 stages:
// output to file
// run file
// run lines

public interface ICompiler {
    /// <summary>
    /// Descriptive name for this backend.
    /// </summary>
    public string Name { get; }

    public IEnumerable<string> GetAliases() { yield break; }

    /// <summary>
    /// Host environment, which provides file system and output abstraction.
    /// </summary>
    public ICompilerHost Host { get; }

    public Stmt.Module? Compile(Source source, DiagnosticList diagnostics);

    public IInterpreter Upgrade() {
        if (this is IInterpreter interpreter) {
            return interpreter;
        } else {
            throw new Exception($"backend {Name} does not support interpreter mode");
        }
    }
}

public interface IInterpreter : ICompiler {
    public void Run(Source source);

    public void RunFile(Uri uri) => Run(Host.ReadFile(uri));
    public void RunLine(string snippet) => Run(Source.FromString(snippet) with { CameFromRepl = true });
}
