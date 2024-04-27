namespace Core;

public static class TextModel {
    public const char UpArrow = '\u2191';
    public const char LeftUpArrow = '\u2196';
    public const char VerticalBar = '\u2502';

    private static (int Start, int End) GetIndexLineLimits(string source, int index) {
        var length = source.Length;
        var start = 0;
        var end = length;

        for (var i = 0; i < length; i++) {
            if (i == index) {
                for (var j = i; j < length; j++) {
                    if (source[j] is '\r' or '\n') {
                        end = j;
                        goto exit;
                    }
                }
                goto exit;
            }

            if (source[i] == '\n' && i + 1 < length) {
                start = i + 1;
            }
        }

    exit:
        Ensures(0 <= start && start <= end && end <= length);
        return (start, end);
    }

    public static void IncludePreview(this IFormatter f, string source, int start, int end) {
        var lineNumber = source.GetLnCol(start).Ln.ToString();
        var (lineStart, lineEnd) = GetIndexLineLimits(source, start);

        var contextStart = lineStart;
        var errorStart = start;
        var errorEnd = Math.Min(end, lineEnd); // only preview 1 line, even if the error stretches multiple
        var contextEnd = lineEnd;

        var pre = source[contextStart..errorStart];
        var at = source[errorStart..errorEnd];
        var post = source[errorEnd..contextEnd];

        var border = " ┃ ";

        // Line 1
        f.AppendLine(); // pre-emptive newline to ensure this lines up
        f.Append(lineNumber);
        f.Append(border);
        f.Append(pre);
        f.Append(at);
        f.Append(post);

        // Line 2
        f.AppendLine();
        f.Append(' '.Repeat(lineNumber.Length));
        f.Append(border);
        f.Append(' '.Repeat(pre.Length));

        var errLength = at.Length;
        if (errLength > 0) {
            f.Append(UpArrow.Repeat(errLength));
        } else {
            f.Append(LeftUpArrow);
        }
        f.Append(' ');
    }
}
