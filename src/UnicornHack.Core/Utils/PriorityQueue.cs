using System.Collections.Generic;

namespace UnicornHack.Utils
{
    public class PriorityQueue<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly IComparer<T> _comparer;

        public PriorityQueue()
            : this(Comparer<T>.Default, capacity: 0)
        {
        }

        public PriorityQueue(IComparer<T> comparer)
            : this(comparer, capacity: 0)
        {
        }

        public PriorityQueue(IComparer<T> comparer, int capacity)
        {
            _comparer = comparer;
            if (capacity != 0)
            {
                _list.Capacity = capacity;
            }
        }

        public virtual int Push(T element)
        {
            var newElementPosition = _list.Count;
            _list.Add(element);

            return BubbleUp(newElementPosition);
        }

        public virtual T Pop()
        {
            var result = _list[index: 0];
            _list[index: 0] = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count - 1);

            BubbleDown(position: 0);

            return result;
        }

        public virtual T this[int index]
        {
            get { return _list[index]; }
            set
            {
                _list[index] = value;
                Update(index);
            }
        }

        private void Update(int position)
        {
            var newPosition = BubbleUp(position);
            if (newPosition != position)
            {
                return;
            }

            BubbleDown(newPosition);
        }

        private int BubbleUp(int position)
        {
            do
            {
                if (position == 0)
                {
                    break;
                }

                var parentPosition = (position - 1) >> 1;
                if (CompareItemsAt(parentPosition, position) <= 0)
                {
                    break;
                }

                SwitchElements(parentPosition, position);
                position = parentPosition;
            } while (true);

            return position;
        }

        private void BubbleDown(int position)
        {
            do
            {
                var parentPosition = position;
                var leftChildPosition = (position << 1) + 1;
                var rightChildPosition = (position << 1) + 2;

                if (_list.Count > leftChildPosition && CompareItemsAt(position, leftChildPosition) > 0)
                {
                    position = leftChildPosition;
                }
                if (_list.Count > rightChildPosition && CompareItemsAt(position, rightChildPosition) > 0)
                {
                    position = rightChildPosition;
                }

                if (position == parentPosition)
                {
                    break;
                }

                SwitchElements(position, parentPosition);
            } while (true);
        }

        public T Peek() => _list.Count > 0 ? _list[index: 0] : default(T);

        public void Clear() => _list.Clear();

        public int Count => _list.Count;

        private void SwitchElements(int i, int j)
        {
            var temp = _list[i];
            _list[i] = _list[j];
            _list[j] = temp;
        }

        private int CompareItemsAt(int i, int j) => _comparer.Compare(_list[i], _list[j]);
    }
}