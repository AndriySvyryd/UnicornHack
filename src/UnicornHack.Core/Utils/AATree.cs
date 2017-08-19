using System;
using System.Collections;
using System.Collections.Generic;

namespace UnicornHack.Utils
{
    // ReSharper disable once InconsistentNaming
    public abstract class AATree<TKey, TValue> : IEnumerable<(TKey, TValue)> where TKey : IComparable<TKey>
    {
        protected static readonly Node Sentinel = new Node();

        protected Node Root;
        protected Node Deleted;

        protected AATree()
        {
            Root = Sentinel;
            Deleted = null;
        }

        public bool Insert(TKey key, TValue value) => Insert(ref Root, key, value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<(TKey, TValue)> GetEnumerator() => GetAll((k, v) => (k, v)).GetEnumerator();

        public IEnumerable<TResult> GetAll<TResult>(Func<TKey, TValue, TResult> selector) =>
            GetAll(Root, selector, new List<TResult>());

        private IEnumerable<TResult> GetAll<TResult>(Node node, Func<TKey, TValue, TResult> selector,
            List<TResult> result)
        {
            while (true)
            {
                if (node == Sentinel)
                {
                    return result;
                }

                GetAll(node.Left, selector, result);
                result.Add(selector(node.Key, node.Value));
                node = node.Right;
            }
        }

        public (TKey, TValue) GetMin()
        {
            var node = Root;
            var min = node;
            while (node != Sentinel)
            {
                min = node;
                node = node.Left;
            }

            if (min == Sentinel)
            {
                throw new InvalidOperationException("Empty");
            }

            return (min.Key, min.Value);
        }

        public (TKey, TValue) GetMax()
        {
            var node = Root;
            var max = node;
            while (node != Sentinel)
            {
                max = node;
                node = node.Right;
            }

            if (max == Sentinel)
            {
                throw new InvalidOperationException("Empty");
            }

            return (max.Key, max.Value);
        }

        public abstract (TKey, TValue) GetNextSmaller(TKey key);

        public abstract (TKey, TValue) GetNextLarger(TKey key);

        public IEnumerable<ValueTuple<TKey, TValue>> GetRange(TKey minKey, TKey maxKey) => GetRange(Root, minKey,
            maxKey, true, true, (k, v) => (k, v), new List<ValueTuple<TKey, TValue>>());

        public IEnumerable<TResult> GetRange<TResult>(TKey minKey, TKey maxKey, Func<TKey, TValue, TResult> selector) =>
            GetRange(Root, minKey, maxKey, true, true, selector, new List<TResult>());

        protected IEnumerable<TResult> GetRange<TResult>(Node node, TKey minKey, TKey maxKey, bool compareMin,
            bool compareMax, Func<TKey, TValue, TResult> selector, List<TResult> result)
        {
            while (node != Sentinel)
            {
                if (!compareMin && !compareMax)
                {
                    return GetAll(node, selector, result);
                }

                var minCompare = compareMin ? minKey.CompareTo(node.Key) : -1;
                var maxCompare = compareMax ? maxKey.CompareTo(node.Key) : 1;
                if (minCompare <= 0)
                {
                    GetRange(node.Left, minKey, maxKey, compareMin, maxCompare <= 0, selector, result);
                }
                if (minCompare <= 0 && maxCompare >= 0)
                {
                    result.Add(selector(node.Key, node.Value));
                }
                if (maxCompare >= 0)
                {
                    node = node.Right;
                    compareMin = minCompare >= 0;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        ///     Rotate right
        /// </summary>
        /// <param name="node"></param>
        protected void Skew(ref Node node)
        {
            if (node.Level != node.Left.Level)
            {
                return;
            }

            var left = node.Left;
            node.Left = left.Right;
            left.Right = node;
            node = left;
        }

        /// <summary>
        ///     Rotate left
        /// </summary>
        /// <param name="node"></param>
        protected void Split(ref Node node)
        {
            if (node.Right.Right.Level != node.Level)
            {
                return;
            }

            var right = node.Right;
            node.Right = right.Left;
            right.Left = node;
            node = right;
            node.Level++;
        }

        protected bool Insert(ref Node node, TKey key, TValue value)
        {
            if (node == Sentinel)
            {
                node = new Node(key, value, Sentinel);
                return true;
            }

            var compare = key.CompareTo(node.Key);
            if (compare < 0)
            {
                if (!Insert(ref node.Left, key, value))
                {
                    return false;
                }
            }
            else if (compare > 0)
            {
                if (!Insert(ref node.Right, key, value))
                {
                    return false;
                }
            }
            else
            {
                if (!AllowsDuplicateKeys || !Insert(ref node.Left, key, value))
                {
                    return false;
                }
            }

            Skew(ref node);
            Split(ref node);

            return true;
        }

        protected virtual bool AllowsDuplicateKeys => true;

        protected class Node
        {
            public int Level;
            public Node Left;
            public Node Right;
            public TKey Key;
            public TValue Value;

            /// <summary>
            ///     Sentinel
            /// </summary>
            public Node()
            {
                Level = 0;
                Left = this;
                Right = this;
            }

            public Node(TKey key, TValue value, Node sentinel)
            {
                Level = 1;
                Left = sentinel;
                Right = sentinel;
                Key = key;
                Value = value;
            }
        }
    }
}