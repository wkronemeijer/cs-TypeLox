namespace TypeLox.Backend.Treewalker;

public sealed class Return(object? value) : Exception("return") {
    public object? Value { get; } = value;
}
