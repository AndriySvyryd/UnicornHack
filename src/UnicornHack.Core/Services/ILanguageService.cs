using UnicornHack.Primitives;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Items;

namespace UnicornHack.Services
{
    public interface ILanguageService
    {
        string GetPropertyString(string propertyName, bool abbreviate);
        string GetString(ItemComponent item, int quantity, SenseType sense);
        string GetString(AbilityComponent ability);
        string GetString(EquipmentSlot slot, GameEntity actorEntity, bool abbreviate);
        string GetString(in ItemPickUpEvent @event);
        string GetString(in ItemDropEvent @event);
        string GetString(in ItemEquipmentEvent @event);
        string GetString(in ItemActivationEvent @event);
        string GetString(in AttackEvent @event);
        string GetString(in DeathEvent @event);
        string Welcome(GameEntity playerEntity);
        string UnableToMove(Direction direction);
    }
}
