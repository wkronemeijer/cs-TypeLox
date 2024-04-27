namespace Core;

public static class BoolExtensions {
    public static bool Implies(this bool a, bool b) {
        return !a || b;
    }
}

public static class TextExtensions {
    public static string Repeat(this char self, int times) {
        if (times > 0) {
            return new string(self, times);
        } else {
            return string.Empty;
        }
    }

    public static string Repeat(this string self, int times) {
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
