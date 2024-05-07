namespace TypeLox.Cli;

public class Program(ICompiler compiler) : ProgramMode.IVisitor {
    private static ICompiler SelectBackend(ICompilerHost host, ProgramOptions options) => options.Backend switch {
        null => new Backend.Treewalker.Interpreter(host),
        _ => throw new Exception($"unknown backend '{options.Backend}'"),
    };

    public void Visit(ProgramMode.Repl command) {
        var interpreter = compiler.Upgrade();
        while (true) {
            try {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line is null or ".exit") { break; }
                interpreter.RunLine(line);
            } catch (LoxException e) {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }

    public void Visit(ProgramMode.ExecuteFile command) {
        var interpreter = compiler.Upgrade();
        try {
            interpreter.RunFile(command.FileUri);
        } catch (LoxException e) {
            Console.Error.WriteLine(e.ToString());
        }
    }

    // TODO: Add compile to file mode
    // Funny to see how disjoint the feature sets between interpreter and transpiler really is

    public void Visit(ProgramMode.TestDirectory value) {
        var interpreter = compiler.Upgrade();
        var sources = interpreter.Host.FindTestFiles();
        var runner = new TestRunner(interpreter, sources);
        Console.WriteLine($"running {sources.Count} test(s)");
        runner.RunAllTests();
    }

    public static void Main(string[] args) {
        Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;
        var (mode, options) = ArgsParser.Parse(args);
        var host = new CompilerHost(options.CompilerOptions);
        var compiler = SelectBackend(host, options);
        host.WriteLine($"using backend '{compiler.Name}'");
        mode.Accept(new Program(compiler));
    }
}
