using System.Collections;
using System.Collections.Generic;
using CSharpScriptSerialization;

namespace UnicornHack.Generation
{
    public class Weight
    {
    }

    public class ConstantWeight : Weight
    {
        public virtual int W { get; set; }
    }

    public class DefaultWeight : Weight
    {
        public virtual float Multiplier { get; set; }
    }

    public class InfiniteWeight : Weight, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<InfiniteWeight>();
        public ICSScriptSerializer GetSerializer() => Serializer;
    }

    public class BranchWeight : Weight
    {
        public virtual Weight W { get; set; }

        public virtual string Name { get; set; }
        public virtual int MinDepth { get; set; }
        public virtual int MaxDepth { get; set; }
        //TODO: Distribution
    }

    public class NotBranchWeight : Weight
    {
        public virtual Weight W { get; set; }

        public virtual string Name { get; set; }
        public virtual short MinDepth { get; set; }
        public virtual short MaxDepth { get; set; }
    }

    public class ConnectedBranchWeight : Weight
    {
    }

    public class InstancesWeight : Weight
    {
        public virtual Weight W { get; set; }

        public virtual int Max { get; set; }
        //TODO: Distribution
        //TODO: Scope: Game, Branch, Level
    }

    public class TagInstancesWeight : Weight
    {
        public virtual Weight W { get; set; }

        public virtual string Name { get; set; }
        public virtual int Max { get; set; }
        //TODO: Distribution
        //TODO: Scope: Game, Branch, Level
    }

    public class MaxWeight : Weight, ICollection<Weight>
    {
        public virtual ICollection<Weight> W { get; set; }

        IEnumerator<Weight> IEnumerable<Weight>.GetEnumerator() => W.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)W).GetEnumerator();
        void ICollection<Weight>.Add(Weight item) => W.Add(item);
        void ICollection<Weight>.Clear() => W.Clear();
        bool ICollection<Weight>.Contains(Weight item) => W.Contains(item);
        void ICollection<Weight>.CopyTo(Weight[] array, int arrayIndex) => W.CopyTo(array, arrayIndex);
        bool ICollection<Weight>.Remove(Weight item) => W.Remove(item);
        int ICollection<Weight>.Count => W.Count;
        bool ICollection<Weight>.IsReadOnly => W.IsReadOnly;
    }

    public class MinWeight : Weight, ICollection<Weight>
    {
        public virtual ICollection<Weight> W { get; set; }

        IEnumerator<Weight> IEnumerable<Weight>.GetEnumerator() => W.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => W.GetEnumerator();
        void ICollection<Weight>.Add(Weight item) => W.Add(item);
        void ICollection<Weight>.Clear() => W.Clear();
        bool ICollection<Weight>.Contains(Weight item) => W.Contains(item);
        void ICollection<Weight>.CopyTo(Weight[] array, int arrayIndex) => W.CopyTo(array, arrayIndex);
        bool ICollection<Weight>.Remove(Weight item) => W.Remove(item);
        int ICollection<Weight>.Count => W.Count;
        bool ICollection<Weight>.IsReadOnly => W.IsReadOnly;
    }
}