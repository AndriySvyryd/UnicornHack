using UnicornHack.Primitives;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects
{
    public class EffectsAppliedMessage : IMessage
    {
        private GameEntity _activatorEntity;
        private GameEntity _abilityEntity;
        private GameEntity _targetEntity;
        private ReferencingList<GameEntity> _appliedEffects;

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
            set
            {
                _appliedEffects?.Clear();
                _appliedEffects = value;
            }
        }

        public ActivationType AbilityType { get; set; }
        public bool SuccessfulApplication { get; set; }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ActivatorEntity = null;
            AbilityEntity = null;
            TargetEntity = null;
            AppliedEffects = null;
        }
    }
}
