using UnicornHack.Primitives;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;

namespace UnicornHack.Services
{
    public interface ILanguageService
    {
        string GetPropertyName(string propertyName, bool abbreviate);
        string GetActorName(GameEntity actorEntity, SenseType sense);
        string GetDescription(string id, DescriptionCategory category);
        string GetString(ItemComponent item, int quantity, SenseType sense);
        string GetString(RaceComponent race, bool abbreviate);
        string GetString(AbilityComponent ability);
        string GetString(Direction direction, bool abbreviate);
        string GetString(EquipmentSlot slot, GameEntity actorEntity, bool abbreviate);
        string GetString(in ItemPickUpEvent @event);
        string GetString(in ItemDropEvent @event);
        string GetString(in ItemEquipmentEvent @event);
        string GetString(in ItemActivationEvent @event);
        string GetString(in AttackEvent @event);
        string GetString(in DeathEvent @event);
        string GetString(in LeveledUpEvent @event);
        string NoDefaultAttack(bool melee);
        string Welcome(GameEntity playerEntity);
        string UnableToMove(Direction direction);
        string NoPath();
    }
}
