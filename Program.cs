using TypeLox;

class Program(IInterpreter interpreter) : ProgramMode.IVisitor {
    void ProgramMode.IVisitor.Visit(ProgramMode.Repl command) {
        while (true) {
            try {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line is null or ".exit") { break; }
                interpreter.RunSnippet(line);
            } catch (LoxException e) {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }

    void ProgramMode.IVisitor.Visit(ProgramMode.ExecuteFile command) {
        interpreter.RunFile(command.Uri);
    }

    static void Main(string[] args) {
        var (mode, options) = ProgramMode.Parse(args);
        var host = new RealCompilerHost();
        var interpreter = new Interpreter(host, options);
        var program = new Program(interpreter);
        mode.Accept(program);
    }
}
