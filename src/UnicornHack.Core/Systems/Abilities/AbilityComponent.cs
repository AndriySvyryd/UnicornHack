using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Abilities
{
    [Component(Id = (int)EntityComponent.Ability)]
    public class AbilityComponent : GameComponent
    {
        private string _name;
        private int? _ownerId;
        private ActivationType _activation;
        private TargetingType _targetingType;
        private TargetingAngle _targetingAngle;
        private AbilityAction _action;
        private ActivationType _trigger;
        private AbilitySuccessCondition _successCondition;
        private int _timeout;
        private int? _timeoutTick;
        private int _energyPointCost;
        private int _delay;
        private bool _isActive;
        private bool _isUsable = true;

        public AbilityComponent()
            => ComponentId = (int)EntityComponent.Ability;

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public int? OwnerId
        {
            get => _ownerId;
            set => SetWithNotify(value, ref _ownerId);
        }

        public ActivationType Activation
        {
            get => _activation;
            set => SetWithNotify(value, ref _activation);
        }

        public TargetingType TargetingType
        {
            get => _targetingType;
            set => SetWithNotify(value, ref _targetingType);
        }

        public TargetingAngle TargetingAngle
        {
            get => _targetingAngle;
            set => SetWithNotify(value, ref _targetingAngle);
        }

        public AbilityAction Action
        {
            get => _action;
            set => SetWithNotify(value, ref _action);
        }

        public ActivationType Trigger
        {
            get => _trigger;
            set => SetWithNotify(value, ref _trigger);
        }

        public AbilitySuccessCondition SuccessCondition
        {
            get => _successCondition;
            set => SetWithNotify(value, ref _successCondition);
        }

        public int Timeout
        {
            get => _timeout;
            set => SetWithNotify(value, ref _timeout);
        }

        public int? TimeoutTick
        {
            get => _timeoutTick;
            set => SetWithNotify(value, ref _timeoutTick);
        }

        public int EnergyPointCost
        {
            get => _energyPointCost;
            set => SetWithNotify(value, ref _energyPointCost);
        }

        public int Delay
        {
            get => _delay;
            set => SetWithNotify(value, ref _delay);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetWithNotify(value, ref _isActive);
        }

        public bool IsUsable
        {
            get => _isUsable;
            set => SetWithNotify(value, ref _isUsable);
        }

        // TODO: Whether it can be interrupted
        // TODO: Activation condition
        // TODO: Success condition

        public void AddToAppliedEffect(GameEntity appliedEffectEntity, int affectedEntityId)
        {
            var manager = appliedEffectEntity.Manager;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = Name;
            ability.Activation = Activation;
            ability.TargetingType = TargetingType;
            ability.TargetingAngle = TargetingAngle;
            ability.Action = Action;
            ability.Delay = Delay;
            ability.Timeout = Timeout;
            ability.EnergyPointCost = EnergyPointCost;

            appliedEffectEntity.Ability = ability;

            foreach (var effectEntity in manager.EffectsToContainingAbilityRelationship[Entity.Id])
            {
                effectEntity.Effect.AddToAbility(appliedEffectEntity);
            }

            ability.OwnerId = affectedEntityId;
        }

        protected override void Clean()
        {
            _name = default;
            _ownerId = default;
            _activation = default;
            _targetingType = default;
            _targetingAngle = default;
            _action = default;
            _trigger = default;
            _successCondition = default;
            _timeout = default;
            _timeoutTick = default;
            _energyPointCost = default;
            _delay = default;
            _isActive = default;
            _isUsable = true;

            base.Clean();
        }
    }
}
