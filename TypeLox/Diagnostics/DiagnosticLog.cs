namespace TypeLox;

public interface IDiagnosticLog {
    /// <summary>
    /// Returns whether the log is OK, i.e. when it doesn't contain any errors.
    /// </summary>
    bool IsOk { get; }

    /// <summary>
    /// Throws if the log is not OK, i.e. when it contains any errors.
    /// </summary>
    /// <exception cref="LoxCompileException"/>
    void ThrowIfNotOk();

    /// <summary>
    /// Adds a diagnostic to the log. 
    /// <see cref="IsOk"/> will get updated based on the severity.
    /// </summary>
    void Add(Diagnostic diagnostic);

    /// <summary>
    /// Shorthand for creating a informational diagnostic
    /// </summary>
    void Info(SourceRange range, string message) {
        Add(new Diagnostic(DiagnosticKind.Info, range, message));
    }

    /// <summary>
    /// Shorthand for creating a warning diagnostic.
    /// </summary>
    void Warn(SourceRange range, string message) {
        Add(new Diagnostic(DiagnosticKind.Warning, range, message));
    }

    /// <summary>
    /// Shorthand for creating an error diagnostic.
    /// </summary>
    void Error(SourceRange range, string message) {
        Add(new Diagnostic(DiagnosticKind.Error, range, message));
    }
}

public class DiagnosticLog() : IDiagnosticLog, IBuildable, IEnumerable<Diagnostic> {
    private readonly List<Diagnostic> diagnostics = [];

    private Diagnostic? FindFirstError() {
        foreach (var diag in diagnostics) {
            if (!diag.IsOk) {
                return diag;
            }
        }
        return null;
    }

    public bool IsOk => FindFirstError() is null;

    public void ThrowIfNotOk() {
        if (FindFirstError() is Diagnostic d) {
            throw new LoxCompileException(d);
        }
    }

    public void Add(Diagnostic diagnostic) {
        diagnostics.Add(diagnostic);
    }

    public IEnumerator<Diagnostic> GetEnumerator() => diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => diagnostics.GetEnumerator();

    public void Format(StringBuilder b) {
        foreach (var diag in diagnostics) {
            diag.Format(b);
            b.AppendLine();
        }
    }

    public string CreateReport() {
        var result = new StringBuilder();
        result.Include(this);
        return result.ToString().Trim();
    }
}
