using TypeLox;

internal abstract record class ProgramMode(ProgramOptions Options) {
    public interface IVisitor {
        void Visit(Repl value);
        void Visit(ExecuteFile value);
    }

    public abstract void Accept(IVisitor visitor);

    /// <summary>
    /// Runs the program as an interactive prompt.
    /// </summary>
    public record class Repl(ProgramOptions Options) : ProgramMode(Options) {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    /// <summary>
    /// Reads the file, then executes it, then terminates.
    /// </summary>
    public record class ExecuteFile(Uri Uri, ProgramOptions Options) : ProgramMode(Options) {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    private class Parser(string[] args) {
        public ProgramOptions Options { get; } = new();

        private int current = 0;
        string? Next() => current < args.Length ? args[current++] : null;

        private static string[] prefixes = ["-", "--"];
        static bool TryParseFlag(string arg, out string value) {
            foreach (var prefix in prefixes) {
                if (arg.StartsWith(prefix)) {
                    value = arg[prefix.Length..];
                    return true;
                }
            }
            value = "";
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
                        Options.PrintTokens = true;
                        Options.PrintTree = true;
                    } else if (flag is "printtokens") {
                        Options.PrintTokens = true;
                    } else if (flag is "printtree") {
                        Options.PrintTree = true;
                    } else {
                        throw new Exception($"unknown flag {rawFlag}");
                    }
                } else {
                    positionals.Add(arg);
                }
            }

            if (positionals is []) {
                return new Repl(Options);
            } else if (positionals is [string file]) {
                return new ExecuteFile(file.ToFileUri(), Options);
            } else {
                throw new Exception($"incorrect number of arguments: {args.Length} (should be 0 or 1)");
            }
        }
    }

    public static (ProgramMode, ProgramOptions) Parse(string[] args) {
        var parser = new Parser(args);

        var mode = parser.Parse();
        return (mode, parser.Options);
    }

}
