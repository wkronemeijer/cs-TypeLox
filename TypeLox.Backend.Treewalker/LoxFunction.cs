namespace TypeLox.Backend.Treewalker;

public sealed class LoxFunction(
    Stmt.Function decl,
    Env closure
) : ILoxCallable {
    public int Arity { get; } = decl.Parameters.Count;
    public string Name { get; } = decl.Name.Lexeme;

    static readonly string @this = TokenKind.THIS.GetLexeme();

    public LoxFunction BindThis(LoxInstance instance) {
        var boundEnv = new Env(closure);
        boundEnv.Define(@this, instance);
        return new(decl, boundEnv);
    }

    public object? Call(
        Interpreter interpreter,
        SourceRange location,
        List<object?> arguments
    ) {
        var env = new Env(closure);
        var expectedCount = Arity; // == decl.Parameters.Count
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

        object? result = null;
        try {
            interpreter.ExecuteBlock(decl.Statements, env);
        } catch (Return r) {
            result = r.Value;
        }

        if (decl.Kind is FunctionKind.Initializer) {
            // Get takes a token, because it needs a location for the runtime exception
            // GetAt fails with a general Exception
            return closure.GetAt(@this, 0);
        } else {
            return result;
        }
    }
}
