namespace Core;

public static class BoolExtensions {
    public static bool Implies(this bool a, bool b) {
        return !a || b;
    }
}
