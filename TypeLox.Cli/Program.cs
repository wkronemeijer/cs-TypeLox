namespace TypeLox.Cli;

using TypeLox.Backend.Treewalker;

public class Program(ICompiler compiler) : ProgramMode.IVisitor {
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
        var (mode, options) = ProgramMode.Parse(args);
        var host = new RealCompilerHost(options.CompilerOptions);
        // TODO: Read program options to pick which backend to use
        var compiler = new TreeWalkInterpreter(host);
        mode.Accept(new Program(compiler));
    }
}
