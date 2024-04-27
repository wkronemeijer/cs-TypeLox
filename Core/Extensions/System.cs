namespace Core;

public static class BoolExtensions {
    public static bool Implies(this bool a, bool b) {
        return !a || b;
    }
}

public static class StringExtensions {
    public static string Repeat(this string self, int times) {
        // Requires(times >= 0, "'repeats' must be non-negative");
        // Just ignore negative counts
        if (times > 0) {
            var result = new StringBuilder(self.Length * times);
            for (var i = 0; i < times; i++) {
                result.Append(self);
            }
            return result.ToString();
        } else {
            return string.Empty;
        }
    }
}
