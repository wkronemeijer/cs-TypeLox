namespace TypeLox;

public class LoxException(Diagnostic diagnostic) : Exception(diagnostic.Message) {
    public Diagnostic Diagnostic { get; } = diagnostic;
    public SourceRange Location { get; } = diagnostic.Location;

    public override string ToString() => Diagnostic.ToString();
}
