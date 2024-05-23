namespace TypeLox.Backend.Treewalker;

// We call it env because System.Environment is already a thing
// No interface because this thing has to be fast

public sealed class Env(Env? parent) : IDisplay {
    public Env() : this(null) { }

    public Env? Parent => parent;

    // TODO: Replace object? with an entry to support something like immutable variables
    private readonly Dictionary<string, object?> values = [];

    private static LoxRuntimeException UndefinedVariableException(
        Token name
    ) => new(name.Location, $"undefined variable '{name.Lexeme}'");

    public void Define(string name, object? value) {
        // TODO: Check if it already exists?
        values[name] = value;
    }

    public void Define(Token name, object? value) => Define(name.Lexeme, value);

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

    Env GetAncestor(int depth) {
        var cursor = this;
        for (var i = 0; i < depth; i++) {
            if (cursor.Parent is Env parent) {
                cursor = parent;
            } else {
                throw new Exception($"ancestor is not deep enough");
            }
        }
        return cursor;
    }

    public object? GetAt(string name, int depth) {
        if (GetAncestor(depth).values.TryGetValue(name, out var result)) {
            return result;
        } else {
            throw new Exception($"could not find '{name}' at depth {depth}");
        }
    }

    public object? GetAt(Token name, int depth) => GetAt(name.Lexeme, depth);

    public void AssignAt(Token name, int depth, object? value) {
        GetAncestor(depth).values[name.Lexeme] = value;
    }

    public void Format(IFormatter f) {
        var oldIndent = f.CurrentIndentation;
        var depth = 0;
        for (Env? cursor = this; cursor is not null; cursor = cursor.Parent, depth++) {
            f.AppendLine($"depth {depth}:");
            f.Indent();
            foreach (var (key, value) in cursor.values) {
                f.Append(key.ToLoxDebugString());
                f.Append(" --> ");
                f.Append(value.ToLoxDebugString());
                f.AppendLine();
            }
            f.AppendLine();
        }

        f.CurrentIndentation = oldIndent;
    }
}
