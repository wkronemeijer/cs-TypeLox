namespace Core;

public interface IBuildable {
    void Format(StringBuilder builder);
}

public static class TextExtensions {
    public static void Include(this StringBuilder self, IBuildable buildable) {
        buildable.Format(self);
    }

    public static void Wrap(this StringBuilder self, AnsiSgrPair pair, Action body) {
        pair.Set.Format(self);
        body();
        pair.Reset.Format(self);
    }
}
