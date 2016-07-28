using UnicornHack.Models.GameDefinitions;
using UnicornHack.Models.GameState;
using UnicornHack.Models.GameState.Events;

namespace UnicornHack.Services
{
    public interface ILanguageService
    {
        string ToString(AttackEvent @event);
        string ToString(ItemDropEvent @event);
        string ToString(ItemPickUpEvent @event);
        string ToString(DeathEvent @event);
        string ToString(ItemConsumptionEvent @event);
        string Welcome(PlayerCharacter character);
        string UnableToMove(Direction direction);
    }
}