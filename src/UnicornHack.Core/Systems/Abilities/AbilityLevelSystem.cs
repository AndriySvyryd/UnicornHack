using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public partial class AbilityLevelSystem :
    IGameSystem<DiedMessage>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int>>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<EntityRemovedMessage<GameEntity>>,
    IEntityChangeListener<GameEntity>
{
    public MessageProcessingResult Process(DiedMessage message, GameManager manager)
    {
        manager.Game.Stats.DeathCount++;
        if (message.BeingEntity.Position?.RoomEntity.Room is { } room)
        {
            room.Level.Stats.DeathCount++;
            foreach (var actorRoom in room.Level.Rooms)
            {
                foreach (var actor in actorRoom.Actors.Values)
                {
                    if (actor.Being is { IsAlive: true })
                    {
                        RefreshDynamicAbilityLevels(actor, EffectApplicationSystem.LevelDeathsInput, manager);
                    }
                }
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
    {
        if (message.Entity.Ability is { OwnerEntity: { } owner } ability)
        {
            SynchronizeDependents(ability.Name, owner, manager, ability.Type);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        if (message.Group.Name != nameof(GameManager.AbilitiesToAffectableRelationship))
        {
            throw new InvalidOperationException(message.Group.Name);
        }

        if (message.Entity.Ability is { } ability && message.PrincipalEntity is { } owner)
        {
            SynchronizeDependents(ability.Name, owner, manager, ability.Type);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager manager)
    {
        if (message.Group.Name != nameof(GameManager.AbilitiesToAffectableRelationship))
        {
            throw new InvalidOperationException(message.Group.Name);
        }

        if (message.RemovedComponent is AbilityComponent ability && message.PrincipalEntity is { } owner)
        {
            SynchronizeDependents(ability.Name, owner, manager, ability.Type);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var owner = entityChange.Entity;
        if (owner.ActiveEffects is not { DynamicAbilitiesInitialized: true } || owner.Being is not { IsAlive: true })
        {
            return;
        }

        var changes = entityChange.PropertyChanges;
        for (var index = 0; index < changes.Count; index++)
        {
            var property = changes.GetChangedPropertyName(index);
            if (changes.GetChangedComponent(index) is not BeingComponent)
            {
                continue;
            }

            if (property is nameof(BeingComponent.HitPoints) or nameof(BeingComponent.HitPointMaximum)
                && (changes.GetValue<int>(index, Utils.MessagingECS.ValueType.Current) <= 0
                    || changes.GetValue<int>(index, Utils.MessagingECS.ValueType.Old) <= 0))
            {
                continue;
            }

            RefreshDynamicAbilityLevels(owner, property, owner.Manager);
        }
    }

    private static IEnumerable<Ability> GetExpressionTemplates(GameEntity owner, GameManager manager)
        => (Creature.GetTemplate(owner)?.Abilities ?? [])
            .Where(ability => ability.LevelExpression != null
                && FindComponentEffect(owner, EffectType.AddAbility, ability.Name) != null)
            .Concat(manager.Game.Services.Data.Abilities.GetAll().Where(ability => ability.LevelExpression != null))
            .DistinctBy(ability => ability.Name);

    public static void RebuildDynamicAbilities(GameEntity owner, GameManager manager)
    {
        if (owner.ActiveEffects is not { } activeEffects || owner.Being is not { IsAlive: true })
        {
            return;
        }

        activeEffects.ClearDynamicAbilities();
        activeEffects.DynamicAbilitiesInitialized = true;
        foreach (var template in GetExpressionTemplates(owner, manager))
        {
            activeEffects.RegisterDynamicAbility(template);
        }

        foreach (var template in activeEffects.GetDynamicAbilities())
        {
            SynchronizeExpressionAbilityLevel(owner, template, manager);
        }
    }

    public static void RefreshDynamicAbilityLevels(GameEntity owner, string input, GameManager manager)
    {
        if (owner.ActiveEffects is not { DynamicAbilitiesInitialized: true } activeEffects
            || !activeEffects.HasDynamicAbilityDependents(input))
        {
            return;
        }

        var dependents = activeEffects.GetDynamicAbilityDependents(input);
        for (var index = 0; index < dependents.Count; index++)
        {
            SynchronizeExpressionAbilityLevel(owner, dependents[index], manager);
        }
    }

    private static void SynchronizeDependents(string? abilityName, GameEntity owner, GameManager manager, AbilityType abilityType)
    {
        if (abilityType == AbilityType.Race)
        {
            RebuildDynamicAbilities(owner, manager);
            return;
        }

        if (abilityName == null)
        {
            return;
        }

        if (owner.ActiveEffects is { DynamicAbilitiesInitialized: false })
        {
            RebuildDynamicAbilities(owner, manager);
        }
        else
        {
            RefreshDynamicAbilityLevels(owner, abilityName, manager);
        }
    }

    public static void SynchronizeExpressionAbilityLevel(GameEntity targetEntity, Ability template, GameManager manager)
    {
        Debug.Assert(template.LevelExpression != null);
        if (targetEntity.ActiveEffects is not { } activeEffects || targetEntity.Being is { IsAlive: false })
        {
            return;
        }

        var record = FindComponentEffect(targetEntity, EffectType.AddAbility, template.Name);
        var level = GetExpressionLevel(targetEntity, template, record);

        if (level == 0)
        {
            if (template.Type == AbilityType.Race
                && targetEntity.Being is { } ownerBeing
                && !ownerBeing.Races.Any(race => race.Ability is { Level: > 0, IsUsable: true } ability
                    && ability.Name != template.Name))
            {
                ownerBeing.HitPoints = 0;
            }

            if (record is { SourceAbilityEntityId: 0 })
            {
                DetachComponentCarrier(record, targetEntity, manager);
            }
            else if (record != null)
            {
                record.Level = 0;
                record.CarrierId = 0;
            }

            return;
        }

        if (record?.Level == level)
        {
            return;
        }

        if (record == null)
        {
            activeEffects.AddComponentEffect(
                EffectType.AddAbility, template.Name, level: level);
        }
        else
        {
            record.Level = level;
        }

        manager.AbilityLevelSystem.RelevelAbility(template.Name, targetEntity, manager, template);
    }

    public static int GetExpressionLevel(GameEntity owner, Ability template)
        => GetExpressionLevel(owner, template, FindComponentEffect(owner, EffectType.AddAbility, template.Name));

    private static int GetExpressionLevel(GameEntity owner, Ability template, ActiveComponentEffect? record)
    {
        var level = template.EvaluateLevelExpression(owner);
        return Math.Clamp(
            template.NonDecreasingLevel ? Math.Max(level, record?.Level ?? 0) : level,
            0, template.MaxLevel);
    }
}