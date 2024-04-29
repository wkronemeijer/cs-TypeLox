namespace TypeLox.Cli;

public sealed class ProgramOptions {
    public CompilerOptions CompilerOptions { get; set; } = new();
    // TODO: Add a --backend option
    public string? Backend { get; set; } = null;
}
