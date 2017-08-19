using System;
using System.Collections.Generic;

namespace UnicornHack.Utils
{
    /// <summary>
    ///     AA-tree that allows duplicate keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    // ReSharper disable once InconsistentNaming
    public class AATreeLax<TKey, TValue> : AATree<TKey, TValue> where TKey : IComparable<TKey>
    {
        public bool Remove(TKey key, TValue value) => Remove(ref Root, key, value);

        public IEnumerable<TValue> GetValues(TKey key) => GetValues(Root, key, new List<TValue>());

        public override (TKey, TValue) GetNextSmaller(TKey key)
        {
            var node = GetNextSmaller(Root, key);
            if (node == Sentinel)
            {
                throw new InvalidOperationException("Empty");
            }

            return (node.Key, node.Value);
        }

        private Node GetNextSmaller(Node node, TKey key)
        {
            var next = Sentinel;
            while (node != Sentinel)
            {
                var compare = key.CompareTo(node.Key);
                if (compare <= 0)
                {
                    if (compare == 0)
                    {
                        var rightSmaller = GetNextSmaller(node.Right, key);
                        if (rightSmaller != Sentinel && (next == Sentinel || next.Key.CompareTo(rightSmaller.Key) < 0))
                        {
                            next = rightSmaller;
                        }
                    }
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

            return next;
        }

        public override (TKey, TValue) GetNextLarger(TKey key)
        {
            var node = GetNextLarger(Root, key);
            if (node == Sentinel)
            {
                throw new InvalidOperationException("Empty");
            }

            return (node.Key, node.Value);
        }

        private Node GetNextLarger(Node node, TKey key)
        {
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
                    if (compare == 0)
                    {
                        var leftLarger = GetNextLarger(node.Left, key);
                        if (leftLarger != Sentinel && (next == Sentinel || next.Key.CompareTo(leftLarger.Key) > 0))
                        {
                            next = leftLarger;
                        }
                    }
                    node = node.Right;
                }
            }

            return next;
        }

        private IEnumerable<TValue> GetValues(Node node, TKey key, List<TValue> result)
        {
            if (node == Sentinel)
            {
                return result;
            }

            var compare = key.CompareTo(node.Key);
            if (compare <= 0)
            {
                GetValues(node.Left, key, result);
            }
            if (compare == 0)
            {
                result.Add(node.Value);
            }
            if (compare >= 0)
            {
                GetValues(node.Right, key, result);
            }

            return result;
        }

        protected bool Remove(ref Node node, TKey key, TValue value)
        {
            if (node == Sentinel)
            {
                return Deleted != null;
            }

            var compare = key.CompareTo(node.Key);
            if (compare < 0)
            {
                if (!Remove(ref node.Left, key, value))
                {
                    return false;
                }
            }
            else
            {
                var skip = false;
                if (compare == 0)
                {
                    if (Equals(node.Value, value))
                    {
                        Deleted = node;
                    }
                    else
                    {
                        skip = Remove(ref node.Left, key, value);
                    }
                }
                if (!skip && !Remove(ref node.Right, key, value))
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
    }
}