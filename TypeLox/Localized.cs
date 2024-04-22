namespace TypeLox;

// TODO: Maybe include location information using a generic wrapper type?
// Some kind of Deref<Target = T> like thing
// Adds location to T
// Implicitly converts to T
// TODO: Should also track comments for formatting
public record class Localized<T>(SourceRange? Range, T Value) {
    // A. This works
    // B. This is disgusting
    // C. Interesting how it uses the T type declared in the class header
    public static implicit operator T(Localized<T> localized) {
        return localized.Value;
    }
}
// Another note: 
// Tokens are split between common data and unique data
// Maybe similar split for exprs too 
// ExprKind?
