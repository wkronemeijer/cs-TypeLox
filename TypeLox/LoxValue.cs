namespace TypeLox;

using System.Diagnostics;

public readonly struct LoxValue {
    public interface IVisitor<R> {
        R VisitNil();
        R Visit(bool value);
        R Visit(double value);
        R Visit(string value);
    }

    public readonly object? Value { get; }
    public LoxValue(object? value) {
        if (!CanBeValue(value)) {
            throw new Exception($"value of type {value?.GetType()} can not be a {nameof(LoxValue)}");
        }
        Value = value;
    }

    public static LoxValue Nil { get; } = new(null);
    public static LoxValue True { get; } = new(true);
    public static LoxValue False { get; } = new(false);

    public static LoxValue FromDouble(double d) => new(d);
    public static LoxValue FromString(string s) => new(s);

    public static bool CanBeValue(object? value) => value switch {
        null => true,
        bool => true,
        double => true,
        string => true,
        _ => false,
    };

    public R Accept<R>(IVisitor<R> visitor) => Value switch {
        null => visitor.VisitNil(),
        bool b => visitor.Visit(b),
        double d => visitor.Visit(d),
        string s => visitor.Visit(s),
        _ => throw new UnreachableException(),
    };

    public override string ToString() => Value?.ToString() ?? "nil";

    public bool ToBool() => Value switch {
        null => false,
        false => false,
        0.0 => false,
        "" => false,
        _ => true,
    };
}
