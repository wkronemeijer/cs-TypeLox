namespace TypeLox;

public sealed class LocalDepth() : IEnumerable<(Expr, int)>, IDisplay {
    // Records define their own equality, we really need identity for this
    private readonly Dictionary<Expr, int> locals = new(
        ReferenceEqualityComparer.Instance
    );

    public void ResolveLocal(Expr expr, int depth) {
        locals[expr] = depth;
    }

    public int? GetDepth(Expr expr) {
        return locals.TryGetValue(expr, out var depth) ? depth : null;
    }

    public void Merge(LocalDepth other) {
        foreach (var (expr, depth) in other) {
            ResolveLocal(expr, depth);
        }
    }

    public IEnumerator<(Expr, int)> GetEnumerator() => (
        locals
        .Select(pair => (pair.Key, pair.Value))
        .GetEnumerator()
    );

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Format(IFormatter f) {
        foreach (var (e, d) in this) {
            f.Append($"at depth {d}: ");
            e.Format(f);
            f.AppendLine();
        }
    }
}
