namespace UnicornHack.Systems.Effects;

public class ActivePropertyEffectCollection
    : PooledTrackableObservableLookupCollection<ActivePropertyEffect>
{
    public ActivePropertyEffectCollection(ActiveEffectsComponent component, string propertyName)
        : base(component, propertyName)
    {
    }

    protected override string? GetKey(ActivePropertyEffect item) => item.TargetName;
}