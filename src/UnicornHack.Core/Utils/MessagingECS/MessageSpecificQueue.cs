namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     Implementation detail of <see cref="SequentialMessageQueue{TEntity}" />
/// </summary>
/// <typeparam name="TMessage"> The message type </typeparam>
/// <typeparam name="TEntity"> The entity type </typeparam>
public class MessageSpecificQueue<TMessage, TEntity> : IMessageSpecificQueue
    where TMessage : class, IMessage, new()
    where TEntity : Entity, new()
{
    private readonly SortedList<int, IMessageConsumer<TMessage>> _consumers = new();

    private readonly Queue<TMessage> _queue = new();

    // FIFO pool of reusable message instances. Returned messages are appended to the tail and
    // rented from the head, so a just-returned message goes to the back of the line rather than
    // being handed straight back out — reducing the chance of a caller obtaining an instance the
    // code path that returned it is still touching.
    private const int PoolCapacity = 64;
    private readonly TMessage?[] _pool = new TMessage?[PoolCapacity];
    private int _poolHead;
    private int _poolTail;
    private int _poolCount;

    public void Add(IMessageConsumer<TMessage> consumer, int order)
        => _consumers.Add(order, consumer);

    public TMessage CreateMessage()
    {
        if (_poolCount != 0)
        {
            var pooled = _pool[_poolHead]!;
            _pool[_poolHead] = null;
            _poolHead = _poolHead + 1 == PoolCapacity ? 0 : _poolHead + 1;
            _poolCount--;
            return pooled;
        }

        return new TMessage();
    }

    public void ReturnMessage(TMessage message)
    {
        message.Clean();
        if (_poolCount == PoolCapacity)
        {
            // Pool full: drop the surplus instance and let the GC reclaim it.
            return;
        }

        _pool[_poolTail] = message;
        _poolTail = _poolTail + 1 == PoolCapacity ? 0 : _poolTail + 1;
        _poolCount++;
    }

    public void Enqueue(TMessage body) => _queue.Enqueue(body);

    public void ProcessNext(IEntityManager manager)
        => Process(_queue.Dequeue(), manager);

    public void Process(TMessage message, IEntityManager manager)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _consumers.Values.Count; i++)
        {
            var messageConsumer = _consumers.Values[i];
            if (messageConsumer.Process(message, manager) == MessageProcessingResult.StopProcessing)
            {
                break;
            }
        }

        ReturnMessage(message);
    }
}
