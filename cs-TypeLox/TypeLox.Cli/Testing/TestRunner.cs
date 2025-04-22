namespace TypeLox.Cli;

public sealed class TestRunner(
    IInterpreter interpreter,
    IList<Source> sources
) : IDisplay {
    private readonly TestResultCounter tally = new();
    private readonly DiagnosticList diagnostics = [];

    private void RunTest(Source source) {
        try {
            interpreter.Run(source);
            tally.Add(TestResult.Success);
        } catch (LoxException e) {
            diagnostics.Add(e.ToDiagnostic());
            tally.Add(TestResult.Failure);
        }
    }

    public void RunAllTests() {
        foreach (var s in sources) { RunTest(s); }
        interpreter.Host.WriteLine(this.FormatToString().Trim());
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
