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
                properties = state == null
                    ? new List<object?>(6)
                    : new List<object?>(7) { (int)state };
                properties.Add(actorKnowledge!.EntityId);

                Debug.Assert(knownEntity != null, nameof(knownEntity));
                Debug.Assert(position != null, nameof(position));
                Debug.Assert(being != null, nameof(being));
                if (actorKnowledge.SensedType.CanIdentify())
                {
                    properties.Add(ai != null
                        ? actorKnowledge.KnownEntity.Being!.Races.First().Race!.TemplateName
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
                properties.Add((byte)position.Heading!);

                if (actorKnowledge.SensedType.CanIdentify()
                    && ai != null)
                {
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

                    return properties;
                }

                return properties;
            case EntityState.Deleted:
                return new List<object?> { (int)state, knowledgeEntity.Id };
            default:
                properties = new List<object?>(2) { (int)state, actorKnowledge!.EntityId };

                Debug.Assert(knownEntity != null, nameof(knownEntity));
                Debug.Assert(position != null, nameof(position));
                Debug.Assert(being != null, nameof(being));
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
                            ? actorKnowledge.KnownEntity.Being!.Races.First().Race!.TemplateName
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
                        properties.Add((byte)position.Heading!);
                    }
                }
                else
                {
                    i += 3;
                }

                if (ai != null)
                {
                    i++;
                    var currentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell)
                        .CanIdentify();
                    var currentPerceptionChanged = snapshot!.CurrentlyPerceived != currentlyPerceived;
                    if (currentPerceptionChanged)
                    {
                        properties.Add(i);
                        properties.Add(currentlyPerceived);
                        snapshot.CurrentlyPerceived = currentlyPerceived;
                    }

                    var beingEntry = context.DbContext.Entry(being);
                    if (beingEntry.State != EntityState.Unchanged
                        || currentPerceptionChanged)
                    {
                        i++;
                        var hitPoints = beingEntry.Property(nameof(BeingComponent.HitPoints));
                        if (hitPoints.IsModified
                            || currentPerceptionChanged)
                        {
                            properties.Add(i);
                            properties.Add(currentlyPerceived ? being.HitPoints : 0);
                        }

                        i++;
                        var hitPointMaximum = beingEntry.Property(nameof(BeingComponent.HitPointMaximum));
                        if (hitPointMaximum.IsModified
                            || currentPerceptionChanged)
                        {
                            properties.Add(i);
                            properties.Add(currentlyPerceived ? being.HitPointMaximum : 0);
                        }

                        i++;
                        var energyPoints = beingEntry.Property(nameof(BeingComponent.EnergyPoints));
                        if (energyPoints.IsModified
                            || currentPerceptionChanged)
                        {
                            properties.Add(i);
                            properties.Add(currentlyPerceived ? being.EnergyPoints : 0);
                        }

                        i++;
                        var energyPointMaximum = beingEntry.Property(nameof(BeingComponent.EnergyPointMaximum));
                        if (energyPointMaximum.IsModified
                            || currentPerceptionChanged)
                        {
                            properties.Add(i);
                            properties.Add(currentlyPerceived ? being.EnergyPointMaximum : 0);
                        }
                    }
                    else
                    {
                        i += 4;
                    }

                    var aiEntry = context.DbContext.Entry(ai);
                    if (currentPerceptionChanged
                        || (currentlyPerceived
                            && aiEntry.State != EntityState.Unchanged
                            && aiEntry.Property(nameof(AIComponent.NextActionTick)).IsModified))
                    {
                        i++;
                        properties.Add(i);
                        properties.Add(currentlyPerceived ? ai.NextActionTick : 0);

                        i++;
                        properties.Add(i);
                        properties.Add(currentlyPerceived ? SerializeNextAction(ai, context) : null);
                    }
                    else
                    {
                        i += 2;
                    }

                    if (sensedType.IsModified)
                    {
                        var canIdentify = actorKnowledge.SensedType.CanIdentify();

                        // TODO: Snapshot the attacks and update them while the actor is perceived
                        i++;
                        properties.Add(i);
                        var meleeAttack = canIdentify
                            ? manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: true, manager)
                            : null;
                        properties.Add(SerializeAttackSummary(
                            meleeAttack,
                            knownEntity,
                            context.Observer,
                            manager));

                        i++;
                        properties.Add(i);
                        var rangedAttack = canIdentify
                            ? manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: false, manager)
                            : null;
                        properties.Add(SerializeAttackSummary(
                            rangedAttack,
                            knownEntity,
                            context.Observer,
                            manager));

                        i++;
                        properties.Add(i);
                        meleeAttack = canIdentify
                            ? manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: true, manager)
                            : null;
                        properties.Add(SerializeAttackSummary(
                            meleeAttack,
                            context.Observer,
                            knownEntity,
                            manager));

                        i++;
                        properties.Add(i);
                        rangedAttack = canIdentify
                            ? manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: false, manager)
                            : null;
                        properties.Add(SerializeAttackSummary(
                            rangedAttack,
                            context.Observer,
                            knownEntity,
                            manager));
                    }
                }

                return properties.Count > 2 ? properties : null;
        }
    }

    private static List<object?>? SerializeNextAction(AIComponent ai, SerializationContext context)
    {
        var result = new List<object?>(2);

        var manager = context.Manager;
        result.Add(ai.NextAction);
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
                result.Add(ai.NextActionTarget2);
                result.Add(ability.TargetingShape);
                result.Add(ability.TargetingShapeSize);

                return result;
            case ActorAction.MoveOneCell:
            case ActorAction.ChangeHeading:
                var direction = (Direction)ai.NextActionTarget!;
                var action = ai.NextAction == ActorAction.MoveOneCell ? "Move " : "Turn ";
                result.Add(action + context.Services.Language.GetString(direction, abbreviate: true));
                result.Add(ai.Entity.Position!.LevelCell.Translate(direction.AsVector()).ToInt32());

                return result;
            default:
                result.Add(ai.NextAction.ToString());
                result.Add(ai.NextActionTarget);

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
        result.Add(ticksToKill);

        return result;
    }

    public static List<object> SerializeAttributes(
        GameEntity? actorEntity, SenseType sense, SerializationContext context)
    {
        var canIdentify = actorEntity != null && sense.CanIdentify();
        if (!canIdentify)
        {
            return new List<object>();
        }

        var being = actorEntity!.Being!;
        var sensor = actorEntity.Sensor!;
        var physical = actorEntity.Physical!;
        var description = actorEntity.HasComponent(EntityComponent.Player)
            ? ""
            : context.Services.Language.GetDescription(
                actorEntity.Being!.Races.First().Race!.TemplateName,
                DescriptionCategory.Creature);
        var result = new List<object>(42)
        {
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

        result.Add(actorEntity.Being!.Abilities
            .Where(a => a.Ability!.IsUsable
                        && ((!isPlayer
                             && a.Ability.Activation != ActivationType.Default
                             && a.Ability.Activation != ActivationType.Always
                             && a.Ability.Activation != ActivationType.OnMeleeAttack
                             && a.Ability.Activation != ActivationType.OnRangedAttack)
                            || (isPlayer
                                && a.Ability.Type == AbilityType.DefaultAttack)))
            .Select(a => AbilitySnapshot.SerializeAttributes(a, actorEntity, context)).ToList());

        return result;
    }
}
