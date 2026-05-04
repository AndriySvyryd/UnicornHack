using System.Collections;
using System.Text;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class ActorChangeBuilder : ChangeBuilder<ActorChange>
{
    /// <summary>
    ///     Persistent cache, keyed by knowledge-entity id, of the last emitted values
    ///     for fields that are recomputed each cycle. Used to suppress redundant emissions
    ///     in deltas when the computed values are identical to what the client already has.
    ///     Updated in <see cref="OnAfterEmit" />.
    /// </summary>
    private readonly Dictionary<int, LastEmittedValues> _lastEmittedValues = [];

    private readonly record struct LastEmittedValues(
        ActorActionChange? NextAction,
        AttackSummary? MeleeAttack,
        AttackSummary? RangeAttack,
        AttackSummary? MeleeDefense,
        AttackSummary? RangeDefense);

    public override void RegisterOnGroups(GameManager manager)
    {
        manager.KnownPositions.AddListener(this);
        manager.KnownActorsToLevelCellRelationship.AddDependentsListener(this);
        manager.LevelActors.AddListener(this);
    }

    public override void UnregisterFromGroups(GameManager manager)
    {
        manager.KnownPositions.RemoveListener(this);
        manager.KnownActorsToLevelCellRelationship.RemoveDependentsListener(this);
        manager.LevelActors.RemoveListener(this);
    }

    /// <summary>
    ///     Routes property changes from the <c>LevelActors</c> group (where the entity is a
    ///     living being) to its corresponding knowledge entity — that's what the client
    ///     actually tracks. Property changes on the knowledge entity itself (from
    ///     <c>KnownPositions</c>) go to <see cref="TrackPropertyChanges" /> on that entity.
    /// </summary>
    protected override void OnBaseGroupPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var entity = entityChange.Entity;
        var changes = entityChange.PropertyChanges;

        if (entity.Being != null)
        {
            var knowledgeEntity = FindKnowledgeEntity(entity);
            if (knowledgeEntity == null)
            {
                return;
            }

            var tracked = GetOrCreateChange(knowledgeEntity, EntityState.Modified);

            TrackBeingPropertyChanges(entity, tracked, changes);
            return;
        }

        if (Changes.TryGetValue(entity.Id, out var knowledgeTracked))
        {
            TrackPropertyChanges(entity, knowledgeTracked, changes);
            return;
        }

        // The entity isn't tracked yet this turn. Check whether the property change
        // warrants creating a new entry — only SensedType changes need this.
        if (entity.Knowledge?.KnownEntity.Being == null)
        {
            return;
        }

        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId == (int)EntityComponent.Knowledge
                && changes.GetChangedPropertyName(i) == nameof(KnowledgeComponent.SensedType))
            {
                knowledgeTracked = GetOrCreateChange(entity, EntityState.Modified);
                TrackPropertyChanges(entity, knowledgeTracked, changes);
                return;
            }
        }
    }

    protected override void TrackPropertyChanges(
        GameEntity entity, ActorChange change, IPropertyValueChanges changes)
    {
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);

            if (componentId == (int)EntityComponent.Position)
            {
                switch (propertyName)
                {
                    case nameof(PositionComponent.LevelX):
                        change.LevelX = entity.Position!.LevelX;
                        break;
                    case nameof(PositionComponent.LevelY):
                        change.LevelY = entity.Position!.LevelY;
                        break;
                    case nameof(PositionComponent.Heading):
                        change.Heading = (byte)entity.Position!.Heading!;
                        break;
                }
            }
            else if (componentId == (int)EntityComponent.Knowledge)
            {
                if (propertyName == nameof(KnowledgeComponent.SensedType))
                {
                    var newSensedType = changes.GetValue<SenseType>(i, Utils.MessagingECS.ValueType.Current);
                    var oldSensedType = changes.GetValue<SenseType>(i, Utils.MessagingECS.ValueType.Old);
                    if (newSensedType.CanIdentify() != oldSensedType.CanIdentify())
                    {
                        change.IsCurrentlyPerceived = newSensedType.CanIdentify();
                    }
                }
                else if (propertyName == nameof(KnowledgeComponent.IsIdentified))
                {
                    var knowledge = entity.Knowledge!;
                    var knownEntity = knowledge.KnownEntity;
                    var canIdentify = knowledge.IsIdentified;
                    change.BaseName = !canIdentify
                        ? null
                        : knownEntity.AI != null
                            ? knownEntity.Being!.Races.First().Race!.TemplateName
                            : "player";
                    change.Name = !canIdentify
                        ? null
                        : entity.Game.Services.Language.GetActorName(knownEntity, canIdentify);
                }
            }
        }
    }

    private static void TrackBeingPropertyChanges(
        GameEntity entity, ActorChange change, IPropertyValueChanges changes)
    {
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);

            if (componentId == (int)EntityComponent.Being)
            {
                switch (propertyName)
                {
                    case nameof(BeingComponent.HitPoints):
                        change.Hp = entity.Being!.HitPoints;
                        break;
                    case nameof(BeingComponent.HitPointMaximum):
                        change.MaxHp = entity.Being!.HitPointMaximum;
                        break;
                    case nameof(BeingComponent.EnergyPoints):
                        change.Ep = entity.Being!.EnergyPoints;
                        break;
                    case nameof(BeingComponent.EnergyPointMaximum):
                        change.MaxEp = entity.Being!.EnergyPointMaximum;
                        break;
                }
            }
            else if (componentId == (int)EntityComponent.AI)
            {
                if (propertyName == nameof(AIComponent.NextActionTick))
                {
                    change.NextActionTick = entity.AI!.NextActionTick ?? 0;
                }
            }
            else if (componentId == (int)EntityComponent.Player)
            {
                if (propertyName == nameof(PlayerComponent.NextActionTick))
                {
                    change.NextActionTick = entity.Player!.NextActionTick ?? 0;
                }
            }
        }
    }

    protected override ActorChange ProduceFullSnapshot(GameEntity entity, SerializationContext context)
        => SerializeActor(entity, context);

    protected override ActorChange? ProduceDelta(GameEntity entity, ActorChange tracked, SerializationContext context)
    {
        var actorKnowledge = entity.Knowledge;
        var knownEntity = actorKnowledge?.KnownEntity;
        var being = knownEntity?.Being;
        if (actorKnowledge == null || being == null)
        {
            return new ActorChange { ChangedProperties = new BitArray(1) };
        }

        var ai = knownEntity!.AI;
        var currentlyPerceived = actorKnowledge.SensedType.CanIdentify();
        var perceptionChanged = tracked.ChangedProperties![6];

        if (perceptionChanged
            && (tracked.ChangedProperties[4] || tracked.ChangedProperties[5]))
        {
            tracked.Hp = being.HitPoints;
            tracked.MaxHp = being.HitPointMaximum;
            tracked.Ep = being.EnergyPoints;
            tracked.MaxEp = being.EnergyPointMaximum;
        }

        if (perceptionChanged && !currentlyPerceived)
        {
            tracked.NextActionTick = 0;
            tracked.NextAction = null;
        }
        else if (ai != null && currentlyPerceived
            && (perceptionChanged || tracked.ChangedProperties[11]
                || tracked.ChangedProperties[1]))
        {
            tracked.NextActionTick = ai.NextActionTick ?? 0;
            var nextAction = SerializeNextAction(ai, context);
            var lastNextAction = _lastEmittedValues.GetValueOrDefault(entity.Id).NextAction;
            if (!NextActionEquals(nextAction, lastNextAction))
            {
                tracked.NextAction = nextAction;
            }
        }
        else if (knownEntity.Player is { } player && currentlyPerceived
            && (perceptionChanged || tracked.ChangedProperties[11]
                || tracked.ChangedProperties[1]))
        {
            tracked.NextActionTick = player.NextActionTick ?? 0;
        }
        else if (tracked.ChangedProperties[11])
        {
            tracked.ChangedProperties[11] = false;
        }

        if (ai != null)
        {
            var last = _lastEmittedValues.GetValueOrDefault(entity.Id);

            if (currentlyPerceived)
            {
                if (actorKnowledge.IsIdentified)
                {
                    var manager = context.Manager;
                    var meleeAbility = manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: true, manager);
                    var newMeleeAttack = SerializeAttackSummary(meleeAbility, knownEntity, context.Observer, manager);
                    var rangedAbility = manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: false, manager);
                    var newRangeAttack = SerializeAttackSummary(rangedAbility, knownEntity, context.Observer, manager);
                    meleeAbility = manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: true, manager);
                    var newMeleeDefense = SerializeAttackSummary(meleeAbility, context.Observer, knownEntity, manager);
                    rangedAbility = manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: false, manager);
                    var newRangeDefense = SerializeAttackSummary(rangedAbility, context.Observer, knownEntity, manager);

                    // Suppress redundant attack-summary emissions: only assign (and thereby
                    // set the dirty bit) when the value differs from what the client last
                    // received. The setters auto-set bits 13/14/15/16, so when nothing
                    // changed we leave tracked alone and the bits stay false.
                    if (!AttackSummaryEquals(newMeleeAttack, last.MeleeAttack))
                    {
                        tracked.MeleeAttack = newMeleeAttack;
                    }
                    if (!AttackSummaryEquals(newRangeAttack, last.RangeAttack))
                    {
                        tracked.RangeAttack = newRangeAttack;
                    }
                    if (!AttackSummaryEquals(newMeleeDefense, last.MeleeDefense))
                    {
                        tracked.MeleeDefense = newMeleeDefense;
                    }
                    if (!AttackSummaryEquals(newRangeDefense, last.RangeDefense))
                    {
                        tracked.RangeDefense = newRangeDefense;
                    }
                }
            }
            else if (perceptionChanged)
            {
                // Actor moved out of view: clear summaries on the client. Only emit if
                // the client actually had a non-null value to clear.
                if (last.MeleeAttack != null)
                {
                    tracked.MeleeAttack = null;
                }
                if (last.RangeAttack != null)
                {
                    tracked.RangeAttack = null;
                }
                if (last.MeleeDefense != null)
                {
                    tracked.MeleeDefense = null;
                }
                if (last.RangeDefense != null)
                {
                    tracked.RangeDefense = null;
                }
            }
        }

        return HasModifiedBits(tracked.ChangedProperties) ? tracked : null;
    }

    /// <summary>
    ///     After each emission, update the persistent cache so the next
    ///     cycle's <see cref="ProduceDelta" /> can suppress redundant fields.
    /// </summary>
    protected override void OnAfterEmit(int id, GameEntity? entity, ActorChange? emitted, EntityState dispatchedState)
    {
        if (dispatchedState == EntityState.Deleted)
        {
            _lastEmittedValues.Remove(id);
            return;
        }

        if (emitted == null)
        {
            // No-op delta — client's view is unchanged, cache stays as-is.
            return;
        }

        // Update only the fields that were actually emitted (bits set or full snapshot).
        var bits = emitted.ChangedProperties;
        var isFullSnapshot = bits == null;
        var existing = _lastEmittedValues.GetValueOrDefault(id);
        var nextAction = (isFullSnapshot || (bits != null && bits.Length > 12 && bits[12])) ? emitted.NextAction : existing.NextAction;
        var melee = (isFullSnapshot || (bits != null && bits.Length > 13 && bits[13])) ? emitted.MeleeAttack : existing.MeleeAttack;
        var range = (isFullSnapshot || (bits != null && bits.Length > 14 && bits[14])) ? emitted.RangeAttack : existing.RangeAttack;
        var meleeDef = (isFullSnapshot || (bits != null && bits.Length > 15 && bits[15])) ? emitted.MeleeDefense : existing.MeleeDefense;
        var rangeDef = (isFullSnapshot || (bits != null && bits.Length > 16 && bits[16])) ? emitted.RangeDefense : existing.RangeDefense;
        _lastEmittedValues[id] = new LastEmittedValues(nextAction, melee, range, meleeDef, rangeDef);
    }

    private static bool NextActionEquals(ActorActionChange? a, ActorActionChange? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Type == b.Type
               && a.Name == b.Name
               && a.Target == b.Target
               && a.TargetingShape == b.TargetingShape
               && a.TargetingShapeSize == b.TargetingShapeSize;
    }

    private static bool AttackSummaryEquals(AttackSummary? a, AttackSummary? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Delay == b.Delay
               && a.HitProbabilities == b.HitProbabilities
               && a.Damages == b.Damages
               && a.TicksToKill == b.TicksToKill;
    }

    public static ActorChange SerializeActor(GameEntity knowledgeEntity, SerializationContext context)
    {
        var actorKnowledge = knowledgeEntity.Knowledge!;
        var knownEntity = actorKnowledge.KnownEntity;
        var ai = knownEntity.AI;
        var position = knowledgeEntity.Position!;
        var being = knownEntity.Being!;
        var manager = context.Manager;

        var canIdentify = actorKnowledge.IsIdentified;
        if (canIdentify)
        {
            var currentlyPerceived = actorKnowledge.SensedType.CanIdentify();

            var change = new ActorChange
            {
                ChangedProperties = null,
                LevelX = position.LevelX,
                LevelY = position.LevelY,
                Heading = (byte)position.Heading!,
                BaseName = ai != null
                    ? knownEntity.Being!.Races.First().Race!.TemplateName
                    : "player",
                Name = context.Services.Language.GetActorName(knownEntity, actorKnowledge.IsIdentified),
                IsCurrentlyPerceived = currentlyPerceived,
                Hp = being.HitPoints,
                MaxHp = being.HitPointMaximum,
                Ep = being.EnergyPoints,
                MaxEp = being.EnergyPointMaximum
            };

            if (ai != null)
            {
                change.NextActionTick = currentlyPerceived ? ai.NextActionTick ?? 0 : 0;
                change.NextAction = currentlyPerceived ? SerializeNextAction(ai, context) : null;
                if (currentlyPerceived)
                {
                    var meleeAttack =
                        manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: true, manager);
                    change.MeleeAttack = SerializeAttackSummary(meleeAttack, knownEntity, context.Observer, manager);

                    var rangedAttack =
                        manager.AISystem.GetDefaultAttack(knownEntity, context.Observer, melee: false, manager);
                    change.RangeAttack = SerializeAttackSummary(rangedAttack, knownEntity, context.Observer, manager);

                    meleeAttack = manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: true, manager);
                    change.MeleeDefense = SerializeAttackSummary(meleeAttack, context.Observer, knownEntity, manager);

                    rangedAttack = manager.PlayerSystem.GetDefaultAttack(context.Observer, melee: false, manager);
                    change.RangeDefense = SerializeAttackSummary(rangedAttack, context.Observer, knownEntity, manager);
                }
            }
            else
            {
                var player = being.Entity.Player;
                change.NextActionTick = currentlyPerceived ? player?.NextActionTick ?? 0 : 0;
            }

            return change;
        }
        else
        {
            return new ActorChange
            {
                ChangedProperties = null,
                LevelX = position.LevelX,
                LevelY = position.LevelY,
                Heading = (byte)position.Heading!
            };
        }
    }

    private static ActorActionChange? SerializeNextAction(AIComponent ai, SerializationContext context)
    {
        if (ai.NextAction == null)
        {
            return null;
        }

        var manager = context.Manager;
        switch (ai.NextAction)
        {
            case ActorActionType.UseAbilitySlot:
                var slot = ai.NextActionTarget;
                if (!slot.HasValue)
                {
                    return null;
                }

                var abilityEntity = manager.AbilitySlottingSystem.GetAbility(ai.Entity, slot.Value);
                if (abilityEntity == null)
                {
                    return null;
                }

                var ability = abilityEntity.Ability!;
                return new ActorActionChange
                {
                    Type = ai.NextAction!.Value,
                    Name = context.Services.Language.GetString(ability),
                    Target = ai.NextActionTarget2 ?? 0,
                    TargetingShape = ability.TargetingShape,
                    TargetingShapeSize = ability.TargetingShapeSize
                };
            case ActorActionType.MoveOneCell:
            case ActorActionType.ChangeHeading:
                var direction = (Direction)ai.NextActionTarget!;
                var actionLabel = ai.NextAction == ActorActionType.MoveOneCell ? "Move " : "Turn ";
                return new ActorActionChange
                {
                    Type = ai.NextAction!.Value,
                    Name = actionLabel + context.Services.Language.GetString(direction, abbreviate: true),
                    Target = ai.Entity.Position!.LevelCell.Translate(direction.AsVector()).ToInt32(),
                    TargetingShape = TargetingShape.Line,
                    TargetingShapeSize = 0
                };
            default:
                return new ActorActionChange
                {
                    Type = ai.NextAction ?? ActorActionType.Wait,
                    Name = ai.NextAction?.ToString(),
                    Target = ai.NextActionTarget ?? 0,
                    TargetingShape = TargetingShape.Line,
                    TargetingShapeSize = 0
                };
        }
    }

    private static AttackSummary? SerializeAttackSummary(
        GameEntity? attackEntity, GameEntity attackerEntity, GameEntity victimEntity, GameManager manager)
    {
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

        // Hot path: this method is called up to 4× per perceived AI actor per cycle. The
        // previous implementation used LINQ (Select/ToList/string.Join) which allocated
        // an enumerator, a delegate, a List<int>, and per-element boxed strings on every
        // call. Replace with a single pass that fills the damages array, computes
        // expected damage, and builds both formatted strings inline.
        var subAttacks = stats.SubAttacks;
        var subAttackCount = subAttacks.Count;
        var damageSystem = manager.EffectApplicationSystem;
        var hp = victimEntity.Being!.HitPoints;

        if (subAttackCount == 0)
        {
            return new AttackSummary
            {
                Delay = stats.Delay,
                HitProbabilities = "",
                Damages = "",
                TicksToKill = 0
            };
        }

        var hitProbBuilder = new StringBuilder(subAttackCount * 6);
        var damagesBuilder = new StringBuilder(subAttackCount * 4);
        var expectedDamage = 0;
        for (var i = 0; i < subAttackCount; i++)
        {
            var sub = subAttacks[i];
            var dmg = damageSystem.GetExpectedDamage(sub.Effects, attackerEntity, victimEntity);
            expectedDamage += dmg * sub.HitProbability;

            if (i > 0)
            {
                hitProbBuilder.Append(", ");
                damagesBuilder.Append(", ");
            }
            hitProbBuilder.Append(sub.HitProbability).Append('%');
            damagesBuilder.Append(dmg);
        }

        var ticksToKill = expectedDamage <= 0
            ? 0
            : (int)(Math.Ceiling(hp * 100f / expectedDamage) * stats.Delay);

        return new AttackSummary
        {
            Delay = stats.Delay,
            HitProbabilities = hitProbBuilder.ToString(),
            Damages = damagesBuilder.ToString(),
            TicksToKill = ticksToKill
        };
    }

    private static GameEntity? FindKnowledgeEntity(GameEntity beingEntity) => beingEntity.Position?.Knowledge;
}
