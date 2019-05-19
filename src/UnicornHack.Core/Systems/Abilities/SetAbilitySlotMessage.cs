using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class SetAbilitySlotMessage : IMessage
    {
        public const string Name = "SetAbilitySlot";

        public static SetAbilitySlotMessage Create(GameManager manager)
            => manager.Queue.CreateMessage<SetAbilitySlotMessage>(Name);

        private GameEntity _abilityEntity;
        private GameEntity _ownerEntity;

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

        public GameEntity OwnerEntity
        {
            get => _ownerEntity;
            set
            {
                _ownerEntity?.RemoveReference(this);
                _ownerEntity = value;
                _ownerEntity?.AddReference(this);
            }
        }

        public int? Slot { get; set; }

        public string AbilityName { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            AbilityEntity = default;
            AbilityName = default;
            Slot = default;
        }
    }
}
