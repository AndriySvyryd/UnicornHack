using System;
using System.Collections.Generic;
using UnicornHack.Effects;

namespace UnicornHack
{
    public class Ability
    {
        public Ability()
        {
            Effects = new HashSet<Effect>();
        }

        public Ability(Game game)
            : this()
        {
            Game = game;
            Id = game.NextAbilityId++;
            game.Abilities.Add(this);
        }

        public virtual Ability Instantiate(Game game)
        {
            if (Game != null)
            {
                throw new InvalidOperationException("This ability is part of a game.");
            }

            var abilityInstance = new Ability(game)
            {
                Activation = Activation,
                Action = Action,
                ActionPointCost = ActionPointCost,
                EnergyPointCost = EnergyPointCost,
                Timeout = Timeout,
                EffectDuration = EffectDuration
            };
            foreach (var effect in Effects)
            {
                abilityInstance.Effects.Add(effect.Instantiate(game));
            }

            return abilityInstance;
        }

        public string Name { get; set; }
        public int Id { get; private set; }
        public int GameId { get; private set; }
        public Game Game { get; set; }
        public virtual AbilityActivation Activation { get; set; }
        public virtual AbilityAction Action { get; set; }
        public virtual int Timeout { get; set; }
        public virtual int TimeoutTurnsLeft { get; set; }
        public virtual bool IsActive { get; set; }
        // If more than one turn - can be interrupted
        public virtual int ActionPointCost { get; set; }
        public virtual int EnergyPointCost { get; set; }
        // Targeting mode
        // Success condition
        public virtual int EffectDuration { get; set; }
        public virtual ISet<Effect> Effects { get; set; }
    }
}