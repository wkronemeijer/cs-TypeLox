namespace TypeLox.Backend.Treewalker;

public interface ILoxCallable {
    /// <summary>
    /// Maximum number of arguments the function can take.
    /// </summary>
    public int Arity { get; }

    public string Name { get; }

    /// <summary>
    /// Performs the 
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public object? Call(
        Interpreter interpreter,
        SourceRange location,
        List<object?> arguments
    );
    // Truth is idc about arity;
}
