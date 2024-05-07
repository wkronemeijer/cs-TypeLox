namespace TypeLox.Backend.Treewalker;

public interface ILoxIndexable {
    public object? this[object? index] { get; set; }
}

public sealed class LoxInstance() {

}
