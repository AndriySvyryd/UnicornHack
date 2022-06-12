using UnicornHack.Primitives;

namespace UnicornHack.Generation
{
    public class Affectable
    {
        protected void AddPropertyEffect(
            string propertyName,
            int? value,
            int abilityId,
            GameManager manager,
            ValueCombinationFunction? function = null)
            => manager.EffectApplicationSystem.AddPropertyEffect(propertyName, value, abilityId, manager, function);
    }
}
