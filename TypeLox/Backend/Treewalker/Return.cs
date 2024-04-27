namespace TypeLox.Backend.Treewalker;

public sealed class Return(object? value) : Exception() {
    public object? Value { get; } = value;
}
