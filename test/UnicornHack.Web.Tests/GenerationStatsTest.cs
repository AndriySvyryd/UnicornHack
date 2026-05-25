using System.Reflection;
using UnicornHack.Generation;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack;

/// <summary>
///     Coverage for the <see cref="GenerationStats" /> persistence contract. Live EF Core
///     round-tripping is exercised end-to-end by the diagnostic client smoke; these tests
///     pin down the in-memory invariants the EF Core mapping relies on:
///     <list type="number">
///         <item>calling <see cref="GenerationStats.SyncEntries" /> reconciles the backing
///         <c>*Entries</c> <see cref="System.Collections.ObjectModel.ObservableCollection{T}" />s with
///         the current dictionary state (firing INotifyCollectionChanged / INotifyPropertyChanged events);</item>
///         <item>hydrating the entries list and then reading a dictionary rebuilds it lazily and silently;</item>
///         <item>after hydration, mutations remain visible to subsequent <see cref="GenerationStats.SyncEntries" /> calls.</item>
///     </list>
/// </summary>
public class GenerationStatsTest
{
    [Fact]
    public void Recording_a_fragment_instance_pushes_into_every_backing_list()
    {
        var stats = new GenerationStats();

        stats.RecordFragmentInstance(
            "dungeon room",
            tags: ["normal", "rectangular"],
            roomFeatures: RoomFeatures.DeityAltar | RoomFeatures.MajorShrine);
        stats.RecordFragmentInstance(
            "dungeon room",
            tags: ["normal"],
            roomFeatures: RoomFeatures.Normal);

        Assert.Equal(2, stats.FragmentInstancesByName["dungeon room"]);
        Assert.Equal(2, stats.FragmentInstancesByTag["normal"]);
        Assert.Equal(1, stats.FragmentInstancesByTag["rectangular"]);
        Assert.Equal(1, stats.FragmentInstancesByFeature[RoomFeatures.DeityAltar]);
        Assert.Equal(1, stats.FragmentInstancesByFeature[RoomFeatures.MajorShrine]);

        stats.SyncEntries();

        var nameEntries = GetEntries<KeyCountPair<string>>(stats, "_fragmentInstancesByNameEntries");
        var tagEntries = GetEntries<KeyCountPair<string>>(stats, "_fragmentInstancesByTagEntries");
        var featureEntries = GetEntries<KeyCountPair<RoomFeatures>>(stats, "_fragmentInstancesByFeatureEntries");

        Assert.Equal(stats.FragmentInstancesByName.Count, nameEntries.Count);
        Assert.Equal(stats.FragmentInstancesByTag.Count, tagEntries.Count);
        Assert.Equal(stats.FragmentInstancesByFeature.Count, featureEntries.Count);

        Assert.Equal(2, nameEntries.Single(e => e.Key == "dungeon room").Count);
        Assert.Equal(2, tagEntries.Single(e => e.Key == "normal").Count);
        Assert.Equal(1, featureEntries.Single(e => e.Key == RoomFeatures.DeityAltar).Count);
    }

    [Fact]
    public void Recording_a_creature_instance_records_each_set_flag()
    {
        var stats = new GenerationStats();

        stats.RecordCreatureInstance("goblin", CreatureGenerationFlags.SmallGroup);
        stats.RecordCreatureInstance("goblin", CreatureGenerationFlags.SmallGroup | CreatureGenerationFlags.LargeGroup);

        Assert.Equal(2, stats.CreatureInstancesByName["goblin"]);
        Assert.Equal(2, stats.CreatureInstancesByFlag[CreatureGenerationFlags.SmallGroup]);
        Assert.Equal(1, stats.CreatureInstancesByFlag[CreatureGenerationFlags.LargeGroup]);

        stats.SyncEntries();

        var flagEntries = GetEntries<KeyCountPair<CreatureGenerationFlags>>(stats, "_creatureInstancesByFlagEntries");
        Assert.Equal(2, flagEntries.Single(e => e.Key == CreatureGenerationFlags.SmallGroup).Count);
        Assert.Equal(1, flagEntries.Single(e => e.Key == CreatureGenerationFlags.LargeGroup).Count);
    }

    [Fact]
    public void Recording_a_room_feature_pushes_into_the_backing_list()
    {
        var stats = new GenerationStats();

        stats.RecordRoomFeature(RoomFeatures.Entrance);
        stats.RecordRoomFeature(RoomFeatures.SmallFountain | RoomFeatures.Cursed);
        stats.RecordRoomFeature(RoomFeatures.SmallFountain);
        stats.RecordRoomFeature(RoomFeatures.Normal); // no-op

        Assert.Equal(1, stats.RoomCountsByFeature[RoomFeatures.Entrance]);
        Assert.Equal(2, stats.RoomCountsByFeature[RoomFeatures.SmallFountain]);
        Assert.Equal(1, stats.RoomCountsByFeature[RoomFeatures.Cursed]);

        stats.SyncEntries();

        var entries = GetEntries<KeyCountPair<RoomFeatures>>(stats, "_roomCountsByFeatureEntries");
        Assert.Equal(stats.RoomCountsByFeature.Count, entries.Count);
        Assert.Equal(2, entries.Single(e => e.Key == RoomFeatures.SmallFountain).Count);
        Assert.Equal(1, entries.Single(e => e.Key == RoomFeatures.Cursed).Count);
    }

    [Fact]
    public void Recording_an_item_with_null_type_only_increments_the_name_bucket()
    {
        var stats = new GenerationStats();

        stats.RecordItemInstance("short sword", ItemType.WeaponMeleeShort);
        stats.RecordItemInstance("mystery thing", type: null);

        Assert.Equal(1, stats.ItemInstancesByName["short sword"]);
        Assert.Equal(1, stats.ItemInstancesByName["mystery thing"]);
        Assert.Single(stats.ItemInstancesByType);
        Assert.Equal(1, stats.ItemInstancesByType[ItemType.WeaponMeleeShort]);

        stats.SyncEntries();

        var typeEntries = GetEntries<KeyCountPair<ItemType>>(stats, "_itemInstancesByTypeEntries");
        Assert.Single(typeEntries);
        Assert.Equal(ItemType.WeaponMeleeShort, typeEntries[0].Key);
    }

    [Fact]
    public void Dictionary_clear_clears_the_backing_list()
    {
        var stats = new GenerationStats();
        stats.RecordCreatureInstance("goblin", CreatureGenerationFlags.SmallGroup);

        stats.SyncEntries();
        var nameEntries = GetEntries<KeyCountPair<string>>(stats, "_creatureInstancesByNameEntries");
        Assert.Single(nameEntries);

        stats.CreatureInstancesByName.Clear();
        stats.SyncEntries();

        Assert.Empty(stats.CreatureInstancesByName);
        Assert.Empty(nameEntries);
    }

    [Fact]
    public void Dictionary_remove_drops_only_the_matching_entry()
    {
        var stats = new GenerationStats();
        stats.RecordFragmentInstance("a", tags: null, roomFeatures: RoomFeatures.Normal);
        stats.RecordFragmentInstance("b", tags: null, roomFeatures: RoomFeatures.Normal);
        stats.RecordFragmentInstance("c", tags: null, roomFeatures: RoomFeatures.Normal);

        Assert.True(stats.FragmentInstancesByName.Remove("b"));

        stats.SyncEntries();

        var nameEntries = GetEntries<KeyCountPair<string>>(stats, "_fragmentInstancesByNameEntries");
        Assert.Equal(2, nameEntries.Count);
        Assert.DoesNotContain(nameEntries, e => e.Key == "b");
    }

    [Fact]
    public void Hydrating_entries_then_reading_a_dictionary_lazily_loads_it()
    {
        var stats = new GenerationStats();
        var nameEntries = GetEntries<KeyCountPair<string>>(stats, "_creatureInstancesByNameEntries");
        nameEntries.Add(new KeyCountPair<string> { Key = "kobold", Count = 4 });
        nameEntries.Add(new KeyCountPair<string> { Key = "orc", Count = 11 });

        // First read should trigger EnsureDictionariesInitialized() and snapshot the entries
        // into the dictionary without re-pushing them back into the list.
        Assert.Equal(4, stats.CreatureInstancesByName["kobold"]);
        Assert.Equal(11, stats.CreatureInstancesByName["orc"]);

        // Mirror invariant: same number of entries, identical instances (no replacements).
        Assert.Equal(2, nameEntries.Count);

        // After hydration, further mutations keep the mirror in sync once SyncEntries is invoked.
        stats.RecordCreatureInstance("kobold", CreatureGenerationFlags.None);
        stats.SyncEntries();

        Assert.Equal(5, stats.CreatureInstancesByName["kobold"]);
        Assert.Equal(5, nameEntries.Single(e => e.Key == "kobold").Count);
    }

    private static System.Collections.ObjectModel.ObservableCollection<T> GetEntries<T>(GenerationStats stats, string fieldName)
    {
        var field = typeof(GenerationStats).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        return (System.Collections.ObjectModel.ObservableCollection<T>)field!.GetValue(stats)!;
    }
}
