namespace TypeLox.Backend.Treewalker;

public sealed class LoxFunction(Stmt.Function decl) : ILoxCallable {
    public int Arity => decl.Parameters.Count;
    public bool IsNative => false;
    public string Name => decl.Name.Lexeme;

    public object? Call(
        IInterpreter interpreter,
        SourceRange location,
        List<object?> arguments
    ) {
        var env = new Env(); // TODO: This is where the closure goes
        var expectedCount = Arity;
        var actualCount = arguments.Count;

        var copyCount = Math.Min(expectedCount, actualCount);

        // Present parameters
        for (var i = 0; i < copyCount; i++) {
            env.Define(decl.Parameters[i].Lexeme, arguments[i]);
        }

        if (copyCount < expectedCount) {
            if (!interpreter.Host.Options.AllowUnderApplication) {
                throw new LoxRuntimeException(location,
                    $"too few arguments for {this.ToLoxString()}"
                );
            }
            // Supply missing arguments
            for (var j = copyCount; j < expectedCount; j++) {
                env.Define(decl.Parameters[j].Lexeme, null);
            }
        } else if (copyCount < actualCount) {
            if (!interpreter.Host.Options.AllowOverApplication) {
                throw new LoxRuntimeException(location,
                    $"too many arguments for {this.ToLoxString()}"
                );
            }
        } // else (copyCount == actualCount == expectedCount) { 👍 }

        try {
            interpreter.ExecuteBlock(decl.Statements, env);
        } catch (Return r) {
            return r.Value;
        }
        return null;
    }
}
