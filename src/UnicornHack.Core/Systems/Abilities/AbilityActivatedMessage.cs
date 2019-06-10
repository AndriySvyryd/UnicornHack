using System.Collections.Immutable;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class AbilityActivatedMessage : IMessage
    {
        public const string Name = "AbilityActivated";

        public static AbilityActivatedMessage Create(GameManager manager)
            => manager.Queue.CreateMessage<AbilityActivatedMessage>(Name);

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
        public ActivationType Trigger { get; set; }
        public ImmutableList<GameEntity> EffectsToApply { get; set; }
        public string ActivationError { get; set; }
        public ApplicationOutcome Outcome { get; set; }

        public AbilityActivatedMessage Clone(GameManager manager)
        {
            var abilityActivatedMessage = manager.CreateMessage<AbilityActivatedMessage>(
                ((IMessage)this).MessageName);
            abilityActivatedMessage.ActivatorEntity = ActivatorEntity;
            abilityActivatedMessage.AbilityEntity = AbilityEntity;
            abilityActivatedMessage.TargetEntity = TargetEntity;
            abilityActivatedMessage.TargetCell = TargetCell;
            abilityActivatedMessage.Trigger = Trigger;
            abilityActivatedMessage.EffectsToApply = EffectsToApply;
            abilityActivatedMessage.ActivationError = ActivationError;
            return abilityActivatedMessage;
        }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ActivatorEntity = default;
            AbilityEntity = default;
            TargetEntity = default;
            TargetCell = default;
            Trigger = default;
            EffectsToApply = default;
            ActivationError = default;
            Outcome = default;
        }
    }
}
