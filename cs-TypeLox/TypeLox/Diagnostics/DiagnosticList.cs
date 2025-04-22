namespace TypeLox;

using static DiagnosticKind;

public class DiagnosticList() : IEnumerable<Diagnostic>, IDisplay {
    private readonly List<Diagnostic> diagnostics = [];

    private Diagnostic? FindFirstError() {
        foreach (var diag in diagnostics) {
            if (!diag.IsOk) {
                return diag;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns whether the log is OK, i.e. when it does not contain any errors.
    /// </summary>
    public bool IsOk => FindFirstError() is null;

    /// <summary>
    /// Throws if the log is not OK, i.e. when it contains any errors.
    /// </summary>
    /// <exception cref="LoxException"/>
    public void ThrowIfNotOk() {
        if (FindFirstError() is Diagnostic d) {
            throw d.ToException();
        }
    }

    /// <summary>
    /// Adds a diagnostic to the log. 
    /// <see cref="IsOk"/> will get updated based on the severity.
    /// </summary>
    public void Add(Diagnostic diagnostic) {
        diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Shorthand for creating a informational diagnostic
    /// </summary>
    public void AddInfo(SourceRange range, string message) {
        Add(new Diagnostic(Info, range, message));
    }

    /// <summary>
    /// Shorthand for creating a warning diagnostic.
    /// </summary>
    public void AddWarning(SourceRange range, string message) {
        Add(new Diagnostic(Warning, range, message));
    }

    /// <summary>
    /// Shorthand for creating an error diagnostic.
    /// </summary>
    public void AddError(SourceRange range, string message) {
        Add(new Diagnostic(Error, range, message));
    }

    /// <summary>
    /// Adds all diagnostics in an enumerable.
    /// </summary>
    /// <param name="diagnostics"></param>
    public void AddAll(IEnumerable<Diagnostic> diagnostics) {
        // if (diagnostics.TryGetNonEnumeratedCount(out var size)) {
        //     this.diagnostics.EnsureCapacity(this.diagnostics.Count + size);
        // }
        foreach (var d in diagnostics) { Add(d); }
    }

    public IEnumerator<Diagnostic> GetEnumerator() => diagnostics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => diagnostics.GetEnumerator();

    public void Format(IFormatter f) {
        foreach (var diag in diagnostics) {
            diag.Format(f);
            f.AppendLine();
        }
    }

    public string CreateReport() => this.FormatToString().Trim();
}
