namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     A message queue that processes messages in the order they were enqueued
/// </summary>
public class SequentialMessageQueue<TEntity> : IMessageQueue
    where TEntity : Entity, new()
{
    private bool _suspended;
    private readonly Queue<string> _mainQueue = new(512);
    private readonly List<string> _secondaryQueue = new();

    private readonly Dictionary<string, IMessageSpecificQueue> _namedQueues = new();

    public int QueuedCount => _mainQueue.Count;

    public void Register<TMessage>(IMessageConsumer<TMessage> consumer, string name, int order)
        where TMessage : class, IMessage, new()
    {
        if (!_namedQueues.TryGetValue(name, out var specificQueue))
        {
            specificQueue = new MessageSpecificQueue<TMessage, TEntity>();
            _namedQueues.Add(name, specificQueue);
        }

        ((MessageSpecificQueue<TMessage, TEntity>)specificQueue).Add(consumer, order);
    }

    public TMessage CreateMessage<TMessage>(string name)
        where TMessage : class, IMessage, new()
    {
        var message = _namedQueues.TryGetValue(name, out var specificQueue)
            ? ((MessageSpecificQueue<TMessage, TEntity>)specificQueue).CreateMessage()
            : new TMessage();
        message.MessageName = name;

        return message;
    }

    public void ReturnMessage<TMessage>(TMessage message)
        where TMessage : class, IMessage, new()
    {
        if (_namedQueues.TryGetValue(message.MessageName, out var specificQueue))
        {
            ((MessageSpecificQueue<TMessage, TEntity>)specificQueue).ReturnMessage(message);
        }
        else
        {
            message.Clean();
        }
    }

    public void Enqueue<TMessage>(TMessage message, bool lowPriority = false)
        where TMessage : class, IMessage, new()
    {
        if (_suspended
            || !_namedQueues.TryGetValue(message.MessageName, out var specificQueue))
        {
            message.Clean();

            return;
        }

        ((MessageSpecificQueue<TMessage, TEntity>)specificQueue).Enqueue(message);
        if (!lowPriority)
        {
            _mainQueue.Enqueue(message.MessageName);
            if (_mainQueue.Count > 16384)
            {
                throw new InvalidOperationException("Queue length limit exceeded");
            }
        }
        else
        {
            _secondaryQueue.Add(message.MessageName);
            if (_mainQueue.Count > 8192)
            {
                throw new InvalidOperationException("Secondary queue length limit exceeded");
            }
        }
    }

    public void ProcessQueue(EntityManager<TEntity> manager)
    {
        var i = 0;
        while (true)
        {
            while (_mainQueue.Count != 0)
            {
                if (++i > 65536)
                {
                    throw new InvalidOperationException("Queue is in infinite loop");
                }

                var messageName = _mainQueue.Dequeue();
                if (_namedQueues.TryGetValue(messageName, out var specificQueue))
                {
                    specificQueue.ProcessNext(manager);
                }
            }

            if (_secondaryQueue.Count == 0)
            {
                break;
            }

            _mainQueue.EnqueueRange(_secondaryQueue);
            _secondaryQueue.Clear();
        }

        manager.OnMessageQueueDrained();
    }

    public void Process<TMessage>(TMessage message, EntityManager<TEntity> manager)
        where TMessage : class, IMessage, new()
    {
        if (_namedQueues.TryGetValue(message.MessageName, out var specificQueue))
        {
            ((MessageSpecificQueue<TMessage, TEntity>)specificQueue).Process(message, manager);
        }
    }

    public IDisposable Suspend() => new QueueSuspender(this);

    private struct QueueSuspender : IDisposable
    {
        private readonly SequentialMessageQueue<TEntity> _queue;

        public QueueSuspender(SequentialMessageQueue<TEntity> queue)
        {
            _queue = queue;
            _queue._suspended = true;
        }

        public void Dispose() => _queue._suspended = false;
    }
}
