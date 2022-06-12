using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicornHack.Utils.DataStructures
{
    /// <summary>
    ///     A priority queue with O(log n) update.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    /// <typeparam name="TPriority"></typeparam>
    public class PrioritySet<TElement, TPriority> : IReadOnlyCollection<(TElement Element, TPriority Priority)>
        where TElement : notnull
    {
        private const int DefaultCapacity = 4;

        private readonly Dictionary<TElement, int> _index;

        private HeapEntry[] _heap;
        private int _version;

        #region Constructors

        public PrioritySet()
            : this(0, null, null)
        {
        }

        public PrioritySet(int initialCapacity)
            : this(initialCapacity, null, null)
        {
        }

        public PrioritySet(IComparer<TPriority> comparer)
            : this(0, null, null)
        {
        }

        public PrioritySet(int initialCapacity, IComparer<TPriority> priorityComparer,
            IEqualityComparer<TElement> elementComparer)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            }

            if (initialCapacity == 0)
            {
                _heap = Array.Empty<HeapEntry>();
            }
            else
            {
                _heap = new HeapEntry[initialCapacity];
            }

            _index = new Dictionary<TElement, int>(initialCapacity, comparer: elementComparer);
            Comparer = priorityComparer ?? Comparer<TPriority>.Default;
        }

        public PrioritySet(IEnumerable<(TElement Element, TPriority Priority)> values)
            : this(values, null, null)
        {
        }

        public PrioritySet(
            IEnumerable<(TElement Element, TPriority Priority)> values,
            IComparer<TPriority> comparer,
            IEqualityComparer<TElement> elementComparer)
        {
            Comparer = comparer ?? Comparer<TPriority>.Default;
            _index = new Dictionary<TElement, int>(elementComparer);
            _heap = Array.Empty<HeapEntry>();
            Count = 0;

            AppendRaw(values);
            Heapify();
        }

        #endregion

        public int Count { get; private set; }

        public IComparer<TPriority> Comparer { get; }

        public void Enqueue(TElement element, TPriority priority)
        {
            if (_index.ContainsKey(element))
            {
                throw new InvalidOperationException("Duplicate element");
            }

            _version++;
            Insert(element, priority);
        }

        public TElement Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            return _heap[0].Element;
        }

        public bool TryPeek(out TElement element, out TPriority priority)
        {
            if (Count == 0)
            {
                element = default!;
                priority = default!;
                return false;
            }

            (element, priority) = _heap[0];
            return true;
        }

        public TElement Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            _version++;
            RemoveIndex(index: 0, out var result, out _);
            return result;
        }

        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (Count == 0)
            {
                element = default!;
                priority = default!;
                return false;
            }

            _version++;
            RemoveIndex(index: 0, out element, out priority);
            return true;
        }

        public TElement EnqueueDequeue(TElement element, TPriority priority)
        {
            if (Count == 0)
            {
                return element;
            }

            if (_index.ContainsKey(element))
            {
                // Set invariant validation assumes behaviour equivalent to
                // calling Enqueue(); Dequeue() operations sequentially.
                // Might consider changing to a Dequeue(); Enqueue() equivalent
                // which is more forgiving under certain scenaria.
                throw new InvalidOperationException("Duplicate element");
            }

            ref var minEntry = ref _heap[0];
            if (Comparer.Compare(priority, minEntry.Priority) <= 0)
            {
                return element;
            }

            _version++;
            var minElement = minEntry.Element;
            var result = _index.Remove(minElement);
            Debug.Assert(result, "Could not find element in index");
            BubbleDown(index: 0, in element, in priority);
            return minElement;
        }

        public void Clear()
        {
            _version++;
            if (Count > 0)
            {
                //if (RuntimeHelpers.IsReferenceOrContainsReferences<HeapEntry>())
                {
                    Array.Clear(_heap, 0, Count);
                }

                _index.Clear();
                Count = 0;
            }
        }

        public bool Contains(TElement element) => _index.ContainsKey(element);

        public bool TryRemove(TElement element)
        {
            if (!_index.TryGetValue(element, out var index))
            {
                return false;
            }

            _version++;
            RemoveIndex(index, out _, out _);
            return true;
        }

        public bool TryUpdate(TElement element, TPriority priority)
        {
            if (!_index.TryGetValue(element, out var index))
            {
                return false;
            }

            _version++;
            UpdateIndex(index, priority);
            return true;
        }

        public void EnqueueOrUpdate(TElement element, TPriority priority)
        {
            _version++;
            if (_index.TryGetValue(element, out var index))
            {
                UpdateIndex(index, priority);
            }
            else
            {
                Insert(element, priority);
            }
        }

        private void Heapify()
        {
            var heap = _heap;

            for (var i = (Count - 1) >> 2; i >= 0; i--)
            {
                var entry = heap[i]; // ensure struct is copied before sifting
                BubbleDown(i, in entry.Element, in entry.Priority);
            }
        }

        private void Insert(in TElement element, in TPriority priority)
        {
            if (Count == _heap.Length)
            {
                Resize(ref _heap);
            }

            BubbleUp(index: Count++, in element, in priority);
        }

        private void RemoveIndex(int index, out TElement element, out TPriority priority)
        {
            Debug.Assert(index < Count);

            (element, priority) = _heap[index];

            var lastElementPos = --Count;
            ref var lastElement = ref _heap[lastElementPos];

            if (lastElementPos > 0)
            {
                BubbleDown(index, in lastElement.Element, in lastElement.Priority);
            }

            lastElement = default;

            var result = _index.Remove(element);
            Debug.Assert(result, "Could not find element in index");
        }

        private void UpdateIndex(int index, TPriority newPriority)
        {
            TElement element;
            ref var entry = ref _heap[index];

            switch (Comparer.Compare(newPriority, entry.Priority))
            {
                // priority is decreased, bubble upward
                case < 0:
                    element = entry.Element;
                    BubbleUp(index, element, newPriority);
                    return;

                // priority is increased, bubble downward
                case > 0:
                    element = entry.Element;
                    BubbleDown(index, element, newPriority);
                    return;

                // priority is same as before, take no action
                default:
                    return;
            }
        }

        private void AppendRaw(IEnumerable<(TElement Element, TPriority Priority)> values)
        {
            var heap = _heap;
            var index = _index;
            var count = Count;

            foreach (var (element, priority) in values)
            {
                if (count == heap.Length)
                {
                    Resize(ref heap);
                }

                if (!index.TryAdd(element, count))
                {
                    throw new ArgumentException("Duplicate elements", nameof(values));
                }

                ref var entry = ref heap[count];
                entry.Element = element;
                entry.Priority = priority;
                count++;
            }

            _heap = heap;
            Count = count;
        }

        private void BubbleUp(int index, in TElement element, in TPriority priority)
        {
            while (index > 0)
            {
                var parentIndex = (index - 1) >> 2;
                ref var parent = ref _heap[parentIndex];

                if (Comparer.Compare(parent.Priority, priority) <= 0)
                {
                    break;
                }

                _heap[index] = parent;
                _index[parent.Element] = index;
                index = parentIndex;
            }

            ref var entry = ref _heap[index];
            entry.Element = element;
            entry.Priority = priority;
            _index[element] = index;
        }

        private void BubbleDown(int index, in TElement element, in TPriority priority)
        {
            int minChildIndex;
            var count = Count;
            var heap = _heap;

            while ((minChildIndex = (index << 2) + 1) < count)
            {
                ref var minChild = ref heap[minChildIndex];
                var childUpperBound = Math.Min(count, minChildIndex + 4);

                for (var nextChildIndex = minChildIndex + 1; nextChildIndex < childUpperBound; nextChildIndex++)
                {
                    ref var nextChild = ref heap[nextChildIndex];
                    if (Comparer.Compare(nextChild.Priority, minChild.Priority) < 0)
                    {
                        minChildIndex = nextChildIndex;
                        minChild = ref nextChild;
                    }
                }

                if (Comparer.Compare(priority, minChild.Priority) <= 0)
                {
                    break;
                }

                heap[index] = minChild;
                _index[minChild.Element] = index;
                index = minChildIndex;
            }

            ref var entry = ref heap[index];
            entry.Element = element;
            entry.Priority = priority;
            _index[element] = index;
        }

        private void Resize(ref HeapEntry[] heap)
        {
            var newSize = heap.Length == 0 ? DefaultCapacity : 2 * heap.Length;
            Array.Resize(ref heap, newSize);
        }

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.
            GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<(TElement Element, TPriority Priority)>
        {
            private readonly PrioritySet<TElement, TPriority> _queue;
            private readonly int _version;
            private int _index;

            internal Enumerator(PrioritySet<TElement, TPriority> queue)
            {
                _version = queue._version;
                _queue = queue;
                _index = 0;
                Current = default;
            }

            public bool MoveNext()
            {
                var queue = _queue;

                if (queue._version == _version && _index < queue.Count)
                {
                    ref var entry = ref queue._heap[_index];
                    Current = (entry.Element, entry.Priority);
                    _index++;
                    return true;
                }

                if (queue._version != _version)
                {
                    throw new InvalidOperationException("Collection was modified");
                }

                return false;
            }

            public (TElement Element, TPriority Priority) Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Reset()
            {
                if (_queue._version != _version)
                {
                    throw new InvalidOperationException("Collection was modified");
                }

                _index = 0;
                Current = default;
            }

            public void Dispose()
            {
            }
        }

        private struct HeapEntry
        {
            public TElement Element;
            public TPriority Priority;

            public void Deconstruct(out TElement element, out TPriority priority)
            {
                element = Element;
                priority = Priority;
            }
        }
    }
}
