namespace TypeLox.Backend.Treewalker;

public class LoxRuntimeException(SourceRange location, string message) : LoxException(location, message);
