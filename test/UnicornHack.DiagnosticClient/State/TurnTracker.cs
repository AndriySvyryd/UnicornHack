namespace UnicornHack.DiagnosticClient.State;

/// <summary>
///     Coordinates an MCP action tool's "wait for the post-action change sets" semantics.
///     Each action arms the tracker before invoking the hub, then awaits the next
///     <c>ReceiveChangeSets</c> message (or a timeout).
/// </summary>
public sealed class TurnTracker
{
    private readonly Lock _gate = new();
    private TaskCompletionSource? _pending;

    public void ArmForNextChangeSets()
    {
        lock (_gate)
        {
            _pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public void SignalChangeSetsReceived()
    {
        TaskCompletionSource? tcs;
        lock (_gate)
        {
            tcs = _pending;
            _pending = null;
        }
        tcs?.TrySetResult();
    }

    public async Task WaitAsync(int timeoutMs, CancellationToken cancellationToken)
    {
        Task? task;
        lock (_gate)
        {
            task = _pending?.Task;
        }
        if (task is null)
        {
            return;
        }
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeoutMs);
        try
        {
            await task.WaitAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Treat timeout as soft: state may still be the latest we have.
        }
    }
}
