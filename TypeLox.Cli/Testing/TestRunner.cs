namespace TypeLox.Cli;

using System.Diagnostics;

using static TestResultKind;

public enum TestResultKind {
    Undecisive,
    Success,
    Failure,
}

public static class TestResultKindMethods {
    public static string GetFormattedName(this TestResultKind self) => self switch {
        Undecisive => "skipped",
        Success => "passed",
        Failure => "failed",
        _ => throw new UnreachableException(),
    };
}

public sealed class TestResultCounter() : IDisplay {
    public int Total { get; private set; } = 0;
    private readonly Dictionary<TestResultKind, int> countByKind = [];

    public void Add(TestResultKind kind) {
        Console.WriteLine($"Inc({kind})");
        countByKind.TryGetValue(kind, out var count);
        countByKind[kind] = count + 1;
        Total += 1;
    }

    public void Format(IFormatter f) {
        foreach (var kind in Enum.GetValues<TestResultKind>()) {
            countByKind.TryGetValue(kind, out var count);
            f.Append(count);
            f.Append(' ');
            f.Append(kind.ToString());
            f.Append(", ");
        }
        f.Append(" (");
        f.Append(Total);
        f.Append(" total)");
    }
}

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
            tally.Add(Success);
        } catch (LoxException e) {
            diagnostics.Add(e.Diagnostic);
            tally.Add(Failure);
        }
    }

    public void Format(IFormatter f) {
        f.Include(tally);
        if (!diagnostics.IsOk) {
            f.Include(diagnostics);
            f.Include(tally);
        }
    }
    public void RunAllTests() {
        foreach (var source in sources) {
            RunTest(source);
        }
        compiler.Host.WriteLine(this.FormatToString().Trim());
    }
}
