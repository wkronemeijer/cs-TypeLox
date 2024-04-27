namespace Core;

public interface INestedStringBuilder {
    void Append(char c);
    void Append(string s);

    void AppendLine();
    void AppendLine(string s);

    // Object has ToString()

    void Indent();
    void Dedent();
}

/// <summary>
/// Like <see cref="StringBuilder"/>, 
/// except it allows you to indent code more easily.
/// </summary>
public class NestedStringBuilder(string indent) : INestedStringBuilder {
    public NestedStringBuilder(int count) : this(new string(' ', count)) { }
    public NestedStringBuilder() : this(4) { }

    private readonly StringBuilder result = new();

    private int level = 0;
    private bool primed = false;

    private static readonly char newline = '\n';

    void TryInsertIndentation() {
        if (primed) {
            if (level > 0) {
                result.Append(indent.Repeat(level));
            }
            primed = false;
        }
    }

    public void Append(char c) {
        TryInsertIndentation();
        result.Append(c);
        if (c == newline) {
            primed = true;
        }
    }

    public void Append(string s) {
        TryInsertIndentation();
        result.Append(s);
        if (s.Contains(newline)) {
            primed = true;
        }
    }

    public void AppendLine() => Append(newline);

    public void AppendLine(string s) {
        Append(s);
        AppendLine();
    }

    public void Indent() {
        level += 1;
    }

    public void Dedent() {
        level -= 1;
        Ensures(level >= 0, "too many dedents");
    }

    public override string ToString() {
        return result.ToString();
    }
}
