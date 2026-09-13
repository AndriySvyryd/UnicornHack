using System.Collections.Concurrent;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnicornHack.DiagnosticClient.State;
using UnicornHack.Hubs;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Actors;

namespace UnicornHack.DiagnosticClient.Connection;

/// <summary>
///     Manages the SignalR connection to <c>UnicornHack.Web</c>'s <c>GameHub</c>.
///     Mirrors the configuration used by the Unity client
///     (<c>Assets/Scripts/NetworkCommunicationManager.cs</c>) so the wire format matches.
///     Uses server DTOs and sparse formatters that implement the same wire contract as the production client.
///     The MCP server stays alive while the game server is stopped or restarted:
///     SignalR auto-reconnects forever, and <see cref="EnsureReadyAsync" /> lazily
///     (re)connects + re-runs <c>GetState</c> on the next tool invocation.
/// </summary>
public sealed class GameConnection : IAsyncDisposable
{
    private readonly ILogger<GameConnection> _logger;
    private readonly GameStateStore _state;
    private readonly TurnTracker _turnTracker;
    private readonly DiagnosticOptions _options;
    private readonly ConcurrentQueue<(string user, string message)> _chat = new();
    private readonly SemaphoreSlim _readyLock = new(1, 1);
    /// <summary>Serializes hub method calls so a burst of MCP tool calls doesn't pile up on the per-player game.</summary>
    private readonly SemaphoreSlim _callLock = new(1, 1);
    private PendingDialogResponse? _pendingDialogResponse;
    private HubConnection? _hub;
    private bool _stateReady;
    private bool _disposed;

    public GameConnection(
        DiagnosticOptions options,
        GameStateStore state,
        TurnTracker turnTracker,
        ILogger<GameConnection> logger)
    {
        _options = options;
        _state = state;
        _turnTracker = turnTracker;
        _logger = logger;
    }

    public bool IsConnected => _hub?.State == HubConnectionState.Connected;

    public string ConnectionState => _hub?.State.ToString() ?? "NotBuilt";

    public bool StateReady => _stateReady;

    public DialogData? LastUIRequest { get; private set; }

    public IReadOnlyCollection<(string user, string message)> Chat => _chat.ToArray();

    /// <summary>
    ///     Lazily builds the hub if it has never been built, then either starts it (if
    ///     disconnected) or waits briefly for an in-flight reconnect, then issues
    ///     <c>GetState</c> if the cached state isn't current. Safe to call from many
    ///     tools concurrently; serializes via an internal lock.
    /// </summary>
    public async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(GameConnection));
        }
        if (_hub is { State: HubConnectionState.Connected } && _stateReady)
        {
            return;
        }

        await _readyLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_hub is { State: HubConnectionState.Connected } && _stateReady)
            {
                return;
            }

            BuildHub();

            switch (_hub!.State)
            {
                case HubConnectionState.Connected:
                    break;
                case HubConnectionState.Disconnected:
                    _logger.LogInformation("Hub disconnected; (re)starting connection");
                    _stateReady = false;
                    await _hub.StartAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case HubConnectionState.Connecting:
                case HubConnectionState.Reconnecting:
                    _logger.LogInformation("Hub state is {State}; waiting for connect", _hub.State);
                    await WaitForConnectedAsync(cancellationToken).ConfigureAwait(false);
                    break;
            }

            if (!_stateReady)
            {
                _logger.LogInformation("Fetching initial state for {Name}", _options.PlayerName);
                var snapshot = await _hub.InvokeAsync<PlayerChange>(
                    "GetState", _options.PlayerName, cancellationToken)
                    .ConfigureAwait(false);
                _state.ApplyInitialSnapshot(snapshot);
                _stateReady = true;
            }
        }
        finally
        {
            _readyLock.Release();
        }
    }

    public async Task PerformActionAsync(
        ActorActionType action, int? target, int? target2, CancellationToken cancellationToken)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        await _callLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _turnTracker.ArmForNextChangeSets();
            await _hub!.InvokeAsync<string?>(
                "PerformAction", _options.PlayerName, (int)action, target, target2, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _callLock.Release();
        }
    }

    public async Task<DialogData?> QueryGameAsync(
        int queryType, int[] arguments, CancellationToken cancellationToken)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        await _callLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var payload = await _hub!.InvokeAsync<DialogData>(
                "QueryGame", _options.PlayerName, queryType, arguments, cancellationToken)
                .ConfigureAwait(false);
            LastUIRequest = (GameQueryType)queryType == GameQueryType.Clear ? null : payload;
            return payload;
        }
        finally
        {
            _callLock.Release();
        }
    }

    public async Task<DialogData?> ShowDialogAsync(
        int queryType, int[] arguments, CancellationToken cancellationToken)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        await _callLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        var response = new PendingDialogResponse(
            queryType,
            new TaskCompletionSource<DialogData>(
                TaskCreationOptions.RunContinuationsAsynchronously));
        Interlocked.Exchange(ref _pendingDialogResponse, response);
        try
        {
            await _hub!.InvokeAsync<string?>(
                "ShowDialog", _options.PlayerName, queryType, arguments, cancellationToken)
                .ConfigureAwait(false);
            return await response.Completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.CompareExchange(ref _pendingDialogResponse, null, response);
            _callLock.Release();
        }
    }

    public async Task<string> QueryStaticDescriptionAsync(
        string topicId, int category, CancellationToken cancellationToken)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        return await _hub!.InvokeAsync<string>(
            "QueryStaticDescription", topicId, category, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        // NOTE: server's SendMessage dereferences Context.User.Identity.Name. The Web server
        // currently has no authentication wired up, so this will throw NRE on the server.
        // Exposed for completeness; failures surface to the caller.
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        await _hub!.InvokeAsync<string?>("SendMessage", message, cancellationToken).ConfigureAwait(false);
    }

    private void BuildHub()
    {
        if (_hub is not null)
        {
            return;
        }

        var hubUrl = _options.ServerUrl.TrimEnd('/') + "/gameHub";
        _logger.LogInformation("Building hub connection to {Url} as {Name}", hubUrl, _options.PlayerName);

        var resolver = CompositeResolver.Create(
            GeneratedResolver.Instance,
            BuiltinResolver.Instance);

        var serializerOptions = MessagePackSerializerOptions.Standard
            .WithSecurity(MessagePackSecurity.UntrustedData)
            .WithResolver(resolver);

        _hub = new HubConnectionBuilder()
            .WithUrl(hubUrl, HttpTransportType.WebSockets, options =>
            {
                options.SkipNegotiation = true;
                if (_options.Insecure)
                {
                    options.HttpMessageHandlerFactory = inner =>
                    {
                        if (inner is HttpClientHandler h)
                        {
                            h.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                        }
                        return inner;
                    };
                    options.WebSocketConfiguration = ws =>
                    {
                        ws.RemoteCertificateValidationCallback = (_, _, _, _) => true;
                    };
                }
            })
            .WithAutomaticReconnect(new ForeverRetryPolicy())
            .AddMessagePackProtocol(o => o.SerializerOptions = serializerOptions)
            .ConfigureLogging(b => b.AddProvider(new PassthroughLoggerProvider(_logger)))
            .Build();

        RegisterCallbacks(_hub);
    }

    private async Task WaitForConnectedAsync(CancellationToken cancellationToken)
    {
        // Bounded polling so a stuck reconnect loop doesn't hang a tool call indefinitely.
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (_hub!.State)
            {
                case HubConnectionState.Connected:
                    return;
                case HubConnectionState.Disconnected:
                    await _hub.StartAsync(cancellationToken).ConfigureAwait(false);
                    return;
            }
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
        }
        if (_hub!.State == HubConnectionState.Disconnected)
        {
            await _hub.StartAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private void RegisterCallbacks(HubConnection hub)
    {
        hub.On<PlayerChange>("ReceiveState", snapshot =>
        {
            try
            {
                _state.ApplyInitialSnapshot(snapshot);
                _stateReady = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply ReceiveState");
            }
        });

        hub.On<List<TurnChangeSet>>("ReceiveChangeSets", sets =>
        {
            try
            {
                _state.ApplyChangeSets(sets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply ReceiveChangeSets");
            }
            finally
            {
                _turnTracker.SignalChangeSetsReceived();
            }
        });

        hub.On<DialogData>("ReceiveUIRequest", payload =>
        {
            LastUIRequest = payload;
            if (Volatile.Read(ref _pendingDialogResponse) is { } pending
                && IsExpectedDialogResponse(payload, pending.QueryType))
            {
                pending.Completion.TrySetResult(payload);
            }
            _logger.LogInformation("Received UI request");
        });

        hub.On<string, string>("ReceiveMessage", (user, message) =>
        {
            _chat.Enqueue((user, message));
            while (_chat.Count > 200 && _chat.TryDequeue(out _)) { }
        });

        hub.Closed += ex =>
        {
            // Auto-reconnect retries forever, so Closed indicates we've stopped retrying
            // (typically: user shutdown or fatal start failure). The next tool call will
            // trigger EnsureReadyAsync → StartAsync.
            _logger.LogWarning(ex, "Hub closed. Will lazily reconnect on next tool call.");
            _stateReady = false;
            return Task.CompletedTask;
        };

        hub.Reconnecting += ex =>
        {
            _logger.LogInformation(ex, "Hub reconnecting…");
            return Task.CompletedTask;
        };

        hub.Reconnected += id =>
        {
            // The server may have restarted between disconnect and reconnect; force a fresh
            // GetState before the next action.
            _logger.LogInformation("Hub reconnected (id={Id}); marking state stale to refetch.", id);
            _stateReady = false;
            return Task.CompletedTask;
        };
    }

    private static bool IsExpectedDialogResponse(DialogData payload, int queryType)
    {
        if (queryType == (int)GameQueryType.Clear)
        {
            return payload.ChangedProperties == 0;
        }

        return payload.ChangedProperties == null
               || (payload.ChangedProperties.GetValueOrDefault() & (1 << (queryType + 1))) != 0;
    }

    private sealed record PendingDialogResponse(
        int QueryType, TaskCompletionSource<DialogData> Completion);

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        if (_hub is not null)
        {
            await _hub.DisposeAsync().ConfigureAwait(false);
            _hub = null;
        }
        _readyLock.Dispose();
        _callLock.Dispose();
    }

    private sealed class PassthroughLoggerProvider(ILogger target) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => target;
        public void Dispose() { }
    }

    /// <summary>Retries forever with capped backoff so the MCP server stays alive across long server outages.</summary>
    private sealed class ForeverRetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
            => retryContext.PreviousRetryCount < 4
                ? TimeSpan.FromSeconds(Math.Min(5, 1 << (int)retryContext.PreviousRetryCount))
                : TimeSpan.FromSeconds(5);
    }
}
