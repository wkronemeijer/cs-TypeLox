namespace Core;

using static AnsiCommand;

public sealed record class AnsiStyle(
    AnsiCommand Set,
    AnsiCommand Reset
) {
    public static readonly AnsiStyle Bold = new(SetBold, ResetWeight);
    public static readonly AnsiStyle Faint = new(SetFaint, ResetWeight);
    public static readonly AnsiStyle Italic = new(SetItalic, ResetItalic);
    public static readonly AnsiStyle Underline = new(SetUnderlined, ResetUnderlined);
    public static readonly AnsiStyle Inverted = new(SetInverted, ResetInverted);
    public static readonly AnsiStyle Strikethrough = new(SetStrikethrough, ResetStrikethrough);
    public static readonly AnsiStyle DoubleUnderline = new(SetDoubleUnderlined, ResetUnderlined);
    public static readonly AnsiStyle Overlined = new(SetOverlined, ResetOverlined);
    public static readonly AnsiStyle BlackLetter = new(SetBlackLetter, ResetLetter);
    public static readonly AnsiStyle RedLetter = new(SetRedLetter, ResetLetter);
    public static readonly AnsiStyle GreenLetter = new(SetGreenLetter, ResetLetter);
    public static readonly AnsiStyle YellowLetter = new(SetYellowLetter, ResetLetter);
    public static readonly AnsiStyle BlueLetter = new(SetBlueLetter, ResetLetter);
    public static readonly AnsiStyle MagentaLetter = new(SetMagentaLetter, ResetLetter);
    public static readonly AnsiStyle CyanLetter = new(SetCyanLetter, ResetLetter);
    public static readonly AnsiStyle WhiteLetter = new(SetWhiteLetter, ResetLetter);
    public static readonly AnsiStyle BrightBlackLetter = new(SetBrightBlackLetter, ResetLetter);
    public static readonly AnsiStyle BrightRedLetter = new(SetBrightRedLetter, ResetLetter);
    public static readonly AnsiStyle BrightGreenLetter = new(SetBrightGreenLetter, ResetLetter);
    public static readonly AnsiStyle BrightYellowLetter = new(SetBrightYellowLetter, ResetLetter);
    public static readonly AnsiStyle BrightBlueLetter = new(SetBrightBlueLetter, ResetLetter);
    public static readonly AnsiStyle BrightMagentaLetter = new(SetBrightMagentaLetter, ResetLetter);
    public static readonly AnsiStyle BrightCyanLetter = new(SetBrightCyanLetter, ResetLetter);
    public static readonly AnsiStyle BrightWhiteLetter = new(SetBrightWhiteLetter, ResetLetter);
    public static readonly AnsiStyle BlackBackground = new(SetBlackBackground, ResetBackground);
    public static readonly AnsiStyle RedBackground = new(SetRedBackground, ResetBackground);
    public static readonly AnsiStyle GreenBackground = new(SetGreenBackground, ResetBackground);
    public static readonly AnsiStyle YellowBackground = new(SetYellowBackground, ResetBackground);
    public static readonly AnsiStyle BlueBackground = new(SetBlueBackground, ResetBackground);
    public static readonly AnsiStyle MagentaBackground = new(SetMagentaBackground, ResetBackground);
    public static readonly AnsiStyle CyanBackground = new(SetCyanBackground, ResetBackground);
    public static readonly AnsiStyle WhiteBackground = new(SetWhiteBackground, ResetBackground);
    public static readonly AnsiStyle BrightBlackBackground = new(SetBrightBlackBackground, ResetBackground);
    public static readonly AnsiStyle BrightRedBackground = new(SetBrightRedBackground, ResetBackground);
    public static readonly AnsiStyle BrightGreenBackground = new(SetBrightGreenBackground, ResetBackground);
    public static readonly AnsiStyle BrightYellowBackground = new(SetBrightYellowBackground, ResetBackground);
    public static readonly AnsiStyle BrightBlueBackground = new(SetBrightBlueBackground, ResetBackground);
    public static readonly AnsiStyle BrightMagentaBackground = new(SetBrightMagentaBackground, ResetBackground);
    public static readonly AnsiStyle BrightCyanBackground = new(SetBrightCyanBackground, ResetBackground);
    public static readonly AnsiStyle BrightWhiteBackground = new(SetBrightWhiteBackground, ResetBackground);
}

public static class AnsiShortcuts {
    // TODO: Add powerline and some color?
    public static string Header(this string self) {
        var f = new Formatter();
        var (set, reset) = AnsiStyle.Bold;
        set.Format(f);
        f.Append(self);
        reset.Format(f);
        return f.ToString();
    }

    public static void Wrap(
        this IFormatter self,
        AnsiStyle pair,
        Action body
    ) {
        pair.Set.Format(self);
        body();
        pair.Reset.Format(self);
    }
}
