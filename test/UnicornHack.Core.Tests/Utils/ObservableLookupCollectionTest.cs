using UnicornHack.Systems.Effects;

namespace UnicornHack;

public class ObservableLookupCollectionTest
{
    [Fact]
    public void Lookup_tracks_collection_mutations()
    {
        var collection = new TestCollection();
        var first = new TestItem(1, "first");
        var replacement = new TestItem(2, "replacement");

        collection.Add(first);
        Assert.Same(first, collection[1]);
        Assert.True(collection.TryGetValue(1, out var found));
        Assert.Same(first, found);

        collection.ReplaceAt(0, replacement);
        Assert.False(collection.ContainsKey(1));
        Assert.Same(replacement, collection.GetValueOrDefault(2));

        collection.Remove(replacement);
        Assert.False(collection.ContainsKey(2));

        collection.Add(first);
        collection.Clear();
        Assert.Empty(collection.Keys);
        Assert.Empty(collection.Values);
    }

    [Fact]
    public void List_lookup_tracks_duplicate_keys_and_collection_mutations()
    {
        var component = new ActiveEffectsComponent();
        var collection = component.PropertyEffects;
        var first = new ActivePropertyEffect(0, 0, 0, EffectType.Default, "Speed");
        var other = new ActivePropertyEffect(0, 0, 0, EffectType.Default, "Might");
        var second = new ActivePropertyEffect(0, 0, 0, EffectType.Default, "Speed");
        var unkeyed = new ActivePropertyEffect(0, 0, 0, EffectType.Default, null);

        collection.Add(first);
        collection.Add(other);
        collection.Insert(1, second);
        collection.Add(unkeyed);

        Assert.Equal([first, second], collection.GetValues("Speed"));
        Assert.Equal([other], collection.GetValues("Might"));
        Assert.Empty(collection.GetValues("Focus"));
        Assert.True(collection.ContainsKey("Speed"));
        Assert.Same(component, first.ContainingComponent);
        Assert.Equal(nameof(ActiveEffectsComponent.PropertyEffects), first.ContainingPropertyName);

        collection.Move(0, 2);
        Assert.Equal([second, first], collection.GetValues("Speed"));

        var replacement = new ActivePropertyEffect(0, 0, 0, EffectType.Default, "Might");
        collection[0] = replacement;

        Assert.Equal([first], collection.GetValues("Speed"));
        Assert.Equal([replacement, other], collection.GetValues("Might"));
        Assert.Null(second.ContainingComponent);
        Assert.Same(component, replacement.ContainingComponent);

        collection.Remove(first);
        Assert.False(collection.TryGetValues("Speed", out var removedValues));
        Assert.Empty(removedValues);

        collection.Clear();
        Assert.False(collection.TryGetValues("Might", out _));
        Assert.Null(replacement.ContainingComponent);
        Assert.Null(other.ContainingComponent);
        Assert.Null(unkeyed.ContainingComponent);
    }

    [Fact]
    public void Component_effect_collection_is_keyed_by_added_name()
    {
        var component = new ActiveEffectsComponent();
        var first = component.AddComponentEffect(EffectType.Default, "Muted");
        var second = component.AddComponentEffect(EffectType.Default, "Muted");

        Assert.Equal([first, second], component.ComponentEffects.GetValues("Muted"));
    }

    private sealed record TestItem(int Id, string Name);

    private sealed class TestCollection : ObservableLookupCollection<int, TestItem>
    {
        public void ReplaceAt(int index, TestItem item) => SetItem(index, item);

        protected override int GetKey(TestItem item) => item.Id;
    }
}
