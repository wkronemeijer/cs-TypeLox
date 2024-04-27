namespace Core;

using static AnsiCommand;

// Ansi coloring!
// Only basic color sequences.
// <command>                  --> <set> / <reset>
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
// ¹ = not <set>+20 for some reason

public enum AnsiCommand {
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

public static class AnsiCommandMethods {
    public static void Format(this AnsiCommand self, IFormatter f) {
        f.Append('\x1B');
        f.Append('[');
        f.Append((int)self);
        f.Append('m');
    }
}
