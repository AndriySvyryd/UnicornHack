using System.Collections.Generic;

namespace UnicornHack.Utils
{
    public class PriorityQueue<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly IComparer<T> _comparer;

        public PriorityQueue()
        {
            _comparer = Comparer<T>.Default;
        }

        public PriorityQueue(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public PriorityQueue(IComparer<T> comparer, int capacity)
        {
            _comparer = comparer;
            _list.Capacity = capacity;
        }

        public int Push(T element)
        {
            int newElementPosition = _list.Count;
            _list.Add(element);

            return BubbleUp(newElementPosition);
        }

        public T Pop()
        {
            var result = _list[index: 0];
            var newPosition = 0;
            _list[newPosition] = _list[_list.Count - 1];
            _list.RemoveAt(_list.Count - 1);

            BubbleDown(newPosition);

            return result;
        }

        public void Update(int position)
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
            int parentPosition;
            do
            {
                if (position == 0)
                {
                    break;
                }

                parentPosition = (position - 1) >> 1;
                if (CompareItemsAt(parentPosition, position) > 0)
                {
                    SwitchElements(parentPosition, position);
                    position = parentPosition;
                }
                else
                {
                    break;
                }
            } while (true);

            return position;
        }

        private void BubbleDown(int position)
        {
            int parentPosition;
            int leftChildPosition;
            int rightChildPosition;
            do
            {
                parentPosition = position;
                leftChildPosition = (position << 1) + 1;
                rightChildPosition = (position << 1) + 2;
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

        public T Peek()
        {
            if (_list.Count > 0)
            {
                return _list[index: 0];
            }
            return default(T);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public T this[int index]
        {
            get { return _list[index]; }
            set
            {
                _list[index] = value;
                Update(index);
            }
        }

        private void SwitchElements(int i, int j)
        {
            var temp = _list[i];
            _list[i] = _list[j];
            _list[j] = temp;
        }

        private int CompareItemsAt(int i, int j)
        {
            return _comparer.Compare(_list[i], _list[j]);
        }
    }
}