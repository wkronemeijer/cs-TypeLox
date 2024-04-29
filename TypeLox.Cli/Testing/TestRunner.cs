namespace TypeLox.Cli;

using TypeLox.Backend.Treewalker;

public enum TestResultKind {
    Undecisive,
    Success,
    Failure,
}

public sealed class TestSuiteResult() {
    private readonly Dictionary<TestResultKind, int> countByKind = [];

    public void Add(TestResultKind kind) {
        countByKind.TryGetValue(kind, out var count);
        countByKind[kind] = count + 1;
    }
}

public sealed class TestRunner(
    // Should we create a new interpreter for each test?
    // Maybe we should instead add a runIsolated flag (or the opposite)
    // to select between variants
    ICompiler compiler,
    IList<Source> sources
) {
    private readonly TestSuiteResult tally = new();

    private readonly DiagnosticLog diagnostics = [];

    private void RunTest(Source source) {
        try {
            compiler.RunAsModule(source);
            tally.Add(TestResultKind.Success);
        } catch (LoxException e) {
            diagnostics.Add(e.Diagnostic);
            tally.Add(TestResultKind.Failure);
        }
    }

    public void RunAllTests() {
        foreach (var source in sources) {
            RunTest(source);
        }
        if (!diagnostics.IsOk) {
            compiler.Host.WriteLine(diagnostics.CreateReport());
        }
    }
}
