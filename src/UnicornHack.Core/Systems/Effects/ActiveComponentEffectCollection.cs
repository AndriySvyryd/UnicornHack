namespace UnicornHack.Systems.Effects;

public class ActiveComponentEffectCollection
    : ObservableListLookupCollection<string, ActiveComponentEffect>
{
    public ActiveComponentEffectCollection(ActiveEffectsComponent component, string propertyName)
        : base(component, propertyName)
    {
    }

    protected override string? GetKey(ActiveComponentEffect item) => item.TargetName;
}