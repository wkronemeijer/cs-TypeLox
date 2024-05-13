namespace TypeLox.Backend.Treewalker;

public sealed class LoxInstance(LoxClass @class) {
    public LoxClass Class { get; } = @class;

    private readonly Dictionary<string, object?> fields = [];

    public object? GetProperty(Token name) {
        var lexeme = name.Lexeme;
        if (fields.TryGetValue(lexeme, out var value)) {
            return value;
        } else if (Class.FindMethod(lexeme) is LoxFunction method) {
            return method.BindThis(this);
        } else {
            // TODO: This is only strict-mode check
            throw new LoxRuntimeException(name.Location,
                $"undefined property '{lexeme}'"
            );
        }
    }

    public void SetProperty(Token name, object? value) {
        fields[name.Lexeme] = value;
    }
}
