namespace TypeLox.Cli;

public sealed class TestRunner(
    // Should we create a new interpreter for each test?
    // Maybe we should instead add a runIsolated flag (or the opposite)
    // to select between variants
    ICompiler compiler,
    IList<Source> sources
) : IDisplay {
    private readonly TestResultCounter tally = new();
    private readonly DiagnosticLog diagnostics = [];

    private void RunTest(Source source) {
        try {
            compiler.RunAsModule(source);
            tally.Add(TestResult.Success);
        } catch (LoxException e) {
            diagnostics.Add(e.ToDiagnostic());
            tally.Add(TestResult.Failure);
        }
    }

    public void RunAllTests() {
        foreach (var source in sources) {
            RunTest(source);
        }
        compiler.Host.WriteLine(this.FormatToString().Trim());
    }

    private void FormatResult(IFormatter f) {
        f.Append($"result: ");
        tally.Format(f);
    }

    public void Format(IFormatter f) {
        FormatResult(f);
        if (!diagnostics.IsOk) {
            diagnostics.Format(f);
            FormatResult(f);
        }
    }
}
