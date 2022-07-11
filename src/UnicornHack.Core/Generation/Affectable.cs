namespace UnicornHack.Generation;

public class Affectable
{
    protected void AddPropertyEffect(
        string propertyName,
        int? value,
        int abilityId,
        GameManager manager,
        ValueCombinationFunction? function = null)
    {
        if (value == null)
        {
            return;
        }
        
        manager.EffectApplicationSystem.AddPropertyEffect(propertyName, value.Value, abilityId, manager, function);
    }
}
