namespace UnicornHack.DiagnosticClient;

public sealed class DiagnosticOptions
{
    public string ServerUrl { get; init; } = "https://localhost:7251";
    public string PlayerName { get; init; } = "diag-bot";
    public bool Insecure { get; init; }
    public string? LogFile { get; init; }
    public bool Verbose { get; init; }

    /// <summary>
    ///     Server-side per-action processing cap. Caps the <c>PerformAction</c> hub invoke
    ///     round-trip. Heavy actions (level transitions, big AI rounds) can take several
    ///     seconds, so the default is generous.
    /// </summary>
    public int ActionTimeoutMs { get; init; } = 30_000;

    /// <summary>
    ///     Post-invoke wait for the <c>ReceiveChangeSets</c> push. The server sends the
    ///     push <em>before</em> returning from <c>PerformAction</c>, so this should be
    ///     near-instant in practice. Treated as a <em>soft</em> deadline: if it expires
    ///     the action is still considered to have completed and the next tool call will
    ///     pick up the catch-up state.
    /// </summary>
    public int WaitTimeoutMs { get; init; } = 5_000;

    public static DiagnosticOptions Parse(string[] args)
    {
        string? server = null;
        string? name = null;
        bool insecure = false;
        string? logFile = null;
        bool verbose = false;
        int actionTimeout = 30_000;
        int waitTimeout = 5_000;

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];
            switch (a)
            {
                case "--server" or "-s":
                    server = RequireNext(args, ref i, a);
                    break;
                case "--name" or "-n":
                    name = RequireNext(args, ref i, a);
                    break;
                case "--insecure":
                    insecure = true;
                    break;
                case "--log-file" or "-l":
                    logFile = RequireNext(args, ref i, a);
                    break;
                case "--verbose" or "-v":
                    verbose = true;
                    break;
                case "--action-timeout-ms":
                    actionTimeout = int.Parse(RequireNext(args, ref i, a));
                    break;
                case "--wait-timeout-ms":
                    waitTimeout = int.Parse(RequireNext(args, ref i, a));
                    break;
                case "--help" or "-h" or "/?":
                    throw new HelpRequestedException();
                default:
                    throw new ArgumentException($"Unknown argument: {a}");
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("--name <playerName> is required");
        }

        return new DiagnosticOptions
        {
            ServerUrl = server ?? "https://localhost:7251",
            PlayerName = name!,
            Insecure = insecure,
            LogFile = logFile,
            Verbose = verbose,
            ActionTimeoutMs = actionTimeout,
            WaitTimeoutMs = waitTimeout
        };
    }

    private static string RequireNext(string[] args, ref int i, string flag)
    {
        if (i + 1 >= args.Length)
        {
            throw new ArgumentException($"{flag} requires a value");
        }
        return args[++i];
    }

    public static string HelpText =>
        """
        UnicornHack Diagnostic Client — MCP server over stdio

        Usage: UnicornHack.DiagnosticClient --name <playerName> [options]

        Options:
          --name, -n <name>            Player/character name (required).
          --server, -s <url>           Server base URL (default https://localhost:7251).
          --insecure                   Accept any TLS certificate (dev only).
          --log-file, -l <path>        Append logs to file (stdio is reserved for MCP).
          --verbose, -v                Verbose logging.
          --action-timeout-ms <ms>     Server invoke (hub round-trip) cap (default 30000).
          --wait-timeout-ms <ms>       Post-invoke wait for ReceiveChangeSets, treated as
                                       a soft deadline (default 5000).
          --help, -h                   Show this help.
        """;
}

public sealed class HelpRequestedException : Exception;
