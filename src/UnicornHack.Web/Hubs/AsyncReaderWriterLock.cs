namespace UnicornHack.Hubs;

public sealed class AsyncReaderWriterLock
{
    private readonly object _gate = new();
    private readonly Queue<TaskCompletionSource<Lease>> _waitingReaders = new();
    private readonly Queue<TaskCompletionSource<Lease>> _waitingWriters = new();
    private int _activeReaders;
    private bool _writerActive;

    public ValueTask<Lease> AcquireReadAsync()
    {
        lock (_gate)
        {
            if (!_writerActive && _waitingWriters.Count == 0)
            {
                _activeReaders++;
                return ValueTask.FromResult(new Lease(this, writer: false));
            }

            var completion = new TaskCompletionSource<Lease>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waitingReaders.Enqueue(completion);
            return new ValueTask<Lease>(completion.Task);
        }
    }

    public ValueTask<Lease> AcquireWriteAsync()
    {
        lock (_gate)
        {
            if (!_writerActive && _activeReaders == 0)
            {
                _writerActive = true;
                return ValueTask.FromResult(new Lease(this, writer: true));
            }

            var completion = new TaskCompletionSource<Lease>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waitingWriters.Enqueue(completion);
            return new ValueTask<Lease>(completion.Task);
        }
    }

    private void Release(bool writer)
    {
        TaskCompletionSource<Lease>? nextWriter = null;
        List<TaskCompletionSource<Lease>>? nextReaders = null;
        lock (_gate)
        {
            if (writer)
            {
                _writerActive = false;
            }
            else
            {
                _activeReaders--;
            }

            if (_writerActive || _activeReaders != 0)
            {
                return;
            }

            if (_waitingWriters.TryDequeue(out nextWriter))
            {
                _writerActive = true;
            }
            else if (_waitingReaders.Count != 0)
            {
                nextReaders = new List<TaskCompletionSource<Lease>>(_waitingReaders.Count);
                while (_waitingReaders.TryDequeue(out var reader))
                {
                    nextReaders.Add(reader);
                }

                _activeReaders = nextReaders.Count;
            }
        }

        nextWriter?.SetResult(new Lease(this, writer: true));
        if (nextReaders != null)
        {
            foreach (var reader in nextReaders)
            {
                reader.SetResult(new Lease(this, writer: false));
            }
        }
    }

    public sealed class Lease : IDisposable
    {
        private AsyncReaderWriterLock? _owner;
        private readonly bool _writer;

        internal Lease(AsyncReaderWriterLock owner, bool writer)
        {
            _owner = owner;
            _writer = writer;
        }

        public void Dispose()
            => Interlocked.Exchange(ref _owner, null)?.Release(_writer);
    }
}
