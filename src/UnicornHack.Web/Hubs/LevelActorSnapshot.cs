using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Primitives;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs
{
    public class LevelActorSnapshot
    {
        private bool CurrentlyPerceived { get; set; }

        public LevelActorSnapshot CaptureState(GameEntity knowledgeEntity, SerializationContext context)
        {
            var actorKnowledge = knowledgeEntity.Knowledge;
            var knownEntity = actorKnowledge.KnownEntity;
            var position = knowledgeEntity.Position;
            var manager = context.Manager;
            CurrentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();

            return this;
        }

        public static List<object> Serialize(
            GameEntity knowledgeEntity, EntityState? state, LevelActorSnapshot snapshot, SerializationContext context)
        {
            List<object> properties;
            var actorKnowledge = knowledgeEntity.Knowledge;
            var knownEntity = actorKnowledge?.KnownEntity;
            var ai = knownEntity?.AI;
            var position = knowledgeEntity.Position;
            var being = knownEntity?.Being;
            var manager = context.Manager;
            switch (state)
            {
                case null:
                case EntityState.Added:
                    properties = state == null
                        ? new List<object>(6)
                        : new List<object>(7) { (int)state };
                    properties.Add(actorKnowledge.EntityId);

                    if (actorKnowledge.SensedType.CanIdentify())
                    {
                        properties.Add(ai != null
                            ? manager.RacesToBeingRelationship[actorKnowledge.KnownEntityId].Values.First().Race
                                .TemplateName
                            : "player");
                        properties.Add(context.Services.Language.GetActorName(knownEntity, actorKnowledge.SensedType));
                    }
                    else
                    {
                        properties.Add(null);
                        properties.Add(null);
                    }

                    properties.Add(position.LevelX);
                    properties.Add(position.LevelY);
                    properties.Add((byte)position.Heading);

                    if (actorKnowledge.SensedType.CanIdentify()
                        && ai != null)
                    {
                        var currentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();
                        if (snapshot != null)
                        {
                            snapshot.CurrentlyPerceived = currentlyPerceived;
                        }
                        properties.Add(currentlyPerceived);
                        properties.Add(being.HitPoints);
                        properties.Add(being.HitPointMaximum);
                        properties.Add(being.EnergyPoints);
                        properties.Add(being.EnergyPointMaximum);
                        properties.Add(ai.NextActionTick);

                        int? _ = null;
                        var meleeAttack =
                            manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: true, manager);
                        SerializeAttackSummary(
                            meleeAttack,
                            knownEntity,
                            context.Observer, properties,
                            index: ref _,
                            manager: manager);

                        var rangedAttack =
                            manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: false, manager);
                        SerializeAttackSummary(
                            rangedAttack,
                            knownEntity,
                            context.Observer, properties,
                            index: ref _,
                            manager: manager);

                        meleeAttack = manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: true, manager);
                        SerializeAttackSummary(
                            meleeAttack,
                            context.Observer,
                            knownEntity,
                            properties,
                            index: ref _,
                            manager: manager);

                        rangedAttack = manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: false, manager);
                        SerializeAttackSummary(
                            rangedAttack,
                            context.Observer,
                            knownEntity,
                            properties,
                            index: ref _,
                            manager: manager);

                        return properties;
                    }

                    return properties;
                case EntityState.Deleted:
                    return new List<object>
                    {
                        (int)state,
                        knowledgeEntity.Id
                    };
                default:
                    properties = new List<object>(2)
                    {
                        (int)state,
                        actorKnowledge.EntityId
                    };

                    var knowledgeEntry = context.DbContext.Entry(actorKnowledge);
                    int? i = 1;
                    var sensedType = knowledgeEntry.Property(nameof(KnowledgeComponent.SensedType));
                    if (sensedType.IsModified)
                    {
                        var canIdentify = actorKnowledge.SensedType.CanIdentify();
                        properties.Add(i);
                        properties.Add(!canIdentify
                            ? null
                            : ai != null
                                ? manager.RacesToBeingRelationship[actorKnowledge.KnownEntityId].Values.First().Race
                                    .TemplateName
                                : "player");

                        i++;
                        properties.Add(i);
                        properties.Add(!canIdentify
                            ? null
                            : context.Services.Language.GetActorName(knownEntity, actorKnowledge.SensedType));
                    }
                    else
                    {
                        i++;
                    }

                    var positionEntry = context.DbContext.Entry(position);
                    if (positionEntry.State != EntityState.Unchanged)
                    {
                        i++;
                        var levelX = positionEntry.Property(nameof(PositionComponent.LevelX));
                        if (levelX.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(position.LevelX);
                        }

                        i++;
                        var levelY = positionEntry.Property(nameof(PositionComponent.LevelY));
                        if (levelY.IsModified)
                        {
                            properties.Add(i);
                            properties.Add(position.LevelY);
                        }

                        i++;
                        var heading = positionEntry.Property(nameof(PositionComponent.Heading));
                        if (heading.IsModified)
                        {
                            properties.Add(i);
                            properties.Add((byte)position.Heading);
                        }
                    }
                    else
                    {
                        i += 3;
                    }

                    if (ai != null)
                    {
                        i++;
                        var currentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();
                        if (snapshot.CurrentlyPerceived != currentlyPerceived)
                        {
                            properties.Add(i);
                            properties.Add(currentlyPerceived);
                            snapshot.CurrentlyPerceived = currentlyPerceived;
                        }

                        if (currentlyPerceived)
                        {
                            var beingEntry = context.DbContext.Entry(being);
                            if (beingEntry.State != EntityState.Unchanged)
                            {
                                i++;
                                var hitPoints = beingEntry.Property(nameof(BeingComponent.HitPoints));
                                if (hitPoints.IsModified)
                                {
                                    properties.Add(i);
                                    properties.Add(being.HitPoints);
                                }

                                i++;
                                var hitPointMaximum = beingEntry.Property(nameof(BeingComponent.HitPointMaximum));
                                if (hitPointMaximum.IsModified)
                                {
                                    properties.Add(i);
                                    properties.Add(being.HitPointMaximum);
                                }

                                i++;
                                var energyPoints = beingEntry.Property(nameof(BeingComponent.EnergyPoints));
                                if (energyPoints.IsModified)
                                {
                                    properties.Add(i);
                                    properties.Add(being.EnergyPoints);
                                }

                                i++;
                                var energyPointMaximum = beingEntry.Property(nameof(BeingComponent.EnergyPointMaximum));
                                if (energyPointMaximum.IsModified)
                                {
                                    properties.Add(i);
                                    properties.Add(being.EnergyPointMaximum);
                                }

                            }
                            else
                            {
                                i += 4;
                            }

                            i++;
                            var aiEntry = context.DbContext.Entry(ai);
                            if (aiEntry.State != EntityState.Unchanged)
                            {
                                var nextActionTick = aiEntry.Property(nameof(AIComponent.NextActionTick));
                                if (nextActionTick.IsModified)
                                {
                                    properties.Add(i);
                                    properties.Add(ai.NextActionTick);
                                }
                            }
                        }

                        if (sensedType.IsModified)
                        {
                            var canIdentify = actorKnowledge.SensedType.CanIdentify();

                            // TODO: Snapshot the attacks and update them while the actor is perceived
                            var meleeAttack = canIdentify
                                ? manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: true, manager)
                                : null;
                            SerializeAttackSummary(
                                meleeAttack,
                                knownEntity,
                                context.Observer,
                                properties,
                                index: ref i, manager: manager);

                            var rangedAttack = canIdentify
                                ? manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: false, manager)
                                : null;
                            SerializeAttackSummary(
                                rangedAttack,
                                knownEntity,
                                context.Observer,
                                properties,
                                index: ref i,
                                manager: manager);

                            meleeAttack = canIdentify
                                ? manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: true, manager)
                                : null;
                            SerializeAttackSummary(
                                meleeAttack,
                                context.Observer,
                                knownEntity, properties,
                                index: ref i,
                                manager: manager);

                            rangedAttack = canIdentify
                                ? manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: false, manager)
                                : null;
                            SerializeAttackSummary(
                                rangedAttack,
                                context.Observer,
                                knownEntity,
                                properties,
                                index: ref i,
                                manager: manager);
                        }
                    }

                    return properties.Count > 2 ? properties : null;
            }
        }

        private static void SerializeAttackSummary(
            GameEntity attackEntity,
            GameEntity attackerEntity,
            GameEntity victimEntity,
            List<object> result,
            ref int? index,
            GameManager manager)
        {
            AttackStats stats = null;
            if (attackEntity != null)
            {
                var activateMessage = ActivateAbilityMessage.Create(manager);
                activateMessage.AbilityEntity = attackEntity;
                activateMessage.ActivatorEntity = attackerEntity;
                activateMessage.TargetEntity = victimEntity;

                stats = manager.AbilityActivationSystem.GetAttackStats(activateMessage);
                manager.ReturnMessage(activateMessage);
            }

            if (stats == null)
            {
                if (!index.HasValue)
                {
                    result.Add(null);
                    result.Add(null);
                    result.Add(null);
                    result.Add(null);
                }
                else
                {
                    index = index.Value + 1;
                    result.Add(index.Value);
                    result.Add(null);
                    index = index.Value + 3;
                }

                return;
            }

            if (index.HasValue)
            {
                index = index.Value + 1;
                result.Add(index.Value);
            }

            result.Add(stats.Delay);

            if (index.HasValue)
            {
                index = index.Value + 1;
                result.Add(index.Value);
            }

            result.Add(string.Join(", ", stats.SubAttacks.Select(s => s.HitProbability).Select(p => p + "%")));

            if (index.HasValue)
            {
                index = index.Value + 1;
                result.Add(index.Value);
            }

            var damages = stats.SubAttacks.Select(s
                => manager.EffectApplicationSystem.GetExpectedDamage(s.Effects, attackerEntity, victimEntity)).ToList();
            result.Add(string.Join(", ", damages));

            if (index.HasValue)
            {
                index = index.Value + 1;
                result.Add(index.Value);
            }

            var expectedDamage = 0;
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < stats.SubAttacks.Count; i++)
            {
                expectedDamage += damages[i] * stats.SubAttacks[i].HitProbability;
            }

            var ticksToKill = expectedDamage <= 0
                ? 0
                : Math.Ceiling(victimEntity.Being.HitPoints * 100f / expectedDamage) * stats.Delay;
            result.Add(ticksToKill);
        }

        public static List<object> SerializeAttributes(
            GameEntity actorEntity, SenseType sense, SerializationContext context)
        {
            var canIdentify = actorEntity != null && sense.CanIdentify();
            if (!canIdentify)
            {
                return new List<object>();
            }

            var being = actorEntity.Being;
            var sensor = actorEntity.Sensor;
            var physical = actorEntity.Physical;
            var description = actorEntity.HasComponent(EntityComponent.Player)
                ? ""
                : context.Services.Language.GetDescription(
                    actorEntity.Manager.RacesToBeingRelationship[actorEntity.Id].Values.First().Race.TemplateName,
                    DescriptionCategory.Creature);
            var result = new List<object>(42)
            {
                context.Services.Language.GetActorName(actorEntity, sense),
                description,
                actorEntity.Manager.XPSystem.GetXPLevel(actorEntity, actorEntity.Manager),
                actorEntity.Position.MovementDelay,
                physical.Size,
                physical.Weight,
                sensor.PrimaryFOVQuadrants,
                sensor.PrimaryVisionRange,
                sensor.TotalFOVQuadrants,
                sensor.SecondaryVisionRange,
                sensor.Infravision,
                sensor.InvisibilityDetection,
                physical.Infravisible,
                being.Visibility,
                being.HitPoints,
                being.HitPointMaximum,
                being.EnergyPoints,
                being.EnergyPointMaximum,
                being.Might,
                being.Speed,
                being.Focus,
                being.Perception,
                being.Regeneration,
                being.EnergyRegeneration,
                being.Armor,
                being.Deflection,
                being.Accuracy,
                being.Evasion,
                being.Hindrance,
                being.PhysicalResistance,
                being.MagicResistance,
                being.BleedingResistance,
                being.AcidResistance,
                being.ColdResistance,
                being.ElectricityResistance,
                being.FireResistance,
                being.PsychicResistance,
                being.ToxinResistance,
                being.VoidResistance,
                being.SonicResistance,
                being.StunResistance,
                being.LightResistance,
                being.WaterResistance
            };

            var isPlayer = actorEntity.HasComponent(EntityComponent.Player);

            result.Add(actorEntity.Manager.AbilitiesToAffectableRelationship[actorEntity.Id]
                .Where(a => a.Ability.IsUsable
                            && ((!isPlayer
                                 && a.Ability.Activation != ActivationType.Default
                                 && a.Ability.Activation != ActivationType.Always
                                 && a.Ability.Activation != ActivationType.OnMeleeAttack
                                 && a.Ability.Activation != ActivationType.OnRangedAttack)
                                || (isPlayer
                                    && a.Ability.Template?.Type == AbilityType.DefaultAttack)))
                .Select(a => AbilitySnapshot.SerializeAttributes(a, actorEntity, context)).ToList());

            return result;
        }
    }
}
