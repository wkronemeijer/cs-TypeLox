namespace TypeLox;

public class LoxException : Exception {
    public LoxException() { }
    public LoxException(string? message) : base(message) { }
    public LoxException(string? message, Exception? innerException) : base(message, innerException) { }
}

// Annoying: (R)un(t)ime -vs- (C)ompile(T)ime
// Maybe use comptime?
public class LoxCompileException : LoxException {
    public LoxCompileException() { }
    public LoxCompileException(string? message) : base(message) { }
    public LoxCompileException(string? message, Exception? innerException) : base(message, innerException) { }
}

public class LoxRuntimeException : LoxException {
    public LoxRuntimeException() { }
    public LoxRuntimeException(string? message) : base(message) { }
    public LoxRuntimeException(string? message, Exception? innerException) : base(message, innerException) { }
}
