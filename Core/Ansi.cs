namespace Core;

using static AnsiSgr;
// Ansi coloring!
// Only basic color sequences.
// Command                    --> enable / undo
// Reset All                  --> 0
// Bold                       --> 1 / 22¹
// Faint                      --> 2 / 22¹
// Italic                     --> 3 / 23
// Underline                  --> 4 / 24
// Inverted ("reverse")       --> 7 / 27
// Strikethrough              --> 9 / 29
// Double Underline           --> 21 / 24
// Overlined                  --> 53 / 55
// Foreground color (3 bits)  --> 30-37 / 39
// Background color (3 bits)  --> 40-47 / 49
// Foreground color (8 bits)  --> 38;5;n / 39²
// Background color (8 bits)  --> 48;5;n / 49²
// Foreground color (24 bits) --> 38;2;r;g;b / 39
// Background color (24 bits) --> 48;2;r;g;b / 39
// 1 = not (c + 20) for some reason

public enum AnsiSgr {
    ResetAll = 0,

    // Simple formatting
    SetBold = 1,
    SetFaint = 2,
    SetItalic = 3,
    SetUnderlined = 4,
    SetInverted = 7,
    SetStrikethrough = 9,
    SetDoubleUnderlined = 21,
    SetOverlined = 53,

    // Colors
    SetBlackLetter = 30,
    SetRedLetter = 31,
    SetGreenLetter = 32,
    SetYellowLetter = 33,
    SetBlueLetter = 34,
    SetMagentaLetter = 35,
    SetCyanLetter = 36,
    SetWhiteLetter = 37,

    SetBrightBlackLetter = 90,
    SetBrightRedLetter = 91,
    SetBrightGreenLetter = 92,
    SetBrightYellowLetter = 93,
    SetBrightBlueLetter = 94,
    SetBrightMagentaLetter = 95,
    SetBrightCyanLetter = 96,
    SetBrightWhiteLetter = 97,

    SetBlackBackground = 40,
    SetRedBackground = 41,
    SetGreenBackground = 42,
    SetYellowBackground = 43,
    SetBlueBackground = 44,
    SetMagentaBackground = 45,
    SetCyanBackground = 46,
    SetWhiteBackground = 47,

    SetBrightBlackBackground = 100,
    SetBrightRedBackground = 101,
    SetBrightGreenBackground = 102,
    SetBrightYellowBackground = 103,
    SetBrightBlueBackground = 104,
    SetBrightMagentaBackground = 105,
    SetBrightCyanBackground = 106,
    SetBrightWhiteBackground = 107,

    // Resets
    ResetWeight = 22,
    ResetItalic = 23,
    ResetUnderlined = 24,
    ResetInverted = 27,
    ResetStrikethrough = 29,
    ResetLetter = 39,
    ResetBackground = 49,
    ResetOverlined = 55,
}

public static class AnsiSgrMethods {
    public static void Format(this AnsiSgr self, StringBuilder builder) {
        builder.Append('\x1B');
        builder.Append('[');
        builder.Append((int)self);
        builder.Append('m');
    }
}

public sealed record class AnsiSgrPair(
    AnsiSgr Set,
    AnsiSgr Reset
) {
    public static readonly AnsiSgrPair Bold = new(SetBold, ResetWeight);
    public static readonly AnsiSgrPair Faint = new(SetFaint, ResetWeight);
    public static readonly AnsiSgrPair Italic = new(SetItalic, ResetItalic);
    public static readonly AnsiSgrPair Underline = new(SetUnderlined, ResetUnderlined);
    public static readonly AnsiSgrPair Inverted = new(SetInverted, ResetInverted);
    public static readonly AnsiSgrPair Strikethrough = new(SetStrikethrough, ResetStrikethrough);
    public static readonly AnsiSgrPair DoubleUnderline = new(SetDoubleUnderlined, ResetUnderlined);
    public static readonly AnsiSgrPair Overlined = new(SetOverlined, ResetOverlined);
    public static readonly AnsiSgrPair BlackLetter = new(SetBlackLetter, ResetLetter);
    public static readonly AnsiSgrPair RedLetter = new(SetRedLetter, ResetLetter);
    public static readonly AnsiSgrPair GreenLetter = new(SetGreenLetter, ResetLetter);
    public static readonly AnsiSgrPair YellowLetter = new(SetYellowLetter, ResetLetter);
    public static readonly AnsiSgrPair BlueLetter = new(SetBlueLetter, ResetLetter);
    public static readonly AnsiSgrPair MagentaLetter = new(SetMagentaLetter, ResetLetter);
    public static readonly AnsiSgrPair CyanLetter = new(SetCyanLetter, ResetLetter);
    public static readonly AnsiSgrPair WhiteLetter = new(SetWhiteLetter, ResetLetter);
    public static readonly AnsiSgrPair BrightBlackLetter = new(SetBrightBlackLetter, ResetLetter);
    public static readonly AnsiSgrPair BrightRedLetter = new(SetBrightRedLetter, ResetLetter);
    public static readonly AnsiSgrPair BrightGreenLetter = new(SetBrightGreenLetter, ResetLetter);
    public static readonly AnsiSgrPair BrightYellowLetter = new(SetBrightYellowLetter, ResetLetter);
    public static readonly AnsiSgrPair BrightBlueLetter = new(SetBrightBlueLetter, ResetLetter);
    public static readonly AnsiSgrPair BrightMagentaLetter = new(SetBrightMagentaLetter, ResetLetter);
    public static readonly AnsiSgrPair BrightCyanLetter = new(SetBrightCyanLetter, ResetLetter);
    public static readonly AnsiSgrPair BrightWhiteLetter = new(SetBrightWhiteLetter, ResetLetter);
    public static readonly AnsiSgrPair BlackBackground = new(SetBlackBackground, ResetBackground);
    public static readonly AnsiSgrPair RedBackground = new(SetRedBackground, ResetBackground);
    public static readonly AnsiSgrPair GreenBackground = new(SetGreenBackground, ResetBackground);
    public static readonly AnsiSgrPair YellowBackground = new(SetYellowBackground, ResetBackground);
    public static readonly AnsiSgrPair BlueBackground = new(SetBlueBackground, ResetBackground);
    public static readonly AnsiSgrPair MagentaBackground = new(SetMagentaBackground, ResetBackground);
    public static readonly AnsiSgrPair CyanBackground = new(SetCyanBackground, ResetBackground);
    public static readonly AnsiSgrPair WhiteBackground = new(SetWhiteBackground, ResetBackground);
    public static readonly AnsiSgrPair BrightBlackBackground = new(SetBrightBlackBackground, ResetBackground);
    public static readonly AnsiSgrPair BrightRedBackground = new(SetBrightRedBackground, ResetBackground);
    public static readonly AnsiSgrPair BrightGreenBackground = new(SetBrightGreenBackground, ResetBackground);
    public static readonly AnsiSgrPair BrightYellowBackground = new(SetBrightYellowBackground, ResetBackground);
    public static readonly AnsiSgrPair BrightBlueBackground = new(SetBrightBlueBackground, ResetBackground);
    public static readonly AnsiSgrPair BrightMagentaBackground = new(SetBrightMagentaBackground, ResetBackground);
    public static readonly AnsiSgrPair BrightCyanBackground = new(SetBrightCyanBackground, ResetBackground);
    public static readonly AnsiSgrPair BrightWhiteBackground = new(SetBrightWhiteBackground, ResetBackground);
}

public static class AnsiShortcuts {
    private static string Wrap(this string self, AnsiSgrPair pair) {
        var result = new StringBuilder();
        var (start, end) = pair;
        start.Format(result);
        result.Append(self);
        end.Format(result);
        return result.ToString();
    }

    public static string Bold(this string self) => self.Wrap(AnsiSgrPair.Bold);
}
