namespace TypeLox.Backend.Treewalker;

using System.Collections.Generic;

public sealed class LoxClass(
    Token name,
    LoxClass? super,
    LoxFunction? initializer,
    Dictionary<string, LoxFunction> methods
) : ILoxCallable {
    public string Name { get; } = name.Lexeme;
    public LoxClass? SuperClass { get; } = super;

    public LoxFunction? FindMethod(string name) {
        if (methods.TryGetValue(name, out var method)) {
            return method;
        } else if (SuperClass is LoxClass super) {
            return super.FindMethod(name);
        } else {
            return null;
        }
    }

    ////////////////////
    // : ILoxCallable //
    ////////////////////

    public int Arity => initializer is LoxFunction init ? init.Arity : 0;

    public object? Call(
        Interpreter interpreter,
        SourceRange location,
        List<object?> arguments
    ) {
        var instance = new LoxInstance(this);
        if (initializer is LoxFunction init) {
            init.BindThis(instance).Call(interpreter, location, arguments);
        }
        return instance;
    }
}
