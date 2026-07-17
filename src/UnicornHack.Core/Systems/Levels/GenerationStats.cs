using System.Collections.ObjectModel;
using UnicornHack.Utils;

namespace UnicornHack.Systems.Levels;

/// <summary>
///     Persistable generation statistics shared between scopes. Owned by both
///     <see cref="UnicornHack.Game" /> (game-wide cumulative aggregate, surfaced to
///     weight expressions as <c>$globalInstances</c> / <c>$globalTagInstances</c>)
///     and <see cref="LevelGenerationStats" /> which inherits from this type
///     (per-level snapshot, surfaced as <c>$instances</c> / <c>$tagInstances</c>).
///     <para>
///         Each <see cref="Dictionary{TKey,TValue}" /> exposed on this entity is
///         the in-memory source of truth; the corresponding private owned-collection
///         field (<c>_*Entries</c>) is the relational projection EF Core persists.
///         The two are kept in sync lazily — <see cref="SyncEntries" /> reconciles the
///         backing <see cref="ObservableCollection{T}" /> against the dictionary during
///         a SaveChanges flow, and <see cref="EnsureDictionariesInitialized" /> goes
///         the other way on first dictionary access after EF materialization.
///     </para>
/// </summary>
public class GenerationStats : NotificationEntity
{
    private protected bool _dictionariesInitialized;

    private readonly ObservableCollection<KeyCountPair<string>> _fragmentInstancesByNameEntries = [];
    private readonly ObservableCollection<KeyCountPair<string>> _fragmentInstancesByTagEntries = [];
    private readonly ObservableCollection<KeyCountPair<RoomFeatures>> _fragmentInstancesByFeatureEntries = [];
    private readonly ObservableCollection<KeyCountPair<string>> _creatureInstancesByNameEntries = [];
    private readonly ObservableCollection<KeyCountPair<CreatureGenerationFlags>> _creatureInstancesByFlagEntries = [];
    private readonly ObservableCollection<KeyCountPair<string>> _itemInstancesByNameEntries = [];
    private readonly ObservableCollection<KeyCountPair<ItemType>> _itemInstancesByTypeEntries = [];
    private readonly ObservableCollection<KeyCountPair<RoomFeatures>> _roomCountsByFeatureEntries = [];

    private readonly Dictionary<string, int> _fragmentInstancesByName = [];
    private readonly Dictionary<string, int> _fragmentInstancesByTag = [];
    private readonly Dictionary<RoomFeatures, int> _fragmentInstancesByFeature = [];
    private readonly Dictionary<string, int> _creatureInstancesByName = [];
    private readonly Dictionary<CreatureGenerationFlags, int> _creatureInstancesByFlag = [];
    private readonly Dictionary<string, int> _itemInstancesByName = [];
    private readonly Dictionary<ItemType, int> _itemInstancesByType = [];
    private readonly Dictionary<RoomFeatures, int> _roomCountsByFeature = [];

    /// <summary>Map-fragment instance counts keyed by fragment source name.</summary>
    public Dictionary<string, int> FragmentInstancesByName
    {
        get { EnsureDictionariesInitialized(); return _fragmentInstancesByName; }
    }

    /// <summary>Map-fragment instance counts keyed by tag (one increment per tag on the picked fragment).</summary>
    public Dictionary<string, int> FragmentInstancesByTag
    {
        get { EnsureDictionariesInitialized(); return _fragmentInstancesByTag; }
    }

    /// <summary>
    ///     Fragment instance counts keyed by the <see cref="RoomFeatures" /> bit they were
    ///     picked for. A pick for a room with multiple feature bits increments every one.
    ///     At game scope this feeds the <c>$globalFeatureInstances</c> DSL parameter so
    ///     weight expressions can throttle features that have already appeared on
    ///     previous levels.
    /// </summary>
    public Dictionary<RoomFeatures, int> FragmentInstancesByFeature
    {
        get { EnsureDictionariesInitialized(); return _fragmentInstancesByFeature; }
    }

    /// <summary>Creature instance counts keyed by variant name.</summary>
    public Dictionary<string, int> CreatureInstancesByName
    {
        get { EnsureDictionariesInitialized(); return _creatureInstancesByName; }
    }

    /// <summary>
    ///     Creature instance counts keyed by each <see cref="CreatureGenerationFlags" />
    ///     bit set on the spawned variant.
    /// </summary>
    public Dictionary<CreatureGenerationFlags, int> CreatureInstancesByFlag
    {
        get { EnsureDictionariesInitialized(); return _creatureInstancesByFlag; }
    }

    /// <summary>Item instance counts keyed by variant name. Stacks count once, not per unit.</summary>
    public Dictionary<string, int> ItemInstancesByName
    {
        get { EnsureDictionariesInitialized(); return _itemInstancesByName; }
    }

    /// <summary>
    ///     Item instance counts keyed by <see cref="ItemType" />. Items without an
    ///     explicit type are skipped.
    /// </summary>
    public Dictionary<ItemType, int> ItemInstancesByType
    {
        get { EnsureDictionariesInitialized(); return _itemInstancesByType; }
    }

    /// <summary>
    ///     Room counts keyed by each <see cref="RoomFeatures" /> bit a placed room
    ///     carries (entrance, exit, reward features, curse, etc.). Unlike
    ///     <see cref="FragmentInstancesByFeature" /> which counts fragment picks, this
    ///     counts rooms — one increment per set bit per room. Consumed by the level
    ///     generator when selecting reward rooms so already-placed features are
    ///     down-weighted (see <see cref="GenerationStats" />-aware overloads in
    ///     <c>LevelLayout.BuildFinalGraph</c>).
    /// </summary>
    public Dictionary<RoomFeatures, int> RoomCountsByFeature
    {
        get { EnsureDictionariesInitialized(); return _roomCountsByFeature; }
    }

    // Each Dictionary<,> exposed above is in-memory state; persistence is handled by
    // the sibling ObservableCollection<...Entries> fields below. EF Core discovers
    // each navigation via the matching internal property (e.g. the navigation
    // <c>FragmentInstancesByNameEntries</c> binds to the property of the same name,
    // which simply returns the backing field). The GameDbContext fluent configuration
    // calls <c>UsePropertyAccessMode(Field)</c> on each navigation so EF reads and
    // writes the collection directly through the field rather than through the
    // property getter. SyncEntries() is called from the SaveChanges override on
    // GameDbContext to reconcile the backing collection against the in-memory
    // dictionary just before the change tracker generates SQL;
    // EnsureDictionariesInitialized() goes the other way on first dictionary access
    // after EF materialization. The change tracker observes INotifyCollectionChanged
    // on each ObservableCollection and INotifyPropertyChanged on individual entries.
    internal ObservableCollection<KeyCountPair<string>> FragmentInstancesByNameEntries
        => _fragmentInstancesByNameEntries;

    internal ObservableCollection<KeyCountPair<string>> FragmentInstancesByTagEntries
        => _fragmentInstancesByTagEntries;

    internal ObservableCollection<KeyCountPair<RoomFeatures>> FragmentInstancesByFeatureEntries
        => _fragmentInstancesByFeatureEntries;

    internal ObservableCollection<KeyCountPair<string>> CreatureInstancesByNameEntries
        => _creatureInstancesByNameEntries;

    internal ObservableCollection<KeyCountPair<CreatureGenerationFlags>> CreatureInstancesByFlagEntries
        => _creatureInstancesByFlagEntries;

    internal ObservableCollection<KeyCountPair<string>> ItemInstancesByNameEntries
        => _itemInstancesByNameEntries;

    internal ObservableCollection<KeyCountPair<ItemType>> ItemInstancesByTypeEntries
        => _itemInstancesByTypeEntries;

    internal ObservableCollection<KeyCountPair<RoomFeatures>> RoomCountsByFeatureEntries
        => _roomCountsByFeatureEntries;

    /// <summary>
    ///     Reconciles every <c>*Entries</c> backing collection against its dictionary.
    ///     EF Core's SaveChanges override on <c>GameDbContext</c> calls this on every
    ///     tracked <see cref="GenerationStats" /> so the change tracker sees the
    ///     latest <see cref="System.Collections.Specialized.INotifyCollectionChanged" />
    ///     events before the SQL is generated.
    /// </summary>
    public virtual void SyncEntries()
    {
        Reconcile(_fragmentInstancesByName, _fragmentInstancesByNameEntries);
        Reconcile(_fragmentInstancesByTag, _fragmentInstancesByTagEntries);
        Reconcile(_fragmentInstancesByFeature, _fragmentInstancesByFeatureEntries);
        Reconcile(_creatureInstancesByName, _creatureInstancesByNameEntries);
        Reconcile(_creatureInstancesByFlag, _creatureInstancesByFlagEntries);
        Reconcile(_itemInstancesByName, _itemInstancesByNameEntries);
        Reconcile(_itemInstancesByType, _itemInstancesByTypeEntries);
        Reconcile(_roomCountsByFeature, _roomCountsByFeatureEntries);
    }

    /// <summary>
    ///     Reconciles <paramref name="entries" /> with <paramref name="dict" />: updates
    ///     <c>Count</c> on existing entries whose count has drifted (firing
    ///     <see cref="System.ComponentModel.INotifyPropertyChanged" />), adds entries
    ///     for new keys, and removes entries whose key is no longer in the dictionary
    ///     (both firing <see cref="System.Collections.Specialized.INotifyCollectionChanged" />).
    /// </summary>
    private protected void Reconcile<TKey>(
        Dictionary<TKey, int> dict,
        ObservableCollection<KeyCountPair<TKey>> entries)
        where TKey : notnull
    {
        if (!_dictionariesInitialized)
        {
            return;
        }

        var seen = new HashSet<TKey>(dict.Count);
        for (var i = entries.Count - 1; i >= 0; i--)
        {
            var entry = entries[i];
            if (dict.TryGetValue(entry.Key, out var count))
            {
                if (entry.Count != count)
                {
                    entry.Count = count;
                }
                seen.Add(entry.Key);
            }
            else
            {
                entries.RemoveAt(i);
            }
        }

        foreach (var pair in dict)
        {
            if (!seen.Contains(pair.Key))
            {
                entries.Add(new KeyCountPair<TKey> { Key = pair.Key, Count = pair.Value });
            }
        }
    }

    /// <summary>
    ///     Rebuilds every dictionary from its <c>*Entries</c> backing collection the
    ///     first time anyone accesses a dictionary after EF Core has materialized the
    ///     entity. The reload bypasses <see cref="Reconcile" /> so the freshly loaded
    ///     values aren't pushed back into the already-correct entries.
    /// </summary>
    private protected virtual void EnsureDictionariesInitialized()
    {
        if (_dictionariesInitialized)
        {
            return;
        }
        _dictionariesInitialized = true;

        LoadFromEntries(_fragmentInstancesByName, _fragmentInstancesByNameEntries);
        LoadFromEntries(_fragmentInstancesByTag, _fragmentInstancesByTagEntries);
        LoadFromEntries(_fragmentInstancesByFeature, _fragmentInstancesByFeatureEntries);
        LoadFromEntries(_creatureInstancesByName, _creatureInstancesByNameEntries);
        LoadFromEntries(_creatureInstancesByFlag, _creatureInstancesByFlagEntries);
        LoadFromEntries(_itemInstancesByName, _itemInstancesByNameEntries);
        LoadFromEntries(_itemInstancesByType, _itemInstancesByTypeEntries);
        LoadFromEntries(_roomCountsByFeature, _roomCountsByFeatureEntries);
    }

    private protected static void LoadFromEntries<TKey>(
        Dictionary<TKey, int> dict,
        IEnumerable<KeyCountPair<TKey>> entries)
        where TKey : notnull
    {
        dict.Clear();
        foreach (var entry in entries)
        {
            dict[entry.Key] = entry.Count;
        }
    }

    /// <summary>
    ///     Records a placed map-fragment instance: bumps the per-name counter, the
    ///     per-tag counters for every tag on the fragment, and the per-feature counters
    ///     for every set bit in <paramref name="roomFeatures" />.
    /// </summary>
    public void RecordFragmentInstance(
        string name,
        IEnumerable<string>? tags,
        RoomFeatures roomFeatures)
    {
        Increment(FragmentInstancesByName, name);
        if (tags is not null)
        {
            foreach (var tag in tags)
            {
                Increment(FragmentInstancesByTag, tag);
            }
        }
        foreach (var feature in roomFeatures.GetSetBits())
        {
            Increment(FragmentInstancesByFeature, feature);
        }
    }

    /// <summary>
    ///     Records a spawned creature instance: bumps the per-name counter and the
    ///     per-flag counters for every set bit in <paramref name="flags" />.
    /// </summary>
    public void RecordCreatureInstance(string name, CreatureGenerationFlags flags)
    {
        Increment(CreatureInstancesByName, name);
        foreach (var flag in flags.GetSetBits())
        {
            Increment(CreatureInstancesByFlag, flag);
        }
    }

    /// <summary>
    ///     Records a placed item instance (one event per <c>Instantiate</c> call regardless
    ///     of stack size). Variants without an <see cref="ItemType" /> contribute
    ///     only to the per-name counter.
    /// </summary>
    public void RecordItemInstance(string name, ItemType? type)
    {
        Increment(ItemInstancesByName, name);
        if (type is { } actualType)
        {
            Increment(ItemInstancesByType, actualType);
        }
    }

    /// <summary>
    ///     Records a placed room: bumps the per-feature counter for every set bit in
    ///     <paramref name="features" />. Rooms with <see cref="RoomFeatures.Normal" />
    ///     (no bits) are not recorded.
    /// </summary>
    public void RecordRoomFeature(RoomFeatures features)
    {
        foreach (var feature in features.GetSetBits())
        {
            Increment(RoomCountsByFeature, feature);
        }
    }

    private static void Increment<TKey>(Dictionary<TKey, int> bucket, TKey key) where TKey : notnull
    {
        bucket[key] = bucket.TryGetValue(key, out var count) ? count + 1 : 1;
    }
}
