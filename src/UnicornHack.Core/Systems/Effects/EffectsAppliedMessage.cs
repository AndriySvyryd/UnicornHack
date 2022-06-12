using UnicornHack.Primitives;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects
{
    public class EffectsAppliedMessage : IMessage
    {
        public const string Name = "EffectsApplied";

        public static EffectsAppliedMessage Create(GameManager manager)
            => manager.Queue.CreateMessage<EffectsAppliedMessage>(Name);

        private GameEntity _activatorEntity;
        private GameEntity _abilityEntity;
        private GameEntity _targetEntity;
        private ReferencingList<GameEntity> _appliedEffects = new();

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

        public ReferencingList<GameEntity> AppliedEffects
        {
            get => _appliedEffects;
        }

        public ActivationType AbilityTrigger { get; set; }
        public bool SuccessfulApplication { get; set; }

        string IMessage.MessageName { get; set; }

        public void Clean()
        {
            ActivatorEntity = default;
            AbilityEntity = default;
            TargetEntity = default;
            _appliedEffects.Clear();
            AbilityTrigger = default;
            SuccessfulApplication = default;
        }
    }
}
