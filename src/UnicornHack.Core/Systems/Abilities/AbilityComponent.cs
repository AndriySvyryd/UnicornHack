using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Abilities
{
    [Component(Id = (int)EntityComponent.Ability)]
    public class AbilityComponent : GameComponent
    {
        private string _name;
        private GameEntity _ownerEntity;
        private int? _ownerId;
        private ActivationType _activation;
        private int? _activationCondition;
        private ActivationType _trigger;
        private TargetingType _targetingType;
        private TargetingAngle _targetingAngle;
        private AbilityAction _action;
        private AbilitySuccessCondition _successCondition;
        private int _cooldown;
        private int _xpCooldown;
        private int? _cooldownTick;
        private int? _cooldownXpLeft;
        private int _energyPointCost;
        private int _delay;
        private bool _isActive;
        private bool _isUsable = true;
        private int? _slot;

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
            set
            {
                _ownerEntity = null;
                SetWithNotify(value, ref _ownerId);
            }
        }

        public ActivationType Activation
        {
            get => _activation;
            set => SetWithNotify(value, ref _activation);
        }

        public int? ActivationCondition
        {
            get => _activationCondition;
            set => SetWithNotify(value, ref _activationCondition);
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

        public int Cooldown
        {
            get => _cooldown;
            set => SetWithNotify(value, ref _cooldown);
        }

        public int XPCooldown
        {
            get => _xpCooldown;
            set => SetWithNotify(value, ref _xpCooldown);
        }

        public int? CooldownTick
        {
            get => _cooldownTick;
            set => SetWithNotify(value, ref _cooldownTick);
        }

        public int? CooldownXpLeft
        {
            get => _cooldownXpLeft;
            set => SetWithNotify(value, ref _cooldownXpLeft);
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

        public int? Slot
        {
            get => _slot;
            set => SetWithNotify(value, ref _slot);
        }

        // TODO: Whether it can be interrupted
        // TODO: Activation condition
        // TODO: Success condition

        // Unmapped properties
        public GameEntity OwnerEntity
        {
            get => _ownerEntity ?? (_ownerEntity = Entity.Manager.FindEntity(_ownerId));
            set
            {
                OwnerId = value?.Id;
                _ownerEntity = value;
            }
        }

        public AbilityComponent AddToEffect(GameEntity abilityEffectEntity, bool includeEffects = true)
        {
            var manager = abilityEffectEntity.Manager;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = Name;
            ability.Activation = Activation;
            ability.ActivationCondition = ActivationCondition;
            ability.Trigger = Trigger;
            ability.TargetingType = TargetingType;
            ability.TargetingAngle = TargetingAngle;
            ability.Action = Action;
            ability.SuccessCondition = SuccessCondition;
            ability.Cooldown = Cooldown;
            ability.XPCooldown = XPCooldown;
            ability.Delay = Delay;
            ability.EnergyPointCost = EnergyPointCost;

            abilityEffectEntity.Ability = ability;

            if (includeEffects)
            {
                foreach (var effectEntity in manager.EffectsToContainingAbilityRelationship[Entity.Id])
                {
                    effectEntity.Effect.AddToAbility(abilityEffectEntity);
                }
            }

            return ability;
        }

        protected override void Clean()
        {
            _name = default;
            _ownerId = default;
            _ownerEntity = default;
            _activation = default;
            _targetingType = default;
            _targetingAngle = default;
            _action = default;
            _trigger = default;
            _successCondition = default;
            _cooldown = default;
            _xpCooldown = default;
            _cooldownTick = default;
            _cooldownXpLeft = default;
            _energyPointCost = default;
            _delay = default;
            _isActive = default;
            _isUsable = true;
            _slot = default;

            base.Clean();
        }
    }
}
