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

        public AbilityActivation Activation { get; set; }
        public TargetingType? TargetingType { get; set; }
        public TargetingDirection? TargetingDirection { get; set; }
        public AbilityAction Action { get; set; }
        public AbilitySuccessCondition? SuccessCondition { get; set; }

        public int Timeout { get; set; }
        public int EnergyPointCost { get; set; }
        public int Delay { get; set; }
        public bool IsActive { get; set; }
        public bool IsUsable { get; set; }
        public int TimeoutLeft { get; set; }

        // TODO: Whether it can be interrupted
        // TODO: Activation condition

        public ObservableSnapshotHashSet<Trigger> Triggers { get; set; } =
            new ObservableSnapshotHashSet<Trigger>();

        public ObservableSnapshotHashSet<Effect> Effects { get; set; } =
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

                DetermineSuccess(abilityContext);

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

        public virtual IReadOnlyCollection<Entity> GetTargets(AbilityActivationContext context)
        {
            if (context.TargetEntity != null)
            {
                return new[] {context.TargetEntity};
            }

            var level = context.Activator.Level;
            var targetCell = context.TargetCell.Value;
            var vectorToTarget = context.Activator.LevelCell.DifferenceTo(targetCell);
            switch (TargetingType)
            {
                case Abilities.TargetingType.AdjacentSingle:
                    if (vectorToTarget.Length() > 1)
                    {
                        return new Entity[0];
                    }

                    return level.Actors.Where(a => a.LevelCell == targetCell).ToList();
                case Abilities.TargetingType.AdjacentArc:
                    if (vectorToTarget.Length() > 1)
                    {
                        return new Entity[0];
                    }

                    var direction = vectorToTarget.AsDirection();
                    var firstNeighbour = targetCell.Translate(Vector.Convert(direction.Rotate(1)));
                    var secondNeighbour = targetCell.Translate(Vector.Convert(direction.Rotate(-1)));

                    return level.Actors.Where(a => a.LevelCell == targetCell
                                                   || a.LevelCell == firstNeighbour
                                                   || a.LevelCell == secondNeighbour)
                        .ToList();
                case Abilities.TargetingType.Beam:
                    // TODO: set abilityContext.TargetCell to the final projectile position
                    // TODO: check LOS
                    return level.Actors.Where(a => a.LevelCell == targetCell).ToList();
                default:
                    throw new ArgumentOutOfRangeException("TargetingType " + TargetingType + " not supported");
            }
        }

        public void DetermineSuccess(AbilityActivationContext context)
        {
            var successCondition = SuccessCondition;
            if (successCondition == null)
            {
                if (Activation != AbilityActivation.OnMeleeAttack
                    && Activation != AbilityActivation.OnRangedAttack
                    && Activation != AbilityActivation.OnTarget)
                {
                    successCondition = AbilitySuccessCondition.Always;
                }
                else
                {
                    switch (Action)
                    {
                        case AbilityAction.Default:
                        case AbilityAction.Modifier:
                            successCondition = AbilitySuccessCondition.Always;
                            break;
                        case AbilityAction.Hit:
                        case AbilityAction.Slash:
                        case AbilityAction.Chop:
                        case AbilityAction.Stab:
                        case AbilityAction.Poke:
                        case AbilityAction.Impale:
                        case AbilityAction.Bludgeon:
                        case AbilityAction.Punch:
                        case AbilityAction.Kick:
                        case AbilityAction.Touch:
                        case AbilityAction.Headbutt:
                        case AbilityAction.Claw:
                        case AbilityAction.Bite:
                        case AbilityAction.Suck:
                        case AbilityAction.Sting:
                        case AbilityAction.Hug:
                        case AbilityAction.Trample:
                        case AbilityAction.Digestion:
                        case AbilityAction.Shoot:
                        case AbilityAction.Throw:
                        case AbilityAction.Spit:
                            successCondition = AbilitySuccessCondition.PhysicalAttack;
                            break;
                        case AbilityAction.Breath:
                        case AbilityAction.Gaze:
                        case AbilityAction.Scream:
                        case AbilityAction.Spell:
                        case AbilityAction.Explosion:
                            successCondition = AbilitySuccessCondition.NonPhysicalAttack;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            // TODO: Allow hit even if attacker doesn't see the victim, but there's LOS
            var success = true;
            switch (successCondition)
            {
                case AbilitySuccessCondition.Always:
                    break;
                case AbilitySuccessCondition.PhysicalAttack:
                    success = context.TargetEntity != null
                              && ((Actor)context.Activator).GetVisibility(context.TargetEntity.LevelCell) > 0
                              && Game.Random.Next(
                                  context.TargetEntity.GetProperty<int>(PropertyData.PhysicalDeflection.Name) + 20)
                              < 15;
                    break;
                case AbilitySuccessCondition.NonPhysicalAttack:
                    success = context.TargetEntity != null
                              && ((Actor)context.Activator).GetVisibility(context.TargetEntity.LevelCell) > 0
                              && Game.Random.Next(
                                  context.TargetEntity.GetProperty<int>(PropertyData.MagicDeflection.Name) + 20)
                              < 15;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            context.Succeeded = success;
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