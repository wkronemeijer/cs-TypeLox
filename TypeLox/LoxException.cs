namespace TypeLox;

public class LoxException(SourceRange location, string message) : Exception(message) {
    public SourceRange Location { get; } = location;
}

// ...but analysis has diagnostics? and almost always you can continue diagnosing
public class LoxCompileException(Diagnostic diagnostic) : LoxException(diagnostic.Location, diagnostic.Message) {
    public Diagnostic Diagnostic { get; } = diagnostic;
}

public class LoxRuntimeException(SourceRange location, string message) : LoxException(location, message);
