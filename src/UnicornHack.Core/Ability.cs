using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Ability : IReferenceable
    {
        public string Name { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }

        public virtual AbilityActivation Activation { get; set; }
        // TODO: Move/refactor this
        public virtual AbilityAction Action { get; set; }
        public virtual int Timeout { get; set; }
        public virtual int EnergyPointCost { get; set; }
        public virtual int Delay { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsUsable { get; set; }
        public virtual int TimeoutLeft { get; set; }

        // TODO: Whether it can be interrupted
        // TODO: Targeting mode
        // TODO: Success condition
        // TODO: Activation condition

        public virtual ISet<Effect> Effects { get; set; } = new HashSet<Effect>();
        public ICollection<AppliedEffect> ActiveEffects { get; set; } = new HashSet<AppliedEffect>();

        public int EntityId { get; set; }
        public Entity Entity { get; set; }

        public Ability()
        {
        }

        public Ability(Game game) : this()
        {
            Game = game;
            Id = game.NextAbilityId++;
            game.Abilities.Add(this);
            IsUsable = true;
        }

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<Ability> AddReference() => new TransientReference<Ability>(this);

        public void RemoveReference()
        {
            if (--_referenceCount <= 0)
            {
                Game.Repository.Delete(this);
                foreach (var effect in Effects)
                {
                    effect.Delete();
                }
                Effects.Clear();
            }
        }

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

            if (Activation == AbilityActivation.Always
                || Activation == AbilityActivation.WhileToggled
                || Activation == AbilityActivation.WhilePossessed
                || Activation == AbilityActivation.WhileEquipped)
            {
                Debug.Assert(!IsActive);
                IsActive = true;
            }

            if (EnergyPointCost > 0)
            {
                abilityContext.Activator.ChangeCurrentEP(-1 * EnergyPointCost);
            }

            var eventOrder = 0;
            var firstAbility = abilityContext.Ability == null;
            if (firstAbility)
            {
                abilityContext.Ability = this;
                // TODO: Also apply delay for secondary attacks
                if (Activation == AbilityActivation.OnTarget)
                {
                    if (Delay == 0)
                    {
                        // TODO: Specify the correct delay in the abilities
                        activator.NextActionTick += Actor.DefaultActionDelay;
                    }
                    else
                    {
                        activator.NextActionTick += Delay;
                    }
                }

                abilityContext.Succeeded = !abilityContext.IsAttack || Game.Random.Next(maxValue: 3) != 0;
                abilityContext.AbilityAction = Action;
                eventOrder = Game.EventOrder++;
            }

            foreach (var effect in Effects)
            {
                effect.Apply(abilityContext);
            }

            if (eventOrder != 0 && abilityContext.IsAttack)
            {
                // TODO: Move to Actor
                AttackEvent.New(abilityContext, eventOrder);
            }

            if (Activation == AbilityActivation.OnTarget && abilityContext.AbilityTrigger != AbilityActivation.Default)
            {
                foreach (var triggeredAbility in activator.Abilities.Where(
                    a => a.IsUsable && a.Activation == abilityContext.AbilityTrigger))
                {
                    triggeredAbility.Activate(new AbilityActivationContext
                    {
                        Activator = abilityContext.Activator,
                        Target = abilityContext.Target,
                        IsAttack = abilityContext.IsAttack
                    });
                }
            }

            return true;
        }

        public void Deactivate()
        {
            Debug.Assert(Activation == AbilityActivation.Always
                         || Activation == AbilityActivation.WhileToggled
                         || Activation == AbilityActivation.WhileEquipped
                         || Activation == AbilityActivation.WhilePossessed);

            Debug.Assert(IsActive);
            IsActive = false;

            foreach (var appliedEffect in ActiveEffects.ToList())
            {
                appliedEffect.Remove();
            }
        }
    }
}