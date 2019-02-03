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
        public string Name { get; set; }
        public AbilityType Type { get; set; }
        public int Cost { get; set; }
        public ActivationType Activation { get; set; }
        public int? ActivationCondition { get; set; }
        public ActivationType ItemCondition { get; set; }
        public ActivationType Trigger { get; set; }
        public int HeadingDeviation { get; set; }
        public int Range { get; set; }
        public TargetingShape TargetingShape { get; set; }
        public TargetingType TargetingType { get; set; }
        public AbilityAction Action { get; set; }
        public AbilitySuccessCondition SuccessCondition { get; set; }

        /// <summary>
        ///     Amount of ticks that need to pass after the ability has been used or deactivated before it can be used again.
        /// </summary>
        public int Cooldown { get; set; }

        /// <summary>
        ///     Amount of experience points as percent of XP required for the next level that a player need to gain after the
        ///     ability has been used or deactivated before it can be used again.
        /// </summary>
        public int XPCooldown { get; set; }

        public int Delay { get; set; }
        public int EnergyPointCost { get; set; }
        public ISet<Effect> Effects { get; set; }

        public virtual AbilityComponent AddToEffect(GameEntity effectEntity, int level = 0)
        {
            var manager = effectEntity.Manager;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = Name;
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
            ability.XPCooldown = XPCooldown;
            ability.Delay = Delay;
            ability.EnergyPointCost = EnergyPointCost;
            ability.Template = this;

            effectEntity.Ability = ability;
            AddEffects(Effects, ability, manager);

            return ability;
        }

        protected void AddEffects(ISet<Effect> effects, AbilityComponent ability, GameManager manager)
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
            {nameof(Type), (o, v) => (AbilityType)v != default},
            {nameof(Cost), (o, v) => (int)v != default},
            {nameof(Activation), (o, v) => (ActivationType)v != default},
            {nameof(ActivationCondition), (o, v) => v != default},
            {nameof(ItemCondition), (o, v) => (ActivationType)v != default},
            {nameof(Trigger), (o, v) => (ActivationType)v != default},
            {nameof(HeadingDeviation), (o, v) => (int)v != default},
            {nameof(Range), (o, v) => (int)v != default},
            {nameof(TargetingShape), (o, v) => (TargetingShape)v != default},
            {nameof(TargetingType), (o, v) => (TargetingType)v != default},
            {nameof(Action), (o, v) => (AbilityAction)v != default},
            {nameof(SuccessCondition), (o, v) => (AbilitySuccessCondition)v != default}
        };

        public virtual ICSScriptSerializer GetSerializer() => Serializer;
    }
}
