namespace TypeLox.Cli;

using TypeLox.Backend.Treewalker;

public class Program(ICompiler compiler) : ProgramMode.IVisitor {
    private static ICompiler SelectBackend(ICompilerHost host, ProgramOptions options) => options.Backend switch {
        null => new TreeWalkInterpreter(host),
        _ => throw new Exception($"unknown backend '{options.Backend}'"),
    };

    public void Visit(ProgramMode.Repl command) {
        while (true) {
            try {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line is null or ".exit") { break; }
                compiler.RunLine(line);
            } catch (LoxException e) {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }

    public void Visit(ProgramMode.ExecuteFile command) {
        try {
            compiler.RunFile(command.FileUri);
        } catch (LoxException e) {
            Console.Error.WriteLine(e.ToString());
        }
    }

    public void Visit(ProgramMode.TestDirectory value) {
        var uri = value.DirectoryUri ?? compiler.Host.GetCurrentDirectory();
        var sources = compiler.Host.ReadDirectory(uri);
        var runner = new TestRunner(compiler, sources);
        runner.RunAllTests();
    }

    public static void Main(string[] args) {
        Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;
        var (mode, options) = ArgsParser.Parse(args);
        var host = new CompilerHost(options.CompilerOptions);
        var compiler = SelectBackend(host, options);
        host.WriteLine($"Using backend '{compiler.Name}'.");
        mode.Accept(new Program(compiler));
    }
}
