using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnicornHack.DiagnosticClient;
using UnicornHack.DiagnosticClient.Connection;
using UnicornHack.DiagnosticClient.State;

DiagnosticOptions options;
try
{
    options = DiagnosticOptions.Parse(args);
}
catch (HelpRequestedException)
{
    Console.Error.WriteLine(DiagnosticOptions.HelpText);
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine(DiagnosticOptions.HelpText);
    return 2;
}

var builder = Host.CreateApplicationBuilder();

// stdio is reserved for MCP JSON-RPC. All logging must go to stderr/file.
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(options.Verbose ? LogLevel.Debug : LogLevel.Information);
if (!string.IsNullOrEmpty(options.LogFile))
{
    builder.Logging.AddProvider(new FileLoggerProvider(options.LogFile));
}
else
{
    // Stderr-only logging so MCP stdout stream stays clean.
    builder.Logging.AddProvider(new StderrLoggerProvider());
}

builder.Services.AddSingleton(options);
builder.Services.AddSingleton<GameStateStore>();
builder.Services.AddSingleton<TurnTracker>();
builder.Services.AddSingleton<GameConnection>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHostedService<ConnectionBootstrap>();

var host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
return 0;

namespace UnicornHack.DiagnosticClient
{
    internal sealed class ConnectionBootstrap : IHostedService
    {
        private readonly GameConnection _conn;
        private readonly DiagnosticOptions _options;
        private readonly ILogger<ConnectionBootstrap> _logger;

        public ConnectionBootstrap(
            GameConnection conn, DiagnosticOptions options, ILogger<ConnectionBootstrap> logger)
        {
            _conn = conn;
            _options = options;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Run on a background task so a slow/unreachable game server can't block the
            // MCP stdio server from coming online. Tools will report not-connected until
            // the bootstrap finishes.
            _ = Task.Run(async () =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(_options.ActionTimeoutMs);
                    await _conn.EnsureReadyAsync(cts.Token).ConfigureAwait(false);
                    _logger.LogInformation("Initial GetState complete; MCP tools are ready.");
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "Bootstrap timed out after {TimeoutMs}ms. MCP server stays up; tools will retry on demand.",
                        _options.ActionTimeoutMs);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Bootstrap failed. MCP server stays up; tools will retry on demand.");
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => _conn.DisposeAsync().AsTask();
    }

    internal sealed class StderrLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new StderrLogger(categoryName);
        public void Dispose() { }

        private sealed class StderrLogger : ILogger
        {
            private readonly string _category;
            public StderrLogger(string category) => _category = category;
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;
                Console.Error.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {logLevel,-5} {_category}: {formatter(state, exception)}");
                if (exception != null) Console.Error.WriteLine(exception);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    internal sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly object _gate = new();
        private readonly string _path;

        public FileLoggerProvider(string path)
        {
            _path = path;
            // Touch the file so failures surface early.
            File.AppendAllText(_path, $"--- session start {DateTime.UtcNow:O} ---{Environment.NewLine}");
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, this);
        public void Dispose() { }

        private void Write(string line)
        {
            lock (_gate)
            {
                File.AppendAllText(_path, line + Environment.NewLine);
            }
        }

        private sealed class FileLogger : ILogger
        {
            private readonly string _category;
            private readonly FileLoggerProvider _provider;

            public FileLogger(string category, FileLoggerProvider provider)
            {
                _category = category;
                _provider = provider;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var msg = $"[{DateTime.UtcNow:O}] {logLevel,-5} {_category}: {formatter(state, exception)}";
                if (exception != null) msg += Environment.NewLine + exception;
                _provider.Write(msg);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
