using System;
using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    /// <summary>
    ///     Implementation detail of <see cref="SequentialMessageQueue{TState}" />
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <typeparam name="TState">The state type</typeparam>
    public class MessageSpecificQueue<TMessage, TState> : IMessageSpecificQueue<TState>
        where TMessage : class, IDisposable, new()
    {
        private readonly SortedList<int, IMessageConsumer<TMessage, TState>> _consumers =
            new SortedList<int, IMessageConsumer<TMessage, TState>>();

        private readonly Queue<TMessage> _queue = new Queue<TMessage>();
        private TMessage _pooledMessage;

        public void Add(IMessageConsumer<TMessage, TState> consumer, int order)
            => _consumers.Add(order, consumer);

        public TMessage CreateMessage()
        {
            if (_pooledMessage != null)
            {
                var message = _pooledMessage;
                _pooledMessage = null;
                return message;
            }

            return new TMessage();
        }

        public void ReturnMessage(TMessage message)
        {
            message.Dispose();
            if (_pooledMessage != null)
            {
                _pooledMessage = message;
            }
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

            message.Dispose();
        }
    }
}
