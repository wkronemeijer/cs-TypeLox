namespace TypeLox;

interface IInterpreter {
    ISourceRepository Repository { get; } // needed to resolve `require` calls
    void Run(Source source);

    void RunFile(Uri uri) {
        Run(Repository.CreateSourceFromFile(uri));
    }

    void RunSnippet(string snippet) {
        Run(Repository.CreateSourceFromSnippet(snippet));
    }
}

class Interpreter(ISourceRepository repository, ProgramOptions options) : IInterpreter {
    public ISourceRepository Repository => repository;

    public void Run(Source source) {
        // Idea: configurable run pipeline
        var log = new DiagnosticLog();
        try {
            var tokens = new Scanner(log, source).ScanAll();
            log.ThrowIfNotOk();

            if (options.PrintTokens) {
                foreach (var token in tokens) {
                    Console.WriteLine(token.ToString());
                }
            }

            var root = new Parser(log, tokens).Parse();
            log.ThrowIfNotOk();
            if (options.PrintTree) { Console.WriteLine(root.ToDebugString()); }
        } catch (DiagnosticLog.NotOkException) {
            // ignore it
            // silly control flow
            // should we use goto instead?
        } finally {

            Console.WriteLine(log.ToString());

        }
    }
}
