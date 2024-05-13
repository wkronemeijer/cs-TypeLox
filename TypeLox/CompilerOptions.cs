namespace TypeLox;

public sealed class CompilerOptions {
    public bool? PrintTokens { get; set; } = null;
    public bool? PrintTree { get; set; } = null;
    public bool? PrintLocals { get; set; } = null;

    public bool? AllowUnderApplication { get; set; } = null;
    public bool? AllowOverApplication { get; set; } = null;

    public bool? TrackEvaluation { get; set; } = null;
    public bool? DisablePrint { get; set; } = null;
}
