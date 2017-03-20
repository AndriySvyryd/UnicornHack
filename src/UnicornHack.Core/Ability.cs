using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Ability : IReferenceable
    {
        #region State

        public string Name { get; set; }
        public int Id { get; private set; }
        public int GameId { get; private set; }
        public Game Game { get; set; }
        public virtual AbilityActivation Activation { get; set; }
        public virtual AbilityAction Action { get; set; }
        public virtual int Timeout { get; set; }
        public virtual int TimeoutTurnsLeft { get; set; }
        public virtual EquipmentSlot FreeSlotsRequired { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsUsable { get; set; }
        public virtual int EnergyPointCost { get; set; }
        public virtual int DelayTicks { get; set; }

        // TODO: Whether it can be interrupted
        // TODO: Targeting mode
        // TODO: Success condition
        public virtual int EffectDuration { get; set; }

        public virtual ISet<Effect> Effects { get; set; }

        #endregion

        #region Creation

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
            IsUsable = true;
        }

        public virtual Ability Instantiate(Game game)
        {
            if (Game != null)
            {
                throw new InvalidOperationException("This ability is already part of a game.");
            }

            var abilityInstance = new Ability(game)
            {
                Activation = Activation,
                Action = Action,
                DelayTicks = DelayTicks,
                EnergyPointCost = EnergyPointCost,
                Timeout = Timeout,
                EffectDuration = EffectDuration
            };

            foreach (var effect in Effects)
            {
                abilityInstance.Effects.Add(effect.Instantiate(game).AddReference().Referenced);
            }

            return abilityInstance;
        }

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<Ability> AddReference()
        {
            return new TransientReference<Ability>(this);
        }

        public void RemoveReference()
        {
            if (--_referenceCount <= 0)
            {
                foreach (var effect in Effects)
                {
                    effect.RemoveReference();
                }
                Game.Repository.Delete(this);
            }
        }

        #endregion

        #region Actions

        public virtual bool Activate(AbilityActivationContext abilityContext, bool pretend = false)
        {
            if (!IsUsable)
            {
                return false;
            }

            var activator = abilityContext.Activator;
            var target = abilityContext.Target;
            if (!target.IsAlive)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            var eventOrder = 0;
            var firstAbility = abilityContext.Ability == null;
            if (firstAbility)
            {
                if (Activation == AbilityActivation.OnTarget)
                {
                    if (DelayTicks == 0)
                    {
                        // TODO: Specify the correct delay in the abilities
                        activator.NextActionTick += Actor.DefaultActionDelay;
                    }
                    else
                    {
                        activator.NextActionTick += DelayTicks;
                    }
                }

                abilityContext.Succeeded = Game.Random.Next(maxValue: 3) != 0;
                abilityContext.Ability = new Ability(Game) {Action = Action};
                eventOrder = Game.EventOrder++;
            }

            foreach (var effect in Effects)
            {
                effect.Apply(abilityContext);
            }

            if (firstAbility)
            {
                AttackEvent.New(abilityContext, eventOrder);
                if (!target.IsAlive)
                {
                    activator.XP += target.XP;
                }
            }

            if (Activation == AbilityActivation.OnTarget
                && abilityContext.AbilityTrigger != AbilityActivation.Default)
            {
                foreach (var triggeredAbility in
                    activator.Abilities.Where(a => a.IsUsable && a.Activation == abilityContext.AbilityTrigger))
                {
                    triggeredAbility.Activate(
                        new AbilityActivationContext
                        {
                            Activator = abilityContext.Activator,
                            Target = abilityContext.Target
                        });
                }
            }

            return true;
        }

        #endregion
    }
}