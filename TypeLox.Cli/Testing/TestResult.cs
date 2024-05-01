namespace TypeLox.Cli;

using System.Diagnostics;

using static TestResult;

public enum TestResult {
    Undecisive,
    Success,
    Failure,
}

public static class TestResultMethods {
    public static string GetFormattedName(this TestResult self) => self switch {
        Undecisive => "undecided",
        Success => "passed",
        Failure => "failed",
        _ => throw new UnreachableException(),
    };
}
