using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Abilities
{
    [Component(Id = (int)EntityComponent.Ability)]
    public class AbilityComponent : GameComponent
    {
        private string _name;
        private int _level;
        private GameEntity _ownerEntity;
        private int? _ownerId;
        private ActivationType _activation;
        private int? _activationCondition;
        private ActivationType _trigger;
        private int _headingDeviation;
        private int _range;
        private TargetingShape _targetingShape;
        private TargetingType _targetingType;
        private AbilityAction _action;
        private AbilitySuccessCondition _successCondition;
        private string _accuracy;
        private int _cooldown;
        private int _xpCooldown;
        private int? _cooldownTick;
        private int? _cooldownXpLeft;
        private int _energyCost;
        private string _delay;
        private bool _isActive;
        private bool _isUsable = true;
        private int? _slot;
        private Ability _template;
        private bool _templateLoaded;

        public AbilityComponent()
            => ComponentId = (int)EntityComponent.Ability;

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public int Level
        {
            get => _level;
            set => SetWithNotify(value, ref _level);
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

        public AbilityAction Action
        {
            get => _action;
            set => SetWithNotify(value, ref _action);
        }

        public int? ActivationCondition
        {
            get => _activationCondition;
            set => SetWithNotify(value, ref _activationCondition);
        }

        public ActivationType Trigger
        {
            get => _trigger;
            set => SetWithNotify(value, ref _trigger);
        }

        /// <summary>
        ///     Max number of octants between heading and direction to target
        /// </summary>
        public int HeadingDeviation
        {
            get => _headingDeviation;
            set => SetWithNotify(value, ref _headingDeviation);
        }

        public int Range
        {
            get => _range;
            set => SetWithNotify(value, ref _range);
        }

        public TargetingShape TargetingShape
        {
            get => _targetingShape;
            set => SetWithNotify(value, ref _targetingShape);
        }

        public TargetingType TargetingType
        {
            get => _targetingType;
            set => SetWithNotify(value, ref _targetingType);
        }

        public AbilitySuccessCondition SuccessCondition
        {
            get => _successCondition;
            set => SetWithNotify(value, ref _successCondition);
        }

        public string Accuracy
        {
            get => _accuracy;
            set => SetWithNotify(value, ref _accuracy);
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

        public int EnergyCost
        {
            get => _energyCost;
            set => SetWithNotify(value, ref _energyCost);
        }

        public string Delay
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

        public Ability Template
        {
            get
            {
                if (_templateLoaded
                    || _name == null)
                {
                    return _template;
                }

                _template = Ability.Loader.Find(_name);
                _templateLoaded = true;
                return _template;
            }

            set
            {
                _template = value;
                _templateLoaded = true;
            }
        }

        public AbilityComponent AddToEffect(GameEntity abilityEffectEntity, bool includeEffects = true)
        {
            var manager = abilityEffectEntity.Manager;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = Name;
            ability.Level = Level;
            ability.Activation = Activation;
            ability.ActivationCondition = ActivationCondition;
            ability.Trigger = Trigger;
            ability.HeadingDeviation = HeadingDeviation;
            ability.Range = Range;
            ability.TargetingShape = TargetingShape;
            ability.TargetingType = TargetingType;
            ability.Action = Action;
            ability.SuccessCondition = SuccessCondition;
            ability.Cooldown = Cooldown;
            ability.Accuracy = Accuracy;
            ability.XPCooldown = XPCooldown;
            ability.Delay = Delay;
            ability.EnergyCost = EnergyCost;

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
            _level = default;
            _ownerId = default;
            _ownerEntity = default;
            _activation = default;
            _activationCondition = default;
            _trigger = default;
            _headingDeviation = default;
            _range = default;
            _targetingShape = default;
            _targetingType = default;
            _action = default;
            _successCondition = default;
            _accuracy = default;
            _cooldown = default;
            _xpCooldown = default;
            _cooldownTick = default;
            _cooldownXpLeft = default;
            _energyCost = default;
            _delay = default;
            _isActive = default;
            _isUsable = true;
            _slot = default;

            base.Clean();
        }
    }
}
