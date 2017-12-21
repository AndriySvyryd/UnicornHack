using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Data.Properties;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Utils;

namespace UnicornHack.Abilities
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

        public virtual ObservableSnapshotHashSet<Trigger> Triggers { get; set; } =
            new ObservableSnapshotHashSet<Trigger>();

        public virtual ObservableSnapshotHashSet<Effect> Effects { get; set; } =
            new ObservableSnapshotHashSet<Effect>();

        public ObservableSnapshotHashSet<AppliedEffect> ActiveEffects { get; set; } =
            new ObservableSnapshotHashSet<AppliedEffect>();

        public int? EntityId { get; set; }
        public Entity Entity { get; set; }

        public Ability()
        {
        }

        public Ability(Game game) : this()
        {
            Game = game;
            Id = ++game.NextAbilityId;
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
                foreach (var trigger in Triggers)
                {
                    trigger.Delete();
                }
                Triggers.Clear();
            }
        }

        public virtual bool Activate(AbilityActivationContext abilityContext, bool pretend = false)
        {
            if (!IsUsable)
            {
                return false;
            }

            var activator = abilityContext.Activator;
            var target = abilityContext.Target as Actor;
            if (target?.IsAlive == false)
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

            if (IsExplicitlyActivated())
            {
                if (EnergyPointCost > 0)
                {
                    if (activator.GetProperty<int>(PropertyData.EnergyPoints.Name) < EnergyPointCost)
                    {
                        return false;
                    }
                    ((Actor)abilityContext.Activator).ChangeCurrentEP(-1 * EnergyPointCost);
                }

                if (activator is Actor actor)
                {
                    if (Delay == 0)
                    {
                        // TODO: Specify the correct delay in the abilities
                        actor.NextActionTick += Actor.DefaultActionDelay;
                    }
                    else
                    {
                        actor.NextActionTick += Delay;
                    }
                }
            }

            Entity?.OnAbilityActivating(this);

            var isFinal = IsFinal();
            if (!isFinal)
            {
                abilityContext.EffectsToApply = abilityContext.EffectsToApply != null
                    ? abilityContext.EffectsToApply.AddRange(Effects)
                    : Effects.ToImmutableList();
            }

            var eventOrder = 0;
            if (IsEventful())
            {
                Debug.Assert(abilityContext.AbilityAction == AbilityAction.Default);

                abilityContext.AbilityAction = Action;
                abilityContext.Succeeded = Game.Random.Next(maxValue: 3) != 0;
                eventOrder = Game.EventOrder++;
            }

            foreach (var trigger in Triggers)
            {
                trigger.Fire(abilityContext);
            }

            if (isFinal)
            {
                Debug.Assert(abilityContext.Ability == null);

                abilityContext.Ability = this;

                foreach (var effect in Effects)
                {
                    effect.Apply(abilityContext);
                }

                if (abilityContext.EffectsToApply != null)
                {
                    foreach (var effect in abilityContext.EffectsToApply)
                    {
                        effect.Apply(abilityContext);
                    }

                    abilityContext.EffectsToApply = null;
                }
            }

            if (eventOrder != 0)
            {
                AttackEvent.New(abilityContext, eventOrder);
            }

            Entity?.OnAbilityActivated(this);
            return true;
        }

        private bool IsFinal() => Action != AbilityAction.Modifier
                                  && Action != AbilityAction.Shoot
                                  && Action != AbilityAction.Throw;

        private bool IsEventful() => Action != AbilityAction.Default
                                     && Action != AbilityAction.Modifier;

        private bool IsExplicitlyActivated() => Activation == AbilityActivation.OnActivation
                                             || Activation == AbilityActivation.OnTarget;

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