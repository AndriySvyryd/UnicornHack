namespace UnicornHack.Systems.Effects;

public abstract class PooledTrackableObservableLookupCollection<TEffect>
    : ObservableLookupComponentCollection<string, TEffect>
    where TEffect : PooledTrackableCollectionItem
{
    private readonly ActiveEffectsComponent _component;

    protected PooledTrackableObservableLookupCollection(ActiveEffectsComponent component, string propertyName)
        : base(component, propertyName)
        => _component = component;

    protected override void InsertItem(int index, TEffect item)
    {
        Attach(item);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, TEffect item)
    {
        var replaced = this[index];
        if (ReferenceEquals(replaced, item))
        {
            base.SetItem(index, item);
            return;
        }

        Attach(item);
        Detach(replaced);
        base.SetItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        var removed = this[index];
        Detach(removed);
        base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
        if (Count == 0)
        {
            return;
        }

        for (var i = Count - 1; i >= 0; i--)
        {
            Detach(this[i]);
        }

        base.ClearItems();
    }

    private void Attach(TEffect item)
        => item.Attach(_component);

    private void Detach(TEffect item)
        => item.Detach(_component.Manager as GameManager
            ?? throw new InvalidOperationException("Active effects require a game manager."));
}
