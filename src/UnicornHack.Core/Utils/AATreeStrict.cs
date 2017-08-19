using System;

namespace UnicornHack.Utils
{
    /// <summary>
    ///     AA-tree that does not allow duplicate keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    // ReSharper disable once InconsistentNaming
    public class AATreeStrict<TKey, TValue> : AATree<TKey, TValue> where TKey : IComparable<TKey>
    {
        public bool Remove(TKey key) => Remove(ref Root, key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            var node = Search(Root, key);
            if (node == null)
            {
                value = default;
                return false;
            }
            value = node.Value;
            return true;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var value))
                {
                    throw new InvalidOperationException($"Key {key} doesn't exist");
                }

                return value;
            }
            set
            {
                var node = Search(Root, key);
                if (node == null)
                {
                    Insert(key, value);
                }
                else
                {
                    node.Value = value;
                }
            }
        }

        public override (TKey, TValue) GetNextSmaller(TKey key)
        {
            var node = Root;
            var next = Sentinel;
            while (node != Sentinel)
            {
                var compare = key.CompareTo(node.Key);
                if (compare <= 0)
                {
                    node = node.Left;
                }
                if (compare > 0)
                {
                    if (next == Sentinel || next.Key.CompareTo(node.Key) < 0)
                    {
                        next = node;
                    }
                    node = node.Right;
                }
            }

            if (next == Sentinel)
            {
                throw new InvalidOperationException("Empty");
            }

            return (next.Key, next.Value);
        }

        public override (TKey, TValue) GetNextLarger(TKey key)
        {
            var node = Root;
            var next = Sentinel;
            while (node != Sentinel)
            {
                var compare = key.CompareTo(node.Key);
                if (compare < 0)
                {
                    if (next == Sentinel || next.Key.CompareTo(node.Key) > 0)
                    {
                        next = node;
                    }
                    node = node.Left;
                }
                if (compare >= 0)
                {
                    node = node.Right;
                }
            }

            if (next == Sentinel)
            {
                throw new InvalidOperationException("Empty");
            }

            return (next.Key, next.Value);
        }

        private Node Search(Node node, TKey key)
        {
            while (true)
            {
                if (node == Sentinel)
                {
                    return null;
                }

                var compare = key.CompareTo(node.Key);
                if (compare < 0)
                {
                    node = node.Left;
                    continue;
                }
                if (compare > 0)
                {
                    node = node.Right;
                    continue;
                }
                return node;
            }
        }

        protected bool Remove(ref Node node, TKey key)
        {
            if (node == Sentinel)
            {
                return Deleted != null;
            }

            var compare = key.CompareTo(node.Key);
            if (compare < 0)
            {
                if (!Remove(ref node.Left, key))
                {
                    return false;
                }
            }
            else
            {
                if (compare == 0)
                {
                    Deleted = node;
                }
                if (!Remove(ref node.Right, key))
                {
                    return false;
                }
            }

            if (Deleted != null)
            {
                Deleted.Key = node.Key;
                Deleted.Value = node.Value;
                Deleted = null;
                node = node.Right;
            }
            else if (node.Left.Level < node.Level - 1 || node.Right.Level < node.Level - 1)
            {
                node.Level--;
                if (node.Right.Level > node.Level)
                {
                    node.Right.Level = node.Level;
                }

                Skew(ref node);
                Skew(ref node.Right);
                Skew(ref node.Right.Right);
                Split(ref node);
                Split(ref node.Right);
            }

            return true;
        }

        protected override bool AllowsDuplicateKeys => false;
    }
}