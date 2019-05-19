using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class DeactivateAbilityMessage : IMessage
    {
        public const string Name = "DeactivateAbility";

        public static DeactivateAbilityMessage Create(GameManager manager)
            => manager.Queue.CreateMessage<DeactivateAbilityMessage>(Name);

        private GameEntity _activatorEntity;
        private GameEntity _abilityEntity;

        public GameEntity ActivatorEntity
        {
            get => _activatorEntity;
            set
            {
                _activatorEntity?.RemoveReference(this);
                _activatorEntity = value;
                _activatorEntity?.AddReference(this);
            }
        }

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

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ActivatorEntity = default;
            AbilityEntity = default;
        }
    }
}
