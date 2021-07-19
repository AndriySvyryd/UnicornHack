using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Data.Abilities;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation
{
    public class Ability : ILoadable, ICSScriptSerializable
    {
        private Func<GameEntity, GameEntity, float> _accuracyFunction;
        private Func<GameEntity, GameEntity, float> _delayFunction;
        private IReadOnlyList<Effect> _effects;

        public string Name { get; set; }
        public string EnglishDescription { get; set; }
        public AbilityType Type { get; set; }
        public int Cost { get; set; }
        public ActivationType Activation { get; set; }
        public int? ActivationCondition { get; set; }
        public ActivationType ItemCondition { get; set; }
        public ActivationType Trigger { get; set; }
        public int MinHeadingDeviation { get; set; }
        public int MaxHeadingDeviation { get; set; }
        public int Range { get; set; }
        public int TargetingShapeSize { get; set; } = 1;
        public TargetingShape TargetingShape { get; set; }
        public AbilityAction Action { get; set; }
        public AbilitySuccessCondition SuccessCondition { get; set; }
        public string Accuracy { get; set; }

        /// <summary>
        ///     Amount of ticks that need to pass after the ability has been used or deactivated before it can be used again.
        /// </summary>
        public int Cooldown { get; set; }

        /// <summary>
        ///     Amount of experience points adjusted by level depth that a player need to gain after the
        ///     ability has been used or deactivated before it can be used again.
        /// </summary>
        public int XPCooldown { get; set; }

        public string Delay { get; set; }
        public int EnergyPointCost { get; set; }

        public IReadOnlyList<Effect> Effects
        {
            get => _effects;
            set
            {
                _effects = value;
                foreach (var effect in value)
                {
                    effect.ContainingAbility = this;
                }
            }
        }

        public virtual AbilityComponent AddToEffect(GameEntity effectEntity, int level = 0)
        {
            var manager = effectEntity.Manager;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = Name;
            ability.Type = Type;
            ability.Activation = Activation;
            ability.ActivationCondition = ActivationCondition;
            ability.Trigger = Trigger;
            ability.MinHeadingDeviation = MinHeadingDeviation;
            ability.MaxHeadingDeviation = MaxHeadingDeviation;
            ability.Range = Range;
            ability.TargetingShapeSize = TargetingShapeSize;
            ability.TargetingShape = TargetingShape;
            ability.Action = Action;
            ability.SuccessCondition = SuccessCondition;
            ability.Accuracy = Accuracy;
            ability.Cooldown = Cooldown;
            ability.XPCooldown = XPCooldown;
            ability.Delay = Delay;
            ability.EnergyCost = EnergyPointCost;
            ability.Template = this;
            ability.Level = level;

            if (Accuracy != null)
            {
                if (_accuracyFunction == null)
                {
                    _accuracyFunction = AbilityActivationSystem.CreateAccuracyFunction(Accuracy, Name);
                }
                ability.AccuracyFunction = _accuracyFunction;
            }

            if (Delay != null)
            {
                if (_delayFunction == null)
                {
                    _delayFunction = AbilityActivationSystem.CreateDelayFunction(Delay, Name);
                }
                ability.DelayFunction = _delayFunction;
            }

            effectEntity.Ability = ability;
            AddEffects(Effects, ability, manager);

            return ability;
        }

        protected void AddEffects(IReadOnlyList<Effect> effects, AbilityComponent ability, GameManager manager)
        {
            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    effect.AddToAbility(ability, manager);
                }
            }
        }

        public AbilityComponent AddToAffectable(GameEntity affectableEntity, int level = 0)
        {
            var manager = affectableEntity.Manager;
            using (var abilityEntityReference = manager.CreateEntity())
            {
                var ability = AddToEffect(abilityEntityReference.Referenced, level);
                ability.OwnerId = affectableEntity.Id;

                return ability;
            }
        }

        public static readonly GroupedCSScriptLoader<AbilityType, Ability> Loader =
            new GroupedCSScriptLoader<AbilityType, Ability>(@"Data\Abilities\", c => c.Type,
                typeof(AbilityData));

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Ability>(GetPropertyConditions<Ability>());

        protected static Dictionary<string, Func<TAbility, object, bool>> GetPropertyConditions<TAbility>()
            where TAbility : Ability => new Dictionary<string, Func<TAbility, object, bool>>
        {
            {nameof(Name), (o, v) => v != default},
            {nameof(EnglishDescription), (o, v) => v != default},
            {nameof(Type), (o, v) => (AbilityType)v != default},
            {nameof(Cost), (o, v) => (int)v != default},
            {nameof(Activation), (o, v) => (ActivationType)v != default},
            {nameof(ActivationCondition), (o, v) => v != default},
            {nameof(ItemCondition), (o, v) => (ActivationType)v != default},
            {nameof(Trigger), (o, v) => (ActivationType)v != default},
            {nameof(MinHeadingDeviation), (o, v) => (int)v != default},
            {nameof(MaxHeadingDeviation), (o, v) => (int)v != default},
            {nameof(Range), (o, v) => (int)v != default},
            {nameof(TargetingShapeSize), (o, v) => (int)v != 1},
            {nameof(TargetingShape), (o, v) => (TargetingShape)v != default},
            {nameof(Action), (o, v) => (AbilityAction)v != default},
            {nameof(SuccessCondition), (o, v) => (AbilitySuccessCondition)v != default},
            {nameof(Accuracy), (o, v) => v != default},
            {nameof(Cooldown), (o, v) => (int)v != default},
            {nameof(XPCooldown), (o, v) => (int)v != default},
            {nameof(Delay), (o, v) => (string)v != default},
            {nameof(EnergyPointCost), (o, v) => (int)v != default},
            {nameof(Effects), (o, v) => (((IReadOnlyList<Effect>)v)?.Count ?? 0) != 0}
        };

        public virtual ICSScriptSerializer GetSerializer() => Serializer;
    }
}
