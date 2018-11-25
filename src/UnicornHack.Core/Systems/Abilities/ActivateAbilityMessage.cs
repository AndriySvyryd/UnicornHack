using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class ActivateAbilityMessage : IMessage
    {
        private GameEntity _activatorEntity;
        private GameEntity _abilityEntity;
        private GameEntity _targetEntity;

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

        public GameEntity TargetEntity
        {
            get => _targetEntity;
            set
            {
                _targetEntity?.RemoveReference(this);
                _targetEntity = value;
                _targetEntity?.AddReference(this);
            }
        }

        public Point? TargetCell { get; set; }

        public ActivateAbilityMessage Clone(ActivateAbilityMessage message)
        {
            var manager = message.ActivatorEntity.Manager;

            var abilityMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
            abilityMessage.AbilityEntity = message.AbilityEntity;
            abilityMessage.ActivatorEntity = message.ActivatorEntity;
            abilityMessage.TargetEntity = message.TargetEntity;
            abilityMessage.TargetCell = message.TargetCell;

            return abilityMessage;
        }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ActivatorEntity = default;
            AbilityEntity = default;
            TargetEntity = default;
            TargetCell = default;
        }
    }
}
