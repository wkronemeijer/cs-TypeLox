namespace TypeLox.Backend.Treewalker;

public static partial class StdLib {
    private sealed class NativeLoxFunction(
        string name,
        int arity,
        NativeLoxFunction.Body body
    ) : ILoxCallable {
        public delegate object? Body(
            Interpreter interpreter,
            SourceRange location,
            List<object?> arguments
        );

        public int Arity => arity;
        public string Name => name;

        public object? Call(
            Interpreter interpreter,
            SourceRange location,
            List<object?> arguments
        ) => body(interpreter, location, arguments);
    }

    private static void DefineFunction(
        this Env env,
        string name,
        int arity,
        NativeLoxFunction.Body body
    ) => env.Define(name, new NativeLoxFunction(name, arity, body));
}

public static partial class StdLib {
    public static void DefineGlobals(Env globals) {
        globals.DefineFunction("clock", 0, (ip, loc, args) => {
            double time = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
            return time;
        });

        globals.DefineFunction("pcall", 1, (ip, loc, args) => {
            var arg = args[0];
            if (arg is not ILoxCallable callable) {
                throw new LoxRuntimeException(loc, $"argument {arg.ToLoxDebugString()} is not callable");
            }

            try {
                callable.Call(ip, loc, []);
                return true;
            } catch (LoxRuntimeException) {
                return false;
            }
        });
    }
}
