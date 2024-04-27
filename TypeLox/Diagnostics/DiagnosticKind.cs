namespace TypeLox;

using System.Diagnostics;

using static DiagnosticKind;

public enum DiagnosticKind {
    Info,
    Warning,
    Error,
}

public static class DiagnosticKindMethods {
    public static bool GetIsOK(this DiagnosticKind self) => self switch {
        Error => false,
        _ => true,
    };

    public static AnsiStyle GetColor(this DiagnosticKind self) => self switch {
        Info => AnsiStyle.BrightBlueLetter,
        Warning => AnsiStyle.BrightYellowLetter,
        Error => AnsiStyle.BrightRedLetter,
        _ => throw new UnreachableException(),
    };
}
