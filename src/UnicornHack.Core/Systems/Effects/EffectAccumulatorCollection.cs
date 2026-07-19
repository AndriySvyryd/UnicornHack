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
        item.SetContainingComponent(_component, _propertyName);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, EffectAccumulator item)
    {
        this[index].SetContainingComponent(null, null);
        item.SetContainingComponent(_component, _propertyName);
        base.SetItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        this[index].SetContainingComponent(null, null);
        base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            item.SetContainingComponent(null, null);
        }

        base.ClearItems();
    }

    protected override EffectType GetKey(EffectAccumulator item) => item.EffectType;
}
