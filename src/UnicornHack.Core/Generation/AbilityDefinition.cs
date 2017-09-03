using System.Collections.Generic;
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
        public virtual AbilityActivation Activation { get; set; }
        public virtual AbilityAction Action { get; set; }
        public virtual int Timeout { get; set; }
        public virtual int Delay { get; set; }

        // TODO: Use a placeholder item
        public virtual EquipmentSlot FreeSlotsRequired { get; set; }

        public virtual int EnergyPointCost { get; set; }

        // TODO: Whether it can be interrupted
        // TODO: Targeting mode
        // TODO: Success condition
        // TODO: Activation condition

        public virtual ISet<Effect> Effects { get; set; } = new HashSet<Effect>();

        public AbilityDefinition()
        {
        }

        public AbilityDefinition(Game game) : this()
        {
            Game = game;
            Id = game.NextAbilityDefinitionId++;
            game.AbilityDefinitions.Add(this);
        }

        public virtual AbilityDefinition Copy(Game game)
        {
            var abilityInstance = new AbilityDefinition(game)
            {
                Name = Name,
                Activation = Activation,
                Action = Action,
                Delay = Delay,
                Timeout = Timeout,
                EnergyPointCost = EnergyPointCost,
                FreeSlotsRequired = FreeSlotsRequired
            };

            foreach (var effect in Effects)
            {
                abilityInstance.Effects.Add(effect.Copy(game));
            }

            return abilityInstance;
        }

        public virtual Ability Instantiate(Game game)
        {
            var abilityInstance = new Ability(game)
            {
                Name = Name,
                Activation = Activation,
                Action = Action,
                Delay = Delay,
                Timeout = Timeout,
                EnergyPointCost = EnergyPointCost
            };

            foreach (var effect in Effects)
            {
                abilityInstance.Effects.Add(effect.Copy(game));
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