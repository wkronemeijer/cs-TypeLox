namespace Core;

public interface IFormatter {
    //////////////
    // Required //
    //////////////

    void Append(char c);
    void Append(string s);

    void Indent();
    void Dedent();

    int CurrentIndentation { get; set; }

    ////////////////
    // Extensions //
    ////////////////

    void AppendLine() => Append('\n');
    void AppendLine(string s) {
        Append(s);
        AppendLine();
    }

    void Include(IDisplay format) => format.Format(this);
    void IncludeLine(IDisplay format) {
        Include(format);
        AppendLine();
    }

    ///////////////
    // Shortcuts //
    ///////////////

    // You can add a lot of these...
    void Append(byte value) => Append(value.ToString());
    void Append(int value) => Append(value.ToString());
    // Should we do Append(object?)?
    // You can just call ToString yourself...

    /// <summary>
    /// Returns the final, formatted string.
    /// </summary>
    string ToString();
}

public sealed class Formatter(string indent) : IFormatter {
    public Formatter(int count) : this(new string(' ', count)) { }
    public Formatter() : this(4) { }

    private readonly StringBuilder result = new();
    private bool primed = false;
    private int level = 0;

    void TryInsertIndentation() {
        if (primed) {
            if (level > 0) {
                result.Append(indent.Repeat(level));
            }
            primed = false;
        }
    }

    private const char newline = '\n';
    // private static readonly char powerlineRight = '\uE0B0';
    // private static readonly char powerlineLeft = '\uE0B2';

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

    public int CurrentIndentation {
        get => level;
        set => level = value >= 0 ? value : throw new ArgumentOutOfRangeException(
            nameof(value),
            value,
            "parameter must be non-negative"
        );
    }

    public void Indent() {
        level += 1;
    }

    public void Dedent() {
        level -= 1;
        if (level < 0) {
            // Should this throw?
            level = 0;
        }
    }

    public override string ToString() => result.ToString();
}
