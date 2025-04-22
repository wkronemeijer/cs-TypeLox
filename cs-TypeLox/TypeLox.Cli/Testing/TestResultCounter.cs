namespace TypeLox.Cli;

public sealed class TestResultCounter() : IDisplay {
    private readonly Dictionary<TestResult, int> countByKind = [];

    public void Add(TestResult kind) {
        countByKind.TryGetValue(kind, out var count);
        countByKind[kind] = count + 1;
    }

    public void Format(IFormatter f) {
        var first = true;
        foreach (var kind in Enum.GetValues<TestResult>()) {
            countByKind.TryGetValue(kind, out var count);
            if (count > 0) {
                if (first) {
                    first = false;
                } else {
                    f.Append(", ");
                }
                f.Append(count);
                f.Append(' ');
                f.Append(kind.GetFormattedName());
            }
        }
    }
}
