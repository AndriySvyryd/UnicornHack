namespace UnicornHack.Systems.Effects;

public class EffectAccumulatorCollection : ObservableLookupCollection<EffectType, EffectAccumulator>
{
    private readonly ActiveEffectsComponent _component;
    private readonly string _propertyName;

    public EffectAccumulatorCollection(ActiveEffectsComponent component, string propertyName)
    {
        _component = component;
        _propertyName = propertyName;
    }

    protected override void InsertItem(int index, EffectAccumulator item)
    {
        Attach(item);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, EffectAccumulator item)
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
        for (var i = Count - 1; i >= 0; i--)
        {
            var item = this[i];
            item.SetContainingComponent(null, null);
            Detach(item);
        }

        base.ClearItems();
    }

    protected override EffectType GetKey(EffectAccumulator item) => item.EffectType;

    private void Attach(EffectAccumulator item)
    {
        item.Attach(_component);
        item.SetContainingComponent(_component, _propertyName);
    }

    private void Detach(EffectAccumulator item)
        => item.Detach(_component.Manager as GameManager
            ?? throw new InvalidOperationException("Active effects require a game manager."));
}
