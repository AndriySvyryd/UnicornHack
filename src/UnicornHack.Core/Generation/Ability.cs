using System.Collections.Generic;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;

namespace UnicornHack.Generation
{
    public class Ability
    {
        public string Name { get; set; }
        public ActivationType Activation { get; set; }
        public int? ActivationCondition { get; set; }
        public ActivationType UsabilityCondition { get; set; }
        public ActivationType Trigger { get; set; }
        public TargetingType TargetingType { get; set; }
        public TargetingAngle TargetingAngle { get; set; }
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

        public AbilityComponent AddToEffect(GameEntity effectEntity)
        {
            var manager = effectEntity.Manager;
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

            effectEntity.Ability = ability;

            if (Effects != null)
            {
                foreach (var effect in Effects)
                {
                    effect.AddToAbility(ability, manager);
                }
            }

            return ability;
        }
    }
}
