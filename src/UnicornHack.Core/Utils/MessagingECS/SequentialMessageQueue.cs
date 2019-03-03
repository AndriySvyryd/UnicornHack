using System;
using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    /// <summary>
    ///     A message queue that processes messages in the order they were enqueued
    /// </summary>
    public class SequentialMessageQueue<TState> : IMessageQueue
    {
        private bool _suspended;
        private readonly Queue<string> _mainQueue = new Queue<string>(512);
        private readonly List<string> _secondaryQueue = new List<string>();

        private readonly Dictionary<string, IMessageSpecificQueue<TState>> _namedQueues =
            new Dictionary<string, IMessageSpecificQueue<TState>>();

        public int QueuedCount => _mainQueue.Count;

        public void Add<TMessage>(IMessageConsumer<TMessage, TState> consumer, string name, int order)
            where TMessage : class, IDisposable, new()
        {
            if (!_namedQueues.TryGetValue(name, out var specificQueue))
            {
                specificQueue = new MessageSpecificQueue<TMessage, TState>();
                _namedQueues.Add(name, specificQueue);
            }

            ((MessageSpecificQueue<TMessage, TState>)specificQueue).Add(consumer, order);
        }

        public TMessage CreateMessage<TMessage>(string name)
            where TMessage : class, IMessage, new()
        {
            var message = _namedQueues.TryGetValue(name, out var specificQueue)
                ? ((MessageSpecificQueue<TMessage, TState>)specificQueue).CreateMessage()
                : new TMessage();
            message.MessageName = name;

            return message;
        }

        public void ReturnMessage<TMessage>(TMessage message)
            where TMessage : class, IMessage, new()
        {
            if (_namedQueues.TryGetValue(message.MessageName, out var specificQueue))
            {
                ((MessageSpecificQueue<TMessage, TState>)specificQueue).ReturnMessage(message);
            }
            else
            {
                message.Dispose();
            }
        }

        public void Enqueue<TMessage>(TMessage message, bool lowPriority = false)
            where TMessage : class, IMessage, new()
        {
            if (_suspended
                || !_namedQueues.TryGetValue(message.MessageName, out var specificQueue))
            {
                message.Dispose();

                return;
            }

            ((MessageSpecificQueue<TMessage, TState>)specificQueue).Enqueue(message);
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

        public void ProcessQueue(TState state)
        {
            var i = 0;
            while (_mainQueue.Count != 0)
            {
                if (++i > 65536)
                {
                    throw new InvalidOperationException("Queue is in infinite loop");
                }

                var messageName = _mainQueue.Dequeue();
                if (_namedQueues.TryGetValue(messageName, out var specificQueue))
                {
                    specificQueue.ProcessNext(state, this);
                }
            }

            if (_secondaryQueue.Count != 0)
            {
                _mainQueue.EnqueueRange(_secondaryQueue);
                _secondaryQueue.Clear();
                ProcessQueue(state);
            }
        }

        public void Process<TMessage>(TMessage message, TState state)
            where TMessage : class, IMessage, new()
        {
            if (_namedQueues.TryGetValue(message.MessageName, out var specificQueue))
            {
                ((MessageSpecificQueue<TMessage, TState>)specificQueue).Process(message, state);
            }
        }

        public IDisposable Suspend() => new QueueSuspender(this);

        private struct QueueSuspender : IDisposable
        {
            private readonly SequentialMessageQueue<TState> _queue;

            public QueueSuspender(SequentialMessageQueue<TState> queue)
            {
                _queue = queue;
                _queue._suspended = true;
            }

            public void Dispose() => _queue._suspended = false;
        }
    }
}
