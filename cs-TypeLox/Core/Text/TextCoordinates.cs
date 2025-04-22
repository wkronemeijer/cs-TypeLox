namespace Core;

/// <summary>
/// 1-indexed line and column pair. This is a class so the ">= 1" invariant can be maintained.
/// </summary>
public sealed class TextCoordinates {
    /// <summary>
    /// 1-based index of the line
    /// </summary>
    public int Ln { get; }
    /// <summary>
    /// 1-based index of the column.
    /// </summary>
    public int Col { get; }

    public TextCoordinates(int ln, int col) {
        Requires(1 <= ln, "");
        Requires(1 <= col, "");
        Ln = ln;
        Col = col;
    }

    public static TextCoordinates FromIndex(string source, int index) {
        var length = source.Length;
        Requires(0 <= index && index <= length, "index out of bounds");
        var ln = 1;
        var col = 1;
        // ("", 0) --> (1, 1)
        // ("a", 0) --> (1, 1)
        // ("a", 1) --> (1, 2)
        // ("ab", 0) --> (1, 1)
        // ("ab", 1) --> (1, 2)
        // ("ab", 2) --> (1, 3)
        var maxIterations = Math.Min(length, index);
        for (var i = 0; i < maxIterations; i++) {
            if (source[i] == '\n') {
                ln += 1;
                col = 1;
            } else {
                col += 1;
            }
        }
        return new(ln, col);
    }
}

public static class StringLocationExtensions {
    /// <summary>
    /// Finds the row and the column the given index points at.
    /// </summary>
    public static TextCoordinates GetLnCol(this string self, int index) {
        return TextCoordinates.FromIndex(self, index);
    }
}
