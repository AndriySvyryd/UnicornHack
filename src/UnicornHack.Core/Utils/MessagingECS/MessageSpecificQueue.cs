namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     Implementation detail of <see cref="SequentialMessageQueue{TState}" />
/// </summary>
/// <typeparam name="TMessage"> The message type </typeparam>
/// <typeparam name="TState"> The state type </typeparam>
public class MessageSpecificQueue<TMessage, TState> : IMessageSpecificQueue<TState>
    where TMessage : class, IMessage, new()
{
    private readonly SortedList<int, IMessageConsumer<TMessage, TState>> _consumers = new();

    private readonly Queue<TMessage> _queue = new();
    private TMessage? _pooledMessage;

    public void Add(IMessageConsumer<TMessage, TState> consumer, int order)
        => _consumers.Add(order, consumer);

    public TMessage CreateMessage()
        => Interlocked.Exchange(ref _pooledMessage, null)
           ?? new TMessage();

    public void ReturnMessage(TMessage message)
    {
        message.Clean();
        Interlocked.CompareExchange(ref _pooledMessage, message, null);
    }

    public void Enqueue(TMessage body) => _queue.Enqueue(body);

    public void ProcessNext(TState state, IMessageQueue queue) => Process(_queue.Dequeue(), state);

    public void Process(TMessage message, TState state)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _consumers.Values.Count; i++)
        {
            var messageConsumer = _consumers.Values[i];
            if (messageConsumer.Process(message, state) == MessageProcessingResult.StopProcessing)
            {
                break;
            }
        }

        ReturnMessage(message);
    }
}
