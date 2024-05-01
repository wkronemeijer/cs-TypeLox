namespace TypeLox;

public class LoxException(SourceRange location, string message) : Exception(message) {
    public SourceRange Location { get; } = location;

    public Diagnostic ToDiagnostic() => new(DiagnosticKind.Error, Location, Message);
    public override string ToString() => ToDiagnostic().ToString();
}
