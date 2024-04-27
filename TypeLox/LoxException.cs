namespace TypeLox;

public class LoxException(Diagnostic diagnostic) : Exception(diagnostic.Message) {
    public Diagnostic Diagnostic { get; } = diagnostic;
    public SourceRange Location { get; } = diagnostic.Location;

    public override string ToString() {
        return $"{Diagnostic}\n{StackTrace}";
    }
}

// ...but analysis has diagnostics? and almost always you can continue diagnosing
public class LoxCompileException(Diagnostic diagnostic) : LoxException(diagnostic) { }

public class LoxRuntimeException(SourceRange location, string message) : LoxException(new(DiagnosticKind.Error, location, message));
