using System.Collections.Generic;

namespace UnicornHack.Utils.DataStructures
{
    public interface ISnapshotableCollection<T> : ICollection<T>
    {
        HashSet<T> Snapshot { get; }
        HashSet<T> CreateSnapshot();
    }
}
