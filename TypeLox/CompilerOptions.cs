namespace TypeLox;

public sealed class CompilerOptions {
    public bool PrintTokens { get; set; } = false;
    public bool PrintTree { get; set; } = false;

    public bool AllowUnderApplication { get; set; } = false;
    public bool AllowOverApplication { get; set; } = false;
}
