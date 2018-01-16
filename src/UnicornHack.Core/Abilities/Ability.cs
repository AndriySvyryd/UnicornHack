using System;
using System.Collections.Generic;
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

        public int? AddedAbilityEffectId { get; set; }

        public virtual AbilityActivation Activation { get; set; }
        public virtual TargetingType? TargetingType { get; set; }
        public virtual TargetingDirection? TargetingDirection { get; set; }
        public virtual AbilityAction Action { get; set; }

        public virtual int Timeout { get; set; }
        public virtual int EnergyPointCost { get; set; }
        public virtual int Delay { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsUsable { get; set; }
        public virtual int TimeoutLeft { get; set; }

        // TODO: Whether it can be interrupted
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
            var target = abilityContext.TargetEntity as Actor;
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

        public virtual IReadOnlyCollection<Entity> GetTargets(AbilityActivationContext abilityContext)
        {
            if (abilityContext.TargetEntity != null)
            {
                return new[] {abilityContext.TargetEntity};
            }

            var level = abilityContext.Activator.Level;
            var targetCell = abilityContext.TargetCell.Value;
            var activatorCell = new Point(abilityContext.Activator.LevelX, abilityContext.Activator.LevelY);
            var vectorToTarget = activatorCell.DifferenceTo(targetCell);
            switch (TargetingType)
            {
                case Abilities.TargetingType.AdjacentSingle:
                    if (vectorToTarget.Length() > 1)
                    {
                        return new Entity[0];
                    }

                    return level.Actors.Where(a => a.LevelX == targetCell.X && a.LevelY == targetCell.Y).ToList();
                case Abilities.TargetingType.AdjacentArc:
                    if (vectorToTarget.Length() > 1)
                    {
                        return new Entity[0];
                    }

                    var direction = vectorToTarget.AsDirection();
                    var firstNeighbour = targetCell.Translate(Vector.Convert(direction.Rotate(1)));
                    var secondNeighbour = targetCell.Translate(Vector.Convert(direction.Rotate(-1)));

                    return level.Actors.Where(a => a.LevelX == targetCell.X && a.LevelY == targetCell.Y
                                                   || a.LevelX == firstNeighbour.X && a.LevelY == firstNeighbour.Y
                                                   || a.LevelX == secondNeighbour.X && a.LevelY == secondNeighbour.Y)
                        .ToList();
                case Abilities.TargetingType.Beam:
                    // TODO: take blocking actors and terrain into account
                    // TODO: set abilityContext.TargetCell to the final projectile position
                    return level.Actors.Where(a => a.LevelX == targetCell.X && a.LevelY == targetCell.Y).ToList();
                default:
                    throw new ArgumentOutOfRangeException("TargetingType " + TargetingType + " not supported");
            }
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