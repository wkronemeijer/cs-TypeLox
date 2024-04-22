using TypeLox;

class Program(IInterpreter host) : ProgramMode.IVisitor {
    void ProgramMode.IVisitor.Visit(ProgramMode.Repl command) {
        while (true) {
            try {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line is null or ".exit") { break; }
                host.RunSnippet(line);
            } catch (LoxException e) {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }

    void ProgramMode.IVisitor.Visit(ProgramMode.ExecuteFile command) {
        host.RunFile(command.Uri);
    }

    static void Main(string[] args) {
        // Muh DI
        var (mode, options) = ProgramMode.Parse(args);
        var repo = new FileSystemSourceRepository();
        var host = new Interpreter(repo, options);
        var program = new Program(host);
        mode.Accept(program);
    }
}
