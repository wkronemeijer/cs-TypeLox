namespace TypeLox.Backend.Treewalker;

// We call it env because System.Environment is already a thing
// No interface because this thing has to be fast

public sealed class Env(Env? parent) {
    public Env() : this(null) { }

    public Env? Parent => parent;

    // TODO: Replace object? with an entry to support something like immutable variables
    private readonly Dictionary<string, object?> values = [];

    private static LoxRuntimeException UndefinedVariableException(Token name) {
        return new LoxRuntimeException(name.Location, $"undefined variable '{name.Lexeme}'");
    }

    public void Define(Token name, object? value) {
        // TODO: Check if it already exists?
        values[name.Lexeme] = value;
    }

    public object? Get(Token name) {
        if (values.TryGetValue(name.Lexeme, out var value)) {
            return value;
        } else if (parent is not null) {
            return parent.Get(name);
        } else {
            throw UndefinedVariableException(name);
        }
    }

    public void Assign(Token name, object? value) {
        if (values.ContainsKey(name.Lexeme)) {
            values[name.Lexeme] = value;
        } else if (parent is not null) {
            parent.Assign(name, value);
        } else {
            throw UndefinedVariableException(name);
        }
    }
}
