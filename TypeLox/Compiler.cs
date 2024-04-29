namespace TypeLox;

// TODO: Should we add a type parameter for intermediate results?
// Otherwise Run/CompileAndRun remains
public interface ICompiler {
    /// <summary>
    /// Descriptive name for this backend.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Host environment, which provides file system and output abstraction.
    /// </summary>
    ICompilerHost Host { get; }

    /// <summary>
    /// Runs a piece of source code in an isolated environment.
    /// </summary>
    void RunAsModule(Source source);

    /// <summary>
    /// Runs a piece of source code in a the shared, global environment.
    /// </summary>
    void RunAsScript(Source source);

    /// <summary>
    /// Runs a source file.
    /// </summary>
    void RunFile(Uri uri) {
        RunAsModule(Host.ReadFile(uri));
    }

    /// <summary>
    /// Runs an indepedent snippet of code.
    /// </summary>
    void RunLine(string snippet) {
        RunAsScript(Host.CreateSourceFromSnippet(snippet));
    }
}
