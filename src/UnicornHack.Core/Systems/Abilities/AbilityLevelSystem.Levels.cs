using UnicornHack.Systems.Effects;

namespace UnicornHack.Systems.Abilities;

public partial class AbilityLevelSystem
{
    internal void ApplyAddAbility(
        in EffectApplicationContext context, string addedName, int level)
    {
        if (context.TargetEntity?.ActiveEffects == null)
        {
            return;
        }

        var effect = context.Effect.Component;
        var expiration = EffectApplicationSystem.GetExpiration(context);

        var targetEntity = context.TargetEntity;
        var manager = context.Manager;
        var embeddedAbility = effect.Entity.Ability;
        Ability? embeddedTemplate = null;
        if (embeddedAbility == null
            && effect.TargetEntityId is { } effectIndex
            && effect.ContainingAbility?.Ability?.Template?.Effects is { } containingEffects
            && effectIndex >= 0
            && effectIndex < containingEffects.Count
            && containingEffects[effectIndex] is AddAbility addAbility)
        {
            embeddedTemplate = addAbility.Ability;
        }

        var resolvedTemplate = embeddedAbility?.Template ?? embeddedTemplate;
        if (embeddedAbility == null)
        {
            resolvedTemplate ??= FindAbilityTemplate(targetEntity, addedName)
                ?? throw new InvalidOperationException($"Unknown ability '{addedName}'.");
        }

        AddOrUpdateAbilityEffect(
            targetEntity, addedName, resolvedTemplate, level, effect.ContainingAbilityId ?? 0,
            expiration, embeddedAbility,
            accumulateLevel: false);
    }

    private ActiveComponentEffect AddOrUpdateAbilityEffect(
        GameEntity targetEntity, string addedName, Ability? template, int level, int sourceAbilityEntityId,
        EffectExpiration expiration, AbilityComponent? embeddedAbility, bool accumulateLevel)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        var manager = targetEntity.Manager;
        ActiveComponentEffect? updatedRecord = null;
        foreach (var existing in activeEffects.ComponentEffects.GetValues(addedName))
        {
            ValidateAbilityLevelChange(existing, targetEntity);
            if (updatedRecord == null
                && existing.EffectType == EffectType.AddAbility
                && existing.SourceAbilityEntityId == sourceAbilityEntityId)
            {
                updatedRecord = existing;
            }
        }

        if (updatedRecord != null)
        {
            updatedRecord.Level = accumulateLevel
                ? checked(updatedRecord.Level + level)
                : level;
            updatedRecord.ExpirationTarget = expiration.Target;
            updatedRecord.ExpirationEvent = expiration.Event;
            RelevelAbility(addedName, targetEntity, manager, template);

            return updatedRecord;
        }

        var reconcilesEveryCarrier = template is LeveledAbility
            || template?.Type == AbilityType.Race || template?.LevelExpression != null;
        var record = AddComponentEffectRecord(
            targetEntity, EffectType.AddAbility, addedName,
            carrierEntityId: reconcilesEveryCarrier
                ? 0
                : manager.AffectableAbilitiesIndex[(targetEntity.Id, addedName)]?.Id ?? 0,
            sourceAbilityEntityId: sourceAbilityEntityId,
            level: template?.LevelExpression == null || template.Type == AbilityType.Race ? level : 0,
            expirationTarget: expiration.Target,
            expirationEvent: expiration.Event);

        if (template?.LevelExpression != null)
        {
            activeEffects.RegisterDynamicAbility(template);
            if (template.Type != AbilityType.Race)
            {
                SynchronizeExpressionAbilityLevel(targetEntity, template, manager);
                return record;
            }
        }

        if (record.CarrierId == 0
            && !reconcilesEveryCarrier
            && embeddedAbility != null)
        {
            using var carrierReference = manager.CreateEntity();
            var ability = embeddedAbility.AddToEffect(carrierReference.Referenced);
            ability.Name ??= addedName;
            ability.OwnerId = targetEntity.Id;
            record.CarrierId = carrierReference.Referenced.Id;
        }

        RelevelAbility(addedName, targetEntity, manager, template);
        return record;
    }

    public ActiveComponentEffect AddAbilityEffect(
        GameEntity entity, Ability template, int level = 1, int sourceAbilityEntityId = 0)
    {
        if (level <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        return AddOrUpdateAbilityEffect(
            entity, template.Name, template, level, sourceAbilityEntityId,
            expiration: default, embeddedAbility: null,
            accumulateLevel: true);
    }

    public void RemoveAbility(
        GameEntity targetEntity, string abilityName, GameManager manager, int sourceAbilityEntityId = 0)
        => RemoveAbility(
            targetEntity, abilityName, manager, (int?)sourceAbilityEntityId);

    public void RemoveAbility(
        GameEntity targetEntity, string abilityName, GameManager manager, int? sourceAbilityEntityId)
    {
        var level = GetComponentEffectLevel(
            targetEntity, abilityName, EffectType.AddAbility, sourceAbilityEntityId);
        if (level != 0)
        {
            RemoveAbilityLevels(
                abilityName, level, sourceAbilityEntityId, targetEntity, manager);
        }
    }

    public static IReadOnlySet<string> GetPlayerRaceAbilityNames(GameEntity playerEntity)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var playerRaces = playerEntity.Game.Services.Data.PlayerRaces;
        foreach (var record in playerEntity.ActiveEffects!.ComponentEffects)
        {
            if (record is
                {
                    EffectType: EffectType.AddAbility,
                    Level: > 0,
                    TargetName: { } abilityName
                }
                && playerRaces.FindByIndex(abilityName) != null)
            {
                result.Add(abilityName);
            }
        }

        return result;
    }

    internal static void RemoveAbilityLevels(
        string abilityName, int levelsToRemove, int? sourceAbilityEntityId,
        GameEntity targetEntity, GameManager manager)
    {
        var records = GetOrderedComponentEffectRecords(
            abilityName, targetEntity, EffectType.AddAbility, sourceAbilityEntityId);
        if (records.Count != 0)
        {
            ValidateAbilityLevelChange(records[0], targetEntity);
        }

        List<ActiveComponentEffect>? removedRecords = null;
        for (var i = records.Count - 1; i >= 0 && levelsToRemove > 0; i--)
        {
            var record = records[i];
            if (record.Level <= levelsToRemove)
            {
                (removedRecords ??= []).Add(record);
                levelsToRemove -= record.Level;
            }
            else
            {
                record.Level -= levelsToRemove;
                levelsToRemove = 0;
            }
        }

        Debug.Assert(levelsToRemove == 0);
        if (removedRecords != null)
        {
            DetachComponentCarriers(removedRecords, targetEntity, manager);
        }
        else
        {
            manager.AbilityLevelSystem.RelevelAbility(
                abilityName, targetEntity, manager);
        }
    }

    internal static ActiveComponentEffect AddComponentEffectRecord(
        GameEntity targetEntity, EffectType effectType, string? targetName,
        int carrierEntityId = 0, int sourceAbilityEntityId = 0, int level = 1,
        int? conditionValue = null,
        int? expirationTarget = null,
        EffectDuration? expirationEvent = null)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        var record = activeEffects.AddComponentEffect(
            effectType, targetName, carrierEntityId, sourceAbilityEntityId,
            conditionValue, level, expirationTarget, expirationEvent);
        EffectApplicationSystem.AdvanceNextEffectTick(
            activeEffects,
            new EffectExpiration(expirationEvent, expirationTarget));
        return record;
    }

    // Re-instantiates a leveled granted ability at the level combined from its current directives when that
    // combined level has changed (a duplicate source was added or removed). Regenerating from the template
    // rebuilds the ability's per-level effects.
    public void RelevelAbility(
        string addedName,
        GameEntity targetEntity,
        GameManager manager,
        Ability? template = null)
    {
        var previousAbility = manager.AffectableAbilitiesIndex[
            (targetEntity.Id, addedName)]?.Ability;
        var previousLevel = previousAbility?.Level ?? 0;
        template ??= previousAbility?.Template;

        template = RelevelAbilityCore(
            addedName, targetEntity, manager, template);
        LogAbilityLevelChange(
            addedName,
            targetEntity,
            manager,
            template,
            previousLevel);
    }

    private Ability? RelevelAbilityCore(
        string addedName,
        GameEntity targetEntity,
        GameManager manager,
        Ability? template)
    {
        var statusMetadata = Effect.FindMetadata(addedName);
        var abilityEntity = manager.AffectableAbilitiesIndex[(targetEntity.Id, addedName)];
        if (template == null)
        {
            foreach (var record in targetEntity.ActiveEffects!.ComponentEffects.GetValues(addedName))
            {
                if (record.EffectType == EffectType.AddAbility
                    && manager.FindEntity(record.CarrierId)?.Ability?.Template is { } candidate)
                {
                    template = candidate;
                    break;
                }
            }
        }

        template ??= FindAbilityTemplate(targetEntity, addedName);
        if (template is LeveledAbility leveledTemplate)
        {
            ReconcileAbilityCarriers(addedName, targetEntity, leveledTemplate, manager);
            return template;
        }

        if (statusMetadata == null
            && template is { Type: not AbilityType.Race }
            && GetAbilityLevelReduction(targetEntity, addedName) == 0
            && abilityEntity?.Ability is { IsUsable: true } usableAbility)
        {
            usableAbility.Level = GetAbilityLevel(targetEntity, addedName);
            return template;
        }

        if (template != null)
        {
            if (ReconcileAbilityCarriers(addedName, targetEntity, template, manager))
            {
                return template;
            }
        }

        if (abilityEntity?.Ability is { } ability)
        {
            var effectiveLevel = GetAbilityLevel(targetEntity, addedName);
            ability.Level = effectiveLevel;
            ability.IsUsable = GetAbilityLevelReduction(targetEntity, addedName) == 0;
        }

        return template;
    }

    private void LogAbilityLevelChange(
        string addedName,
        GameEntity targetEntity,
        GameManager manager,
        Ability? template,
        int previousLevel)
    {
        if (template == null)
        {
            return;
        }

        var currentLevel = GetAbilityLevel(targetEntity, addedName);
        if (previousLevel == currentLevel)
        {
            return;
        }

        if (targetEntity.Player == null
            || !(template.Type.IsSpell()
                 || template.Type is AbilityType.Miracle
                     or AbilityType.Race
                     or AbilityType.Buff
                     or AbilityType.Boon
                     or AbilityType.Curse
                     or AbilityType.Trait
                     or AbilityType.Flaw
                     or AbilityType.Feat
                     or AbilityType.Fault
                     or AbilityType.Skill
                     or AbilityType.Faith))
        {
            return;
        }

        var displayName = template.Type == AbilityType.Race
            ? manager.Game.Services.Data.PlayerRaces.FindByIndex(addedName)?.Name
              ?? (addedName.EndsWith(" race", StringComparison.OrdinalIgnoreCase)
                  ? addedName[..^" race".Length]
                  : addedName)
            : addedName;
        manager.LoggingSystem.WriteLog(
            manager.Game.Services.Language.PlayerAbilityLevelChanged(
                displayName,
                template.Type,
                previousLevel,
                currentLevel),
            targetEntity,
            manager);
    }

    private static bool ReconcileAbilityCarriers(
        string addedName, GameEntity targetEntity, Ability template, GameManager manager)
    {
        var statusMetadata = Effect.FindMetadata(addedName);
        var records = GetOrderedComponentEffectRecords(
            addedName, targetEntity, EffectType.AddAbility,
            sourceAbilityEntityId: statusMetadata == null ? null : 0);
        if (records.Count == 0)
        {
            return false;
        }

        var (amount, secondaryAmount) = statusMetadata is { PropagatesAmountToStatusAbility: true }
            ? EffectApplicationSystem.GetPropagatedStatusAmounts(
                statusMetadata.EffectType, statusMetadata, targetEntity.ActiveEffects!)
            : (null, null);
        ReconcileComponentEffectCarriers(
            records, addedName, targetEntity, template, manager, amount, secondaryAmount);
        return true;
    }

    private static void ReconcileComponentEffectCarriers(
        IReadOnlyList<ActiveComponentEffect> records, string addedName, GameEntity targetEntity,
        Ability template, GameManager manager,
        string? propagatedAmount = null, string? propagatedSecondaryAmount = null)
    {
        var effectiveLevel = GetAbilityLevel(targetEntity, addedName);
        var decrease = GetAbilityLevelReduction(targetEntity, addedName);
        var activeLevel = template is not LeveledAbility
                          && template.LevelExpression == null
                          && template.Type != AbilityType.Race
                          && effectiveLevel == 0
                          && decrease == 0
            ? 1
            : Math.Max(0, effectiveLevel);
        var retainSuppressedCarrier = activeLevel == 0 && template.Type != AbilityType.Race;
        var usedCarrierIds = new HashSet<int>();
        var rebuildCarriers = new GameEntity?[records.Count];
        List<IDisposable>? rebuildReferences = null;
        var rebuildReferenceOwner = new object();
        void ConfigureRecordCarrier(
            GameEntity carrier, ActiveComponentEffect record, int carrierIndex,
            int firstLevel, int endpoint)
        {
            var lastLevel = Math.Min(endpoint, activeLevel);
            var name = carrierIndex == 0 ? addedName : $"{addedName}#{endpoint}";
            var carrierLevel = carrierIndex == 0 ? effectiveLevel : lastLevel;
            if (template.AddToEffect(
                carrier, firstLevel, lastLevel,
                propagatedAmount, propagatedSecondaryAmount) is { } ability)
            {
                ConfigureComponentCarrier(
                    ability, carrier, record, targetEntity, name, carrierLevel);
            }
        }

        try
        {
            var previousLevel = 0;
            for (var i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var rangeLevel = template is LeveledAbility
                    ? record.Level
                    : Math.Max(1, record.Level);
                var endpoint = checked(previousLevel + rangeLevel);
                var firstLevel = previousLevel + 1;
                var lastLevel = Math.Min(endpoint, activeLevel);
                var shouldCreateCarrier = retainSuppressedCarrier && i == 0
                                          || lastLevel >= firstLevel
                                          && template.ShouldCreateCarrier(
                                              firstLevel, lastLevel, activeLevel);
                if (record.CarrierId != 0
                    && !usedCarrierIds.Add(record.CarrierId))
                {
                    record.CarrierId = 0;
                }
                else if (record.CarrierId != 0)
                {
                    var carrierId = record.CarrierId;
                    if (manager.FindEntity(carrierId) is not { Ability: not null } carrier)
                    {
                        throw new InvalidOperationException(
                            $"Missing carrier {carrierId} for {addedName}.");
                    }

                    if (shouldCreateCarrier)
                    {
                        rebuildCarriers[i] = carrier;
                        (rebuildReferences ??= []).Add(carrier.AddReference(rebuildReferenceOwner));
                    }

                    record.CarrierId = 0;
                }

                previousLevel = endpoint;
            }

            previousLevel = 0;
            var carrierIndex = 0;
            for (var i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var rangeLevel = template is LeveledAbility
                    ? record.Level
                    : Math.Max(1, record.Level);
                var endpoint = checked(previousLevel + rangeLevel);
                var firstLevel = previousLevel + 1;
                var lastLevel = Math.Min(endpoint, activeLevel);
                var suppressedCarrier = retainSuppressedCarrier && i == 0;
                if (!suppressedCarrier
                    && (lastLevel < firstLevel
                        || !template.ShouldCreateCarrier(firstLevel, lastLevel, activeLevel)))
                {
                    previousLevel = endpoint;
                    continue;
                }

                if (record.CarrierId == 0)
                {
                    if (rebuildCarriers[i] is { } rebuildCarrier)
                    {
                        if (suppressedCarrier)
                        {
                            ConfigureComponentCarrier(
                                template.CreateAbility(rebuildCarrier, 0), rebuildCarrier,
                                record, targetEntity, addedName, 0);
                        }
                        else
                        {
                            ConfigureRecordCarrier(
                                rebuildCarrier, record, carrierIndex, firstLevel, endpoint);
                        }
                    }
                    else
                    {
                        using var carrierReference = manager.CreateEntity();
                        if (suppressedCarrier)
                        {
                            ConfigureComponentCarrier(
                                template.CreateAbility(carrierReference.Referenced, 0),
                                carrierReference.Referenced, record, targetEntity, addedName, 0);
                        }
                        else
                        {
                            ConfigureRecordCarrier(
                                carrierReference.Referenced, record, carrierIndex, firstLevel, endpoint);
                        }
                    }
                }
                else
                {
                    var carrier = manager.FindEntity(record.CarrierId)
                                  ?? throw new InvalidOperationException(
                                      $"Missing carrier {record.CarrierId} for {addedName}.");
                    if (suppressedCarrier)
                    {
                        ConfigureComponentCarrier(
                            template.CreateAbility(carrier, 0), carrier,
                            record, targetEntity, addedName, 0);
                    }
                    else
                    {
                        ConfigureRecordCarrier(
                            carrier, record, carrierIndex, firstLevel, endpoint);
                    }
                }

                carrierIndex++;
                previousLevel = endpoint;
            }
        }
        finally
        {
            if (rebuildReferences != null)
            {
                foreach (var reference in rebuildReferences)
                {
                    reference.Dispose();
                }
            }
        }
    }

    private static void ConfigureComponentCarrier(
        AbilityComponent ability, GameEntity carrier, ActiveComponentEffect record,
        GameEntity targetEntity, string name, int level)
    {
        ability.Name = name;
        ability.Level = level;
        ability.IsUsable = level > 0;
        ability.OwnerId = targetEntity.Id;
        record.CarrierId = carrier.Id;
    }

    /// <summary>
    ///     Tears down the ability additions and level decreases a source ability applied to the target.
    ///     If another source contributes the same ability, it retains the existing carrier; removing a
    ///     decrease restores the ranges it had suppressed.
    /// </summary>
    public void RemoveAbilityComponentEffects(GameEntity targetEntity, int sourceAbilityId, GameManager manager)
    {
        if (targetEntity.ActiveEffects is not { } activeEffects)
        {
            return;
        }

        Dictionary<string, List<ActiveComponentEffect>>? recordsByName = null;
        List<ActiveComponentEffect>? decreases = null;
        foreach (var record in activeEffects.ComponentEffects)
        {
            if (record.SourceAbilityEntityId != sourceAbilityId)
            {
                continue;
            }

            if (record.EffectType == EffectType.DecreaseAbilityLevel)
            {
                (decreases ??= []).Add(record);
                continue;
            }

            if (record.EffectType != EffectType.AddAbility)
            {
                continue;
            }

            var targetName = record.TargetName!;
            recordsByName ??= [];
            if (!recordsByName.TryGetValue(targetName, out var records))
            {
                records = [];
                recordsByName.Add(targetName, records);
            }

            records.Add(record);
        }

        if (recordsByName != null)
        {
            foreach (var records in recordsByName.Values)
            {
                DetachComponentCarriers(records, targetEntity, manager);
            }
        }

        if (decreases != null)
        {
            foreach (var decrease in decreases)
            {
                DetachComponentCarrier(decrease, targetEntity, manager);
            }
        }
    }

    // Drops a structural directive. The component is detached when no matching record still references its
    // carrier; leveled AddAbility records then reconcile their remaining carriers and authoritative level.
    public static void DetachComponentCarrier(
        ActiveComponentEffect record, GameEntity targetEntity, GameManager manager)
        => DetachComponentCarriers([record], targetEntity, manager);

    private static void DetachComponentCarriers(
        IReadOnlyList<ActiveComponentEffect> detachedRecords,
        GameEntity targetEntity,
        GameManager manager)
    {
        Debug.Assert(detachedRecords.Count != 0);
        var records = targetEntity.ActiveEffects?.ComponentEffects;
        if (records == null)
        {
            return;
        }

        var firstRecord = detachedRecords[0];
        var targetName = firstRecord.TargetName;
        var effectType = firstRecord.EffectType;
        var conditionValue = firstRecord.ConditionValue;
        var sourceAbilityEntityId = firstRecord.SourceAbilityEntityId;
        Ability? abilityTemplate = null;
        var previousAbilityLevel = 0;
        if (effectType == EffectType.AddAbility)
        {
            var previousAbility = manager.AffectableAbilitiesIndex[
                (targetEntity.Id, targetName!)]?.Ability;
            previousAbilityLevel = previousAbility?.Level ?? 0;
            abilityTemplate = previousAbility?.Template;
            if (abilityTemplate == null)
            {
                for (var i = 0; i < detachedRecords.Count; i++)
                {
                    if (manager.FindEntity(detachedRecords[i].CarrierId)?.Ability?.Template is { } candidate)
                    {
                        abilityTemplate = candidate;
                        break;
                    }
                }

                abilityTemplate ??= FindAbilityTemplate(targetEntity, targetName!);
            }

            HashSet<int>? queuedCarrierIds = null;
            for (var i = 0; i < detachedRecords.Count; i++)
            {
                var carrierId = detachedRecords[i].CarrierId;
                if (carrierId == 0
                    || queuedCarrierIds?.Contains(carrierId) == true
                    || records.Any(record => record.CarrierId == carrierId
                                             && !detachedRecords.Contains(record))
                    || manager.FindEntity(carrierId)?.Ability is not
                        { IsActive: true, Activation: var activation } ability
                    || (activation & ActivationType.Continuous) == 0)
                {
                    continue;
                }

                (queuedCarrierIds ??= []).Add(carrierId);
                manager.Enqueue(DeactivateAbilityMessage.Create(ability, targetEntity, manager));
            }
        }

        for (var i = 0; i < detachedRecords.Count; i++)
        {
            var record = detachedRecords[i];
            Debug.Assert(record.TargetName == targetName && record.EffectType == effectType);
            records.Remove(record);
        }

        if (effectType == EffectType.AddAbility)
        {
            if (abilityTemplate?.LevelExpression != null
                && sourceAbilityEntityId != 0
                && FindComponentEffect(targetEntity, EffectType.AddAbility, targetName!) == null)
            {
                targetEntity.ActiveEffects!.UnregisterDynamicAbility(abilityTemplate);
            }

            abilityTemplate = manager.AbilityLevelSystem.RelevelAbilityCore(
                targetName!, targetEntity, manager,
                abilityTemplate);
            manager.AbilityLevelSystem.LogAbilityLevelChange(
                targetName!,
                targetEntity,
                manager,
                abilityTemplate,
                previousAbilityLevel);
        }
        else if (effectType == EffectType.DecreaseAbilityLevel)
        {
            manager.AbilityLevelSystem.ReconcileDecreasedAbilities(
                targetName, conditionValue, sourceAbilityEntityId, targetEntity, manager);
        }
    }

    internal void ApplyDecreaseAbilityLevel(in EffectApplicationContext context, int amount)
    {
        if (context.Pretend
            || context.TargetEntity?.ActiveEffects is not { } activeEffects)
        {
            return;
        }

        var targetEntity = context.TargetEntity;
        var effect = context.Effect.Component;
        var abilityName = effect.TargetName;
        var abilityType = DecreaseAbilityLevel.DecodeAbilityTypeTarget(effect.TargetEntityId);
        Debug.Assert((abilityName == null) != (abilityType == null));
        amount = GetSourceAbilityDecreaseAmount(context, abilityName, abilityType, amount);

        var sourceAbilityId = effect.ContainingAbilityId ?? 0;
        var targetsRace = abilityType == AbilityType.Race
                          || abilityName != null
                          && GetAbilityType(targetEntity, abilityName) == AbilityType.Race;
        if (effect.Duration == EffectDuration.Instant)
        {
            RemoveMatchingAbilityRecords(
                targetEntity, sourceAbilityId, abilityName, abilityType, context.Manager);
            if (targetsRace && !HasActiveRace(targetEntity))
            {
                targetEntity.Being!.HitPoints = 0;
            }

            return;
        }

        var expiration = EffectApplicationSystem.GetExpiration(context);
        var updatedRecord = FindDecreaseRecord(
            targetEntity, abilityName, abilityType, sourceAbilityId);
        var previousLevel = 0;
        var previousExpiration = default(EffectExpiration);
        if (updatedRecord != null)
        {
            previousLevel = updatedRecord.Level;
            previousExpiration = new EffectExpiration(
                updatedRecord.ExpirationEvent, updatedRecord.ExpirationTarget);
            updatedRecord.Level = amount;
            updatedRecord.ExpirationEvent = expiration.Event;
            updatedRecord.ExpirationTarget = expiration.Target;
            ReconcileDecreasedAbilities(updatedRecord, targetEntity, context.Manager);
        }

        var record = updatedRecord;
        if (record == null)
        {
            record = AddComponentEffectRecord(
                targetEntity, EffectType.DecreaseAbilityLevel, abilityName,
                sourceAbilityEntityId: sourceAbilityId, level: amount,
                conditionValue: (int?)abilityType,
                expirationTarget: expiration.Target, expirationEvent: expiration.Event);
            ReconcileDecreasedAbilities(record, targetEntity, context.Manager);
        }

        if (!targetsRace || HasActiveRace(targetEntity))
        {
            return;
        }

        if (updatedRecord == null)
        {
            var recordTargetName = record.TargetName;
            var recordConditionValue = record.ConditionValue;
            var recordSourceAbilityEntityId = record.SourceAbilityEntityId;
            activeEffects.ComponentEffects.Remove(record);
            ReconcileDecreasedAbilities(
                recordTargetName, recordConditionValue, recordSourceAbilityEntityId,
                targetEntity, context.Manager);
        }
        else
        {
            record.Level = previousLevel;
            record.ExpirationEvent = previousExpiration.Event;
            record.ExpirationTarget = previousExpiration.Target;
            ReconcileDecreasedAbilities(record, targetEntity, context.Manager);
        }
        throw new InvalidOperationException(
            $"{EffectType.DecreaseAbilityLevel} cannot suppress every race on entity {targetEntity.Id}. ");
    }

    private static bool HasActiveRace(GameEntity targetEntity)
        => targetEntity.Being!.Races.Any(race => race.Ability is { Level: > 0, IsUsable: true });

    private static void ValidateAbilityLevelChange(
        ActiveComponentEffect record, GameEntity owner)
    {
        if (record.EffectType == EffectType.AddAbility
            && record.TargetName is { } name
            && (owner.Manager.AffectableAbilitiesIndex[(owner.Id, name)]?.Ability?.Template
                ?? FindAbilityTemplate(owner, name))?.LevelExpression != null)
        {
            throw new InvalidOperationException($"The level of ability '{name}' is controlled by its level expression.");
        }
    }

    private static ActiveComponentEffect? FindDecreaseRecord(
        GameEntity targetEntity, string? abilityName,
        AbilityType? abilityType, int sourceAbilityId)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        ActiveComponentEffect? result = null;
        if (abilityName != null)
        {
            foreach (var record in activeEffects.ComponentEffects.GetValues(abilityName))
            {
                ValidateAbilityLevelChange(record, targetEntity);
                if (result == null
                    && record.EffectType == EffectType.DecreaseAbilityLevel
                    && record.SourceAbilityEntityId == sourceAbilityId
                    && record.ConditionValue == null)
                {
                    result = record;
                }
            }

            return result;
        }

        foreach (var record in activeEffects.ComponentEffects)
        {
            if (record.EffectType == EffectType.AddAbility
                && record.TargetName is { } targetName
                && GetAbilityType(targetEntity, targetName) == abilityType)
            {
                ValidateAbilityLevelChange(record, targetEntity);
            }

            if (result == null
                && record.EffectType == EffectType.DecreaseAbilityLevel
                && record.SourceAbilityEntityId == sourceAbilityId
                && GetDecreasedAbilityType(record) == abilityType)
            {
                result = record;
            }
        }

        return result;
    }

    private static void RemoveMatchingAbilityRecords(
        GameEntity targetEntity, int sourceAbilityId, string? abilityName,
        AbilityType? abilityType, GameManager manager)
    {
        var componentEffects = targetEntity.ActiveEffects!.ComponentEffects;
        if (abilityName != null)
        {
            List<ActiveComponentEffect>? namedRecords = null;
            foreach (var record in componentEffects.GetValues(abilityName))
            {
                ValidateAbilityLevelChange(record, targetEntity);
                if (record.EffectType != EffectType.AddAbility
                    || IsAbilityNonDecreasing(targetEntity, abilityName)
                    || IsSourceAbility(sourceAbilityId, abilityName, manager))
                {
                    continue;
                }

                (namedRecords ??= []).Add(record);
            }

            if (namedRecords != null)
            {
                DetachComponentCarriers(namedRecords, targetEntity, manager);
            }

            return;
        }

        Dictionary<string, List<ActiveComponentEffect>>? matchingRecords = null;
        foreach (var record in componentEffects)
        {
            if (record.EffectType != EffectType.AddAbility
                || record.TargetName is not { } targetName
                || IsSourceAbility(sourceAbilityId, targetName, manager)
                || abilityType != null && GetAbilityType(targetEntity, targetName) != abilityType)
            {
                continue;
            }

            ValidateAbilityLevelChange(record, targetEntity);
            if (IsAbilityNonDecreasing(targetEntity, targetName))
            {
                continue;
            }

            matchingRecords ??= [];
            if (!matchingRecords.TryGetValue(targetName, out var records))
            {
                records = [];
                matchingRecords.Add(targetName, records);
            }

            records.Add(record);
        }

        if (matchingRecords == null)
        {
            return;
        }

        foreach (var records in matchingRecords.Values)
        {
            DetachComponentCarriers(records, targetEntity, manager);
        }
    }

    private int GetSourceAbilityDecreaseAmount(
        in EffectApplicationContext context, string? abilityName,
        AbilityType? abilityType, int fallbackAmount)
    {
        if (context.Effect.Component.ContainingAbility?.Ability?.Effects is not { } sourceEffects)
        {
            return fallbackAmount;
        }

        var total = 0;
        foreach (var effectEntity in sourceEffects)
        {
            if (effectEntity.Effect is not
                {
                    EffectType: EffectType.DecreaseAbilityLevel
                } decrease
                || decrease.TargetName != abilityName
                || DecreaseAbilityLevel.DecodeAbilityTypeTarget(decrease.TargetEntityId) != abilityType)
            {
                continue;
            }

            total = checked(total + context.System.GetAmount(
                decrease, context.ActivatorEntity, context.TargetEntity));
        }

        return total == 0 ? fallbackAmount : total;
    }

    private void ReconcileDecreasedAbilities(
        ActiveComponentEffect decrease, GameEntity targetEntity, GameManager manager)
        => ReconcileDecreasedAbilities(
            decrease.TargetName, decrease.ConditionValue, decrease.SourceAbilityEntityId,
            targetEntity, manager);

    private void ReconcileDecreasedAbilities(
        string? targetName, int? conditionValue, int sourceAbilityEntityId,
        GameEntity targetEntity, GameManager manager)
    {
        if (conditionValue is not { } targetTypeValue)
        {
            RelevelAbility(targetName!, targetEntity, manager);
            return;
        }

        var targetType = (AbilityType)targetTypeValue;

        HashSet<string>? affectedAbilityNames = null;
        foreach (var record in targetEntity.ActiveEffects!.ComponentEffects)
        {
            if (record.EffectType != EffectType.AddAbility
                || record.TargetName is not { } abilityName
                || GetAbilityType(targetEntity, abilityName) != targetType
                || IsSourceAbility(sourceAbilityEntityId, abilityName, manager))
            {
                continue;
            }

            (affectedAbilityNames ??= []).Add(abilityName);
        }

        if (affectedAbilityNames == null)
        {
            return;
        }

        foreach (var abilityName in affectedAbilityNames)
        {
            RelevelAbility(abilityName, targetEntity, manager);
        }
    }

    public static int GetAbilityLevel(GameEntity targetEntity, string abilityName)
    {
        var rawLevel = GetComponentEffectLevel(
            targetEntity, abilityName, EffectType.AddAbility);
        var decrease = GetAbilityLevelReduction(targetEntity, abilityName);
        return checked(rawLevel - decrease);
    }

    internal static int GetComponentEffectLevel(
        GameEntity targetEntity, string addedName, EffectType effectType,
        int? sourceAbilityEntityId = null)
    {
        var level = 0;
        foreach (var record in targetEntity.ActiveEffects!.ComponentEffects.GetValues(addedName))
        {
            if (MatchesComponentEffect(
                    record, effectType, sourceAbilityEntityId))
            {
                level = checked(level + record.Level);
            }
        }

        return level;
    }

    public static int GetAbilityLevelReduction(GameEntity targetEntity, string abilityName)
    {
        if (IsAbilityNonDecreasing(targetEntity, abilityName))
        {
            return 0;
        }

        var activeEffects = targetEntity.ActiveEffects!;
        var manager = targetEntity.Manager;
        var abilityType = GetAbilityType(targetEntity, abilityName);
        var decrease = 0;
        foreach (var record in activeEffects.ComponentEffects.GetValues(abilityName))
        {
            if (record.EffectType != EffectType.DecreaseAbilityLevel
                || IsTypeWideAbilityDecrease(record)
                || IsSourceAbility(record, abilityName, manager))
            {
                continue;
            }

            decrease = checked(decrease + record.Level);
        }

        if (abilityType != null)
        {
            foreach (var record in activeEffects.ComponentEffects)
            {
                if (record.EffectType != EffectType.DecreaseAbilityLevel
                    || GetDecreasedAbilityType(record) != abilityType
                    || IsSourceAbility(record, abilityName, manager))
                {
                    continue;
                }

                decrease = checked(decrease + record.Level);
            }
        }

        return decrease;
    }

    private static bool IsAbilityNonDecreasing(GameEntity owner, string abilityName)
        => (owner.Manager.AffectableAbilitiesIndex[(owner.Id, abilityName)]?.Ability?.Template
            ?? FindAbilityTemplate(owner, abilityName)) is { NonDecreasingLevel: true };

    private static AbilityType? GetDecreasedAbilityType(ActiveComponentEffect record)
        => IsTypeWideAbilityDecrease(record)
            ? (AbilityType)record.ConditionValue!.Value
            : null;

    private static bool IsTypeWideAbilityDecrease(ActiveComponentEffect record)
        => record.EffectType == EffectType.DecreaseAbilityLevel
           && record.ConditionValue != null;

    private static bool MatchesComponentEffect(
        ActiveComponentEffect record, EffectType effectType,
        int? sourceAbilityEntityId)
        => record.EffectType == effectType
           && (sourceAbilityEntityId == null
               || record.SourceAbilityEntityId == sourceAbilityEntityId);

    private static AbilityType? GetAbilityType(GameEntity targetEntity, string abilityName)
        => targetEntity.Manager.AffectableAbilitiesIndex[(targetEntity.Id, abilityName)]?.Ability?.Type
           ?? FindAbilityTemplate(targetEntity, abilityName)?.Type;

    private static Ability? FindAbilityTemplate(GameEntity owner, string abilityName)
        => Ability.FindTemplate(owner.Manager, abilityName, item: false, owner: owner);

    private static bool IsSourceAbility(ActiveComponentEffect record, string abilityName, GameManager manager)
        => IsSourceAbility(record.SourceAbilityEntityId, abilityName, manager);

    private static bool IsSourceAbility(int sourceAbilityId, string abilityName, GameManager manager)
    {
        if (sourceAbilityId == 0
            || manager.FindEntity(sourceAbilityId)?.Ability is not { } sourceAbility)
        {
            return false;
        }

        return (sourceAbility.Template?.Name ?? sourceAbility.Name) == abilityName;
    }

    // Folds the level contributions of every AddAbility directive for the named ability on the target into
    // the ability's combined level.
    internal static List<ActiveComponentEffect> GetOrderedComponentEffectRecords(
        string addedName, GameEntity targetEntity, EffectType effectType,
        int? sourceAbilityEntityId = null)
    {
        var contributions = new List<ActiveComponentEffect>();
        foreach (var record in targetEntity.ActiveEffects!.ComponentEffects.GetValues(addedName))
        {
            if (MatchesComponentEffect(
                    record, effectType, sourceAbilityEntityId))
            {
                contributions.Add(record);
            }
        }

        contributions.Sort(static (x, y) => x.InstanceId.CompareTo(y.InstanceId));

        return contributions;
    }

    // Finds the structural directive that attached the named component to the target, or null.
    internal static ActiveComponentEffect? FindComponentEffect(
        GameEntity targetEntity, EffectType effectType, string addedName,
        int? sourceAbilityEntityId = null)
    {
        var records = targetEntity.ActiveEffects!.ComponentEffects.GetValues(addedName);
        for (var index = 0; index < records.Count; index++)
        {
            var record = records[index];
            if (MatchesComponentEffect(
                    record, effectType, sourceAbilityEntityId))
            {
                return record;
            }
        }

        return null;
    }
}
