namespace TypeLox;

public enum DiagnosticKind {
    Info,
    Warning,
    Error,
}

public record class Diagnostic(
    DiagnosticKind Kind,
    SourceRange Location,
    string Message
) {
    public bool IsOk { get; } = Kind switch {
        DiagnosticKind.Error => false,
        _ => true,
    };

    public override string ToString() {
        return $"{Kind.ToString().ToUpperInvariant()} at {Location}:\n{Message}";
    }
}

public interface IDiagnosticLog {
    /// <summary>
    /// Returns whether the log is OK, i.e. when it doesn't contain any errors.
    /// </summary>
    bool IsOk { get; }

    /// <summary>
    /// Throws if the log is not OK, i.e. when it contains any errors.
    /// </summary>
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

public class DiagnosticLog() : IDiagnosticLog, IEnumerable<Diagnostic> {
    public class NotOkException(IDiagnosticLog log) : LoxException("log is not ok") {
        public IDiagnosticLog Log { get; } = log;
    }

    private readonly List<Diagnostic> diagnostics = [];

    public bool IsOk => diagnostics.All(i => i.IsOk);

    public void ThrowIfNotOk() {
        if (!IsOk) {
            throw new NotOkException(this);
        }
    }

    public void Add(Diagnostic diagnostic) {
        diagnostics.Add(diagnostic);
    }

    public IEnumerator<Diagnostic> GetEnumerator() => diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => diagnostics.GetEnumerator();
}
