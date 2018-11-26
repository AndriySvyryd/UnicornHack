using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class SetAbilitySlotMessage : IMessage
    {
        private GameEntity _abilityEntity;

        public GameEntity AbilityEntity
        {
            get => _abilityEntity;
            set
            {
                _abilityEntity?.RemoveReference(this);
                _abilityEntity = value;
                _abilityEntity?.AddReference(this);
            }
        }

        public int? Slot { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            AbilityEntity = default;
            Slot = default;
        }
    }
}
