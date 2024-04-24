namespace TypeLox;

// Unfortunately, `using LoxValue = object?` doesn't work
// Wrapping it in a struct all the time didn't see like a good idea either.

public static class LoxValueObjectExtensions {
    public static bool CanBeLoxValue(this object? value) => value switch {
        null => true,
        bool => true,
        double => true,
        string => true,
        _ => false,
    };

    public static bool IsLoxTruthy(this object? value) => value switch {
        null => false,
        false => false,
        _ => true,
    };

    public static string GetLoxTypeOf(this object? value) => value switch {
        null => "nil",
        bool => "boolean",
        double => "number",
        string => "string",
        _ => throw new ArgumentException($"{value} is not a valid Lox value"),
    };

    public static string ToLoxString(this object? value) => value switch {
        null => "nil",
        bool b => b.ToString(),
        double d => d.ToString(),
        _ => value.ToString() ?? "<err>",
    };

    public static string ToLoxDebugString(this object? value) => value switch {
        string s => $"\"{s}\"",
        _ => value.ToLoxString(),
    };
}
