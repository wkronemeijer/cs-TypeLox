namespace TypeLox.Cli;

/// <summary>
/// Parses command-line arguments. 
/// Supports Powershell-style parameter features, like single dashes and case insensitivity.
/// </summary>
public sealed class ArgsParser(string[] args) {
    private readonly Dictionary<string, Action> actionByFlag = [];
    private readonly List<string> positionals = [];
    private readonly ProgramOptions options = new();

    // TODO: When error'ing, automatically append a "usage: ..." line
    private static Exception Error(string message) => new(message);

    private int current = 0;
    private string? Next() => current < args.Length ? args[current++] : null;
    private string Consume() => Next() is string s ? s : throw Error($"unexpected end of args");

    public static string NormalizeFlag(string self) =>
        self
        .ToLowerInvariant()
        .Replace("-", "")
        .Replace("_", "")
    ;

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

    private record class RegisterOptions() {
        public required string Name { get; init; }
        public required Action Action { get; init; }
        public string? HelpText { get; init; }
    }

    private void DefineFlag(RegisterOptions action) {
        actionByFlag[NormalizeFlag(action.Name)] = action.Action;
    }

    private void ProcessArgs() {
        while (Next() is string arg) {
            if (TryParseFlag(arg, out var flag)) {
                if (actionByFlag.TryGetValue(NormalizeFlag(flag), out var action)) {
                    action();
                } else {
                    throw Error($"unknown flag '{arg}'");
                }
            } else {
                positionals.Add(arg);
            }
        }
    }

    private ProgramMode Parse() {
        // TODO: Feels like case-specific code and generic code
        // Can we split the two? Would the codebase benefit in clarity?
        DefineFlag(new() {
            Name = "Backend",
            Action = delegate {
                options.Backend = Consume();
            },
            HelpText = "Select the compiler backend.",
        });

        DefineFlag(new() {
            Name = "PrintAll",
            Action = delegate {
                options.CompilerOptions.PrintTokens = true;
                options.CompilerOptions.PrintTree = true;
            },
            HelpText = "Print tokens and the tree.",
        });

        DefineFlag(new() {
            Name = "PrintTokens",
            Action = delegate {
                options.CompilerOptions.PrintTokens = true;
            },
            HelpText = "Print tokens after scanning.",
        });

        DefineFlag(new() {
            Name = "PrintTree",
            Action = delegate {
                options.CompilerOptions.PrintTree = true;
            },
            HelpText = "Print the tree after parsing.",
        });

        ProcessArgs();

        switch (positionals) {
            case []:
                return new ProgramMode.Repl();
            case ["run", string fileToRun]:
                return new ProgramMode.ExecuteFile(fileToRun.ToFileUri());
            case ["test", string dirToTest]:
                return new ProgramMode.TestDirectory(dirToTest.ToFileUri());
            case [string file]:
                return new ProgramMode.ExecuteFile(file.ToFileUri());
            default:
                throw Error($"unrecognized command");
        }
    }

    public static (ProgramMode, ProgramOptions) Parse(string[] args) {
        var parser = new ArgsParser(args);
        var mode = parser.Parse();
        return (mode, parser.options);
    }
}
