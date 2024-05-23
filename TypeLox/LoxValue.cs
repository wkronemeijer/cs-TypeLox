namespace TypeLox;

using TypeLox.Backend.Treewalker;

// Unfortunately, `using LoxValue = object?` doesn't work
// Wrapping it in a struct all the time didn't see like a good idea either.

public static class LoxValueObjectExtensions {
    private static string Inspect(object? value) => value switch {
        string s => $"\"{s}\"",
        _ => value?.ToString() ?? "null",
    };

    private static Exception InvalidValue(object? value) => (
        new Exception($"{Inspect(value)} is not a valid Lox value")
    );

    public static bool CanBeLoxValue(this object? value) => value switch {
        null => true,
        bool => true,
        double => true,
        string => true,
        LoxClass => true,
        LoxFunction => true,
        LoxInstance => true,
        ILoxCallable => true,
        _ => false,
    };

    public static bool IsLoxTruthy(this object? value) => value switch {
        null => false,
        false => false,
        _ => true,
    };

    // Note: because this is an extension method, lhs can be null (unlike Object.Equals)
    public static bool LoxEquals(this object? lhs, object? rhs) => (lhs, rhs) switch {
        (null, null) => true,
        (bool a, bool b) => a == b,
        (double a, double b) => a == b, // TODO: How should NaN be handled? SameValueZero?
        (string a, string b) => a == b,
        _ => ReferenceEquals(lhs, rhs),
    };

    public static string GetLoxTypeOf(this object? value) => value switch {
        null => "nil",
        bool => "boolean",
        double => "number",
        string => "string",
        LoxClass => "class",
        LoxInstance => "object",
        LoxFunction or ILoxCallable => "function",
        _ => throw InvalidValue(value),
    };

    public static string ToLoxString(this object? value) => value switch {
        null => "nil",
        bool b => b ? "true" : "false",
        double d => d.ToString(),
        string s => s,
        LoxClass c => $"<class {c.Name}>",
        LoxInstance i => $"<instance {i.Class.Name}>",
        LoxFunction c => $"<fn {c.Name}>",
        ILoxCallable c => $"<fn {c.Name}>",
        _ => throw InvalidValue(value),
    };

    public static string ToLoxDebugString(this object? value) => value switch {
        string s => $"\"{s}\"",
        _ => value.ToLoxString(),
    };
}
