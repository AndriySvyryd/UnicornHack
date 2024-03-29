﻿namespace UnicornHack.Utils.DataStructures;

public class ObservableSnapshotableHashSet<T> : ObservableHashSet<T>, ISnapshotableCollection<T>
{
    public HashSet<T>? Snapshot
    {
        get;
        private set;
    }

    public HashSet<T> CreateSnapshot()
    {
        if (Snapshot == null)
        {
            Snapshot = new HashSet<T>(Set, Comparer);
        }
        else
        {
            Snapshot.Clear();
            Snapshot.AddRange(Set);
        }

        return Snapshot;
    }
}
