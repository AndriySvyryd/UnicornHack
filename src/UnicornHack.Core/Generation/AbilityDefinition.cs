using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;

namespace UnicornHack.Generation
{
    public class AbilityDefinition
    {
        public string Name { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }
        public int? AddAbilityEffectId { get; set; }
        public virtual AbilityActivation Activation { get; set; }
        public virtual TargetingType? TargetingType { get; set; }
        public virtual TargetingDirection? TargetingDirection { get; set; }
        public virtual AbilityAction Action { get; set; }
        public virtual int Timeout { get; set; }
        public virtual int Delay { get; set; }

        // TODO: Use a placeholder item
        public virtual EquipmentSlot FreeSlotsRequired { get; set; }

        public virtual int EnergyPointCost { get; set; }

        // TODO: Whether it can be interrupted
        // TODO: Success condition

        public virtual ISet<Trigger> Triggers { get; set; }
        public virtual ISet<Effect> Effects { get; set; }

        public AbilityDefinition()
        {
        }

        public AbilityDefinition(Game game) : this()
        {
            Game = game;
            Id = ++game.NextAbilityDefinitionId;
            game.AbilityDefinitions.Add(this);
            Triggers = new HashSet<Trigger>();
            Effects = new HashSet<Effect>();
        }

        public virtual AbilityDefinition Copy(Game game)
        {
            var abilityCopy = new AbilityDefinition(game)
            {
                Name = Name,
                Activation = Activation,
                TargetingType = TargetingType,
                TargetingDirection = TargetingDirection,
                Action = Action,
                Delay = Delay,
                Timeout = Timeout,
                EnergyPointCost = EnergyPointCost,
                FreeSlotsRequired = FreeSlotsRequired
            };

            if (Triggers != null)
            {
                foreach (var trigger in Triggers)
                {
                    abilityCopy.Triggers.Add(trigger.Copy(game));
                }
            }

            if (Effects != null)
            {
                foreach (var effect in Effects)
                {
                    abilityCopy.Effects.Add(effect.Copy(game));
                }
            }

            return abilityCopy;
        }

        public virtual Ability Instantiate(Game game)
        {
            var abilityInstance = new Ability(game)
            {
                Name = Name,
                Activation = Activation,
                TargetingType = TargetingType,
                TargetingDirection = TargetingDirection,
                Action = Action,
                Delay = Delay,
                Timeout = Timeout,
                EnergyPointCost = EnergyPointCost
            };

            if (Triggers != null)
            {
                foreach (var trigger in Triggers)
                {
                    abilityInstance.Triggers.Add(trigger.Copy(game));
                }
            }

            if (Effects != null)
            {
                foreach (var effect in Effects)
                {
                    abilityInstance.Effects.Add(effect.Copy(game));
                }
            }

            return abilityInstance;
        }

        public virtual void Delete()
        {
            Game.Repository.Delete(this);
            foreach (var effect in Effects)
            {
                effect.Delete();
            }

            Effects.Clear();
        }
    }
}