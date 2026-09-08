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
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        entity.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        var component = entity.ActiveEffects;
        var collection = component.PropertyEffects;
        var first = CreatePropertyEffect(component, 0, 0, 0, EffectType.Default, "Speed");
        var other = CreatePropertyEffect(component, 0, 0, 0, EffectType.Default, "Might");
        var second = CreatePropertyEffect(component, 0, 0, 0, EffectType.Default, "Speed");
        var unkeyed = CreatePropertyEffect(component, 0, 0, 0, EffectType.Default, null);

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

        var replacement = CreatePropertyEffect(component, 0, 0, 0, EffectType.Default, "Might");
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
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        entity.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        var component = entity.ActiveEffects;
        var first = CreateComponentEffect(component, 0, 0, 0, EffectType.Default, "Muted");
        var second = CreateComponentEffect(component, 0, 0, 1, EffectType.Default, "Muted");
        component.ComponentEffects.Add(first);
        component.ComponentEffects.Add(second);

        Assert.Equal([first, second], component.ComponentEffects.GetValues("Muted"));
    }

    private sealed record TestItem(int Id, string Name);

    private sealed class TestCollection : ObservableLookupCollection<int, TestItem>
    {
        public void ReplaceAt(int index, TestItem item) => SetItem(index, item);

        protected override int GetKey(TestItem item) => item.Id;
    }
}
