namespace Core;

public interface IDisplay {
    void Format(IFormatter f);
}

public static class Display {
    public static string FormatToString(this IDisplay self) {
        var f = new Formatter();
        self.Format(f);
        return f.ToString();
    }
}
