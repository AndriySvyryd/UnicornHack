using UnicornHack.Utils.Caching;

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

    [ThreadStatic]
    private static ListObjectPool<TMessage>? _pool;

    private static ListObjectPool<TMessage> CurrentPool
        => _pool ??= new(static () => new TMessage(), initialSize: 64, maxSize: 64, preallocatedCount: 0);

    public void Add(IMessageConsumer<TMessage> consumer, int order)
        => _consumers.Add(order, consumer);

    public TMessage CreateMessage() => CurrentPool.Rent();

    public void ReturnMessage(TMessage message)
    {
        message.Clean();
        CurrentPool.Return(message);
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
