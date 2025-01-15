using System.Collections;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs;

public class LevelActorSnapshot
{
    private bool CurrentlyPerceived
    {
        get;
        set;
    }

    public LevelActorSnapshot CaptureState(GameEntity knowledgeEntity, SerializationContext context)
    {
        var actorKnowledge = knowledgeEntity.Knowledge!;
        var knownEntity = actorKnowledge.KnownEntity;
        var position = knowledgeEntity.Position!;
        var manager = context.Manager;
        CurrentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();

        return this;
    }

    public static List<object?>? Serialize(
        GameEntity knowledgeEntity, EntityState? state, LevelActorSnapshot? snapshot, SerializationContext context)
    {
        List<object?> properties;
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
            {
                Debug.Assert(actorKnowledge != null, nameof(actorKnowledge));
                Debug.Assert(knownEntity != null, nameof(knownEntity));
                Debug.Assert(position != null, nameof(position));
                Debug.Assert(being != null, nameof(being));

                var canIdentify = actorKnowledge.SensedType.CanIdentify();
                properties = new List<object?>(canIdentify ? 17 : 4)
                {
                    null,
                    position.LevelX,
                    position.LevelY,
                    (byte)position.Heading!
                };

                if (canIdentify)
                {
                    properties.Add(ai != null
                        ? actorKnowledge.KnownEntity.Being!.Races.First().Race!.TemplateName
                        : "player");
                    properties.Add(context.Services.Language.GetActorName(knownEntity, actorKnowledge.SensedType));
                    var currentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell)
                        .CanIdentify();
                    if (snapshot != null)
                    {
                        snapshot.CurrentlyPerceived = currentlyPerceived;
                    }

                    properties.Add(currentlyPerceived);
                    properties.Add(currentlyPerceived ? being.HitPoints : 0);
                    properties.Add(currentlyPerceived ? being.HitPointMaximum : 0);
                    properties.Add(currentlyPerceived ? being.EnergyPoints : 0);
                    properties.Add(currentlyPerceived ? being.EnergyPointMaximum : 0);

                    if (ai != null)
                    {
                        properties.Add(currentlyPerceived ? ai.NextActionTick : 0);
                        properties.Add(currentlyPerceived ? SerializeNextAction(ai, context) : null);

                        var meleeAttack =
                            manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: true, manager);
                        properties.Add(SerializeAttackSummary(
                            meleeAttack,
                            knownEntity,
                            context.Observer,
                            manager));

                        var rangedAttack =
                            manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: false, manager);
                        properties.Add(SerializeAttackSummary(
                            rangedAttack,
                            knownEntity,
                            context.Observer,
                            manager));

                        meleeAttack = manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: true, manager);
                        properties.Add(SerializeAttackSummary(
                            meleeAttack,
                            context.Observer,
                            knownEntity,
                            manager));

                        rangedAttack = manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: false, manager);
                        properties.Add(SerializeAttackSummary(
                            rangedAttack,
                            context.Observer,
                            knownEntity,
                            manager));
                    }
                    else
                    {
                        properties.Add(0);
                        properties.AddRange(Enumerable.Repeat<object?>(null, 5));
                    }
                }
                else
                {
                    var setValues = new bool[17];
                    setValues[0] = true;
                    setValues[1] = true;
                    setValues[2] = true;
                    setValues[3] = true;
                    properties[0] = new BitArray(setValues);
                }

                return properties;
            }
            case EntityState.Deleted:
                return SerializationContext.DeletedBitArray;
            default:
            {
                var i = 0;
                var setValues = new bool[17];
                setValues[i++] = true;
                properties = [null];

                Debug.Assert(actorKnowledge != null, nameof(actorKnowledge));
                Debug.Assert(knownEntity != null, nameof(knownEntity));
                Debug.Assert(position != null, nameof(position));
                Debug.Assert(being != null, nameof(being));

                var positionEntry = context.DbContext.Entry(position);
                if (positionEntry.State != EntityState.Unchanged)
                {
                    var levelX = positionEntry.Property(nameof(PositionComponent.LevelX));
                    if (levelX.IsModified)
                    {
                        setValues[i++] = true;
                        properties.Add(position.LevelX);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }

                    var levelY = positionEntry.Property(nameof(PositionComponent.LevelY));
                    if (levelY.IsModified)
                    {
                        setValues[i++] = true;
                        properties.Add(position.LevelY);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }

                    var heading = positionEntry.Property(nameof(PositionComponent.Heading));
                    if (heading.IsModified)
                    {
                        setValues[i++] = true;
                        properties.Add((byte)position.Heading!);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }
                }
                else
                {
                    setValues[i++] = false;
                    setValues[i++] = false;
                    setValues[i++] = false;
                }

                var knowledgeEntry = context.DbContext.Entry(actorKnowledge);
                var sensedType = knowledgeEntry.Property(nameof(KnowledgeComponent.SensedType));
                if (sensedType.IsModified)
                {
                    var canIdentify = actorKnowledge.SensedType.CanIdentify();
                    setValues[i++] = true;
                    properties.Add(!canIdentify
                        ? null
                        : ai != null
                            ? actorKnowledge.KnownEntity.Being!.Races.First().Race!.TemplateName
                            : "player");

                    setValues[i++] = true;
                    properties.Add(!canIdentify
                        ? null
                        : context.Services.Language.GetActorName(knownEntity, actorKnowledge.SensedType));
                }
                else
                {
                    setValues[i++] = false;
                    setValues[i++] = false;
                }

                var currentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell)
                    .CanIdentify();
                var currentPerceptionChanged = snapshot!.CurrentlyPerceived != currentlyPerceived;
                if (currentPerceptionChanged)
                {
                    setValues[i++] = true;
                    properties.Add(currentlyPerceived);
                    snapshot.CurrentlyPerceived = currentlyPerceived;
                }
                else
                {
                    setValues[i++] = false;
                }

                var beingEntry = context.DbContext.Entry(being);
                if (beingEntry.State != EntityState.Unchanged
                    || currentPerceptionChanged)
                {
                    var hitPoints = beingEntry.Property(nameof(BeingComponent.HitPoints));
                    if ((hitPoints.IsModified && currentlyPerceived)
                        || currentPerceptionChanged)
                    {
                        setValues[i++] = true;
                        properties.Add(currentlyPerceived ? being.HitPoints : 0);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }

                    var hitPointMaximum = beingEntry.Property(nameof(BeingComponent.HitPointMaximum));
                    if ((hitPointMaximum.IsModified && currentlyPerceived)
                        || currentPerceptionChanged)
                    {
                        setValues[i++] = true;
                        properties.Add(currentlyPerceived ? being.HitPointMaximum : 0);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }

                    var energyPoints = beingEntry.Property(nameof(BeingComponent.EnergyPoints));
                    if ((energyPoints.IsModified && currentlyPerceived)
                        || currentPerceptionChanged)
                    {
                        setValues[i++] = true;
                        properties.Add(currentlyPerceived ? being.EnergyPoints : 0);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }

                    var energyPointMaximum = beingEntry.Property(nameof(BeingComponent.EnergyPointMaximum));
                    if ((energyPointMaximum.IsModified && currentlyPerceived)
                        || currentPerceptionChanged)
                    {
                        setValues[i++] = true;
                        properties.Add(currentlyPerceived ? being.EnergyPointMaximum : 0);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }
                }
                else
                {
                    setValues[i++] = false;
                    setValues[i++] = false;
                    setValues[i++] = false;
                    setValues[i++] = false;
                }

                if (ai != null)
                {
                    var aiEntry = context.DbContext.Entry(ai);
                    if (currentPerceptionChanged
                        || (currentlyPerceived
                            && aiEntry.State != EntityState.Unchanged
                            && aiEntry.Property(nameof(AIComponent.NextActionTick)).IsModified))
                    {
                        setValues[i++] = true;
                        properties.Add(currentlyPerceived ? ai.NextActionTick : 0);

                        setValues[i++] = true;
                        properties.Add(currentlyPerceived ? SerializeNextAction(ai, context) : null);
                    }
                    else
                    {
                        setValues[i++] = false;
                        setValues[i++] = false;
                    }

                    if (sensedType.IsModified)
                    {
                        var canIdentify = actorKnowledge.SensedType.CanIdentify();

                        // TODO: Snapshot the attacks and update them while the actor is perceived
                        setValues[i++] = true;
                        var meleeAttack = canIdentify
                            ? manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: true, manager)
                            : null;
                        properties.Add(SerializeAttackSummary(
                            meleeAttack,
                            knownEntity,
                            context.Observer,
                            manager));

                        setValues[i++] = true;
                        var rangedAttack = canIdentify
                            ? manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: false, manager)
                            : null;
                        properties.Add(SerializeAttackSummary(
                            rangedAttack,
                            knownEntity,
                            context.Observer,
                            manager));

                        setValues[i++] = true;
                        meleeAttack = canIdentify
                            ? manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: true, manager)
                            : null;
                        properties.Add(SerializeAttackSummary(
                            meleeAttack,
                            context.Observer,
                            knownEntity,
                            manager));

                        setValues[i++] = true;
                        rangedAttack = canIdentify
                            ? manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: false, manager)
                            : null;
                        properties.Add(SerializeAttackSummary(
                            rangedAttack,
                            context.Observer,
                            knownEntity,
                            manager));
                    }
                    else
                    {
                        setValues[i++] = false;
                        setValues[i++] = false;
                        setValues[i++] = false;
                        setValues[i++] = false;
                    }
                }
                else
                {
                    i += 6;
                }

                if (properties.Count == 1)
                {
                    return null;
                }

                Debug.Assert(i == 17);
                properties[0] = new BitArray(setValues);
                return properties;
            }
        }
    }

    private static List<object?>? SerializeNextAction(AIComponent ai, SerializationContext context)
    {
        var result = new List<object?>(2);

        var manager = context.Manager;
        result.Add((int?)ai.NextAction);
        switch (ai.NextAction)
        {
            case ActorAction.UseAbilitySlot:
                var slot = ai.NextActionTarget;
                if (!slot.HasValue)
                {
                    // TODO: Log
                    return null;
                }

                var abilityEntity = manager.AbilitySlottingSystem.GetAbility(ai.Entity, slot.Value);
                if (abilityEntity == null)
                {
                    // TODO: Log
                    return null;
                }

                var ability = abilityEntity.Ability!;
                result.Add(context.Services.Language.GetString(ability));
                result.Add(ai.NextActionTarget2 ?? 0);
                result.Add((int)ability.TargetingShape);
                result.Add(ability.TargetingShapeSize);

                return result;
            case ActorAction.MoveOneCell:
            case ActorAction.ChangeHeading:
                var direction = (Direction)ai.NextActionTarget!;
                var action = ai.NextAction == ActorAction.MoveOneCell ? "Move " : "Turn ";
                result.Add(action + context.Services.Language.GetString(direction, abbreviate: true));
                result.Add(ai.Entity.Position!.LevelCell.Translate(direction.AsVector()).ToInt32());
                result.Add((int)TargetingShape.Line);
                result.Add(0);

                return result;
            default:
                result.Add(ai.NextAction?.ToString());
                result.Add(ai.NextActionTarget ?? 0);
                result.Add((int)TargetingShape.Line);
                result.Add(0);

                return result;
        }
    }

    private static List<object?>? SerializeAttackSummary(
        GameEntity? attackEntity,
        GameEntity attackerEntity,
        GameEntity victimEntity,
        GameManager manager)
    {
        var result = new List<object?>(4);
        AttackStats? stats = null;
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
            return null;
        }

        result.Add(stats.Delay);

        result.Add(string.Join(", ", stats.SubAttacks.Select(s => s.HitProbability).Select(p => p + "%")));

        var damages = stats.SubAttacks.Select(s
            => manager.EffectApplicationSystem.GetExpectedDamage(s.Effects, attackerEntity, victimEntity)).ToList();
        result.Add(string.Join(", ", damages));

        var expectedDamage = 0;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < stats.SubAttacks.Count; i++)
        {
            expectedDamage += damages[i] * stats.SubAttacks[i].HitProbability;
        }

        var ticksToKill = expectedDamage <= 0
            ? 0
            : Math.Ceiling(victimEntity.Being!.HitPoints * 100f / expectedDamage) * stats.Delay;
        result.Add((int)ticksToKill);

        return result;
    }

    public static List<object?> SerializeAttributes(
        GameEntity? actorEntity, SenseType sense, SerializationContext context)
    {
        var canIdentify = actorEntity != null && sense.CanIdentify();
        if (!canIdentify)
        {
            return SerializationContext.DeletedBitArray;
        }

        var being = actorEntity!.Being!;
        var sensor = actorEntity.Sensor!;
        var physical = actorEntity.Physical!;
        var description = actorEntity.HasComponent(EntityComponent.Player)
            ? ""
            : context.Services.Language.GetDescription(
                actorEntity.Being!.Races.First().Race!.TemplateName,
                DescriptionCategory.Creature);
        var result = new List<object?>(45)
        {
            null,
            context.Services.Language.GetActorName(actorEntity, sense),
            description,
            actorEntity.Manager.XPSystem.GetXPLevel(actorEntity),
            actorEntity.Position!.MovementDelay,
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

        var abilities = actorEntity.Being!.Abilities
            .Where(a => a.Ability!.IsUsable
                    && ((!isPlayer
                            && a.Ability.Activation != ActivationType.Default
                            && a.Ability.Activation != ActivationType.Always
                            && a.Ability.Activation != ActivationType.OnMeleeAttack
                            && a.Ability.Activation != ActivationType.OnRangedAttack)
                        || (isPlayer
                            && a.Ability.Type == AbilityType.DefaultAttack)))
            .Select(a => AbilitySnapshot.SerializeAttributes(a, actorEntity, context))
            .ToList();
        result.Add(abilities);

        return result;
    }
}
