namespace TypeLox;

using static ModuleKind;

public enum ModuleKind {
    SourceFile,
    Declaration,
    ReplLine,
}

public static class ModuleKindMethods {
    public static bool IsIsolated(this ModuleKind self) => self switch {
        ReplLine => false,
        _ => true,
    };
}
