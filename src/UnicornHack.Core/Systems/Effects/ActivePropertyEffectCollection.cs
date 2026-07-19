namespace UnicornHack.Systems.Effects;

public class ActivePropertyEffectCollection
    : ObservableListLookupCollection<string, ActivePropertyEffect>
{
    public ActivePropertyEffectCollection(ActiveEffectsComponent component, string propertyName)
        : base(component, propertyName)
    {
    }

    protected override string? GetKey(ActivePropertyEffect item) => item.TargetName;
}