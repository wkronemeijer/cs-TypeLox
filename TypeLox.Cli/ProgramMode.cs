namespace TypeLox.Cli;

public abstract record class ProgramMode() {
    public interface IVisitor {
        void Visit(Repl value);
        void Visit(ExecuteFile value);
        void Visit(TestDirectory value);
    }

    public abstract void Accept(IVisitor visitor);

    /// <summary>
    /// Runs the program as an interactive prompt.
    /// </summary>
    public record class Repl() : ProgramMode() {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Reads the file, then executes it, then terminates.
    /// </summary>
    public record class ExecuteFile(Uri FileUri) : ProgramMode() {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Reads the file, then executes it, then terminates.
    /// </summary>
    public record class TestDirectory(Uri? DirectoryUri) : ProgramMode() {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    ////////////
    // Parser //
    ////////////

    private sealed class Parser(string[] args) {
        public ProgramOptions options = new();

        private int current = 0;
        private string? Next() => current < args.Length ? args[current++] : null;

        private static readonly string[] prefixes = ["-", "--"];
        private static bool TryParseFlag(string arg, out string value) {
            foreach (var prefix in prefixes) {
                if (arg.StartsWith(prefix)) {
                    value = arg[prefix.Length..];
                    return true;
                }
            }
            value = string.Empty;
            return false;
        }

        public ProgramMode Parse() {
            var positionals = new List<string>();

            while (Next() is string arg) {
                if (TryParseFlag(arg, out var rawFlag)) {
                    var flag = rawFlag.ToLowerInvariant().Replace("-", "");
                    // Do you hear it?
                    // The siren call of reflection?
                    if (flag is "printall") {
                        options.CompilerOptions.PrintTokens = true;
                        options.CompilerOptions.PrintTree = true;
                    } else if (flag is "printtokens") {
                        options.CompilerOptions.PrintTokens = true;
                    } else if (flag is "printtree") {
                        options.CompilerOptions.PrintTree = true;
                    } else {
                        throw new Exception($"unknown flag {rawFlag}");
                    }
                } else {
                    positionals.Add(arg);
                }
            }

            // New/ideal argument parsing
            if (positionals is ["run", string runFile]) {
                return new ExecuteFile(runFile.ToFileUri());
            } else if (positionals is ["test", string testDir]) {
                return new TestDirectory(testDir.ToFileUri());
            }

            // Legacy arguments 
            if (positionals is []) {
                return new Repl();
            } else if (positionals is [string file]) {
                return new ExecuteFile(file.ToFileUri());
            }

            // No pattern found
            throw new Exception($"unrecognized command");
        }
    }

    public static (ProgramMode, ProgramOptions) Parse(string[] args) {
        var parser = new Parser(args);
        var mode = parser.Parse();
        return (mode, parser.options);
    }
}
