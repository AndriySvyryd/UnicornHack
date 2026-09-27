using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Systems.Effects;

public partial class EffectApplicationSystem
{
    internal void ApplyAddAbility(
        in EffectApplicationContext context, string addedName, int level)
    {
        if (context.TargetEntity?.ActiveEffects == null)
        {
            return;
        }

        var effect = context.Effect.Component;
        var expiration = GetExpiration(context);

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
            resolvedTemplate ??= FindAbilityTemplate(manager, addedName)
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
        foreach (var existing in activeEffects.ComponentEffects.GetValues(addedName))
        {
            if (!IsUnconditionalAbilityGrant(existing)
                || existing.SourceAbilityEntityId != sourceAbilityEntityId)
            {
                continue;
            }

            existing.Level = accumulateLevel
                ? checked(existing.Level + level)
                : level;
            existing.ExpirationTarget = expiration.Target;
            existing.ExpirationEvent = expiration.Event;
            RelevelAbility(addedName, targetEntity, manager, template);

            return existing;
        }

        var reconcilesEveryCarrier = template is LeveledAbility || template?.Type == AbilityType.Race;
        var record = AddComponentEffectRecord(
            targetEntity, EffectType.AddAbility, addedName,
            carrierEntityId: reconcilesEveryCarrier
                ? 0
                : manager.AffectableAbilitiesIndex[(targetEntity.Id, addedName)]?.Id ?? 0,
            sourceAbilityEntityId: sourceAbilityEntityId, level: level,
            expirationTarget: expiration.Target,
            expirationEvent: expiration.Event);

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
            targetEntity, abilityName, EffectType.AddAbility, sourceAbilityEntityId,
            onlyUnconditional: true);
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
                    ConditionValue: null,
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

    private static void RemoveAbilityLevels(
        string abilityName, int levelsToRemove, int? sourceAbilityEntityId,
        GameEntity targetEntity, GameManager manager)
    {
        var records = GetOrderedComponentEffectRecords(
            abilityName, targetEntity, EffectType.AddAbility, sourceAbilityEntityId,
            onlyUnconditional: true);
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
            manager.EffectApplicationSystem.RelevelAbility(
                abilityName, targetEntity, manager);
        }
    }

    private static ActiveComponentEffect AddComponentEffectRecord(
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
        AdvanceNextEffectTick(
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

        template ??= FindAbilityTemplate(manager, addedName);
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
            ? GetPropagatedStatusAmounts(
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
                          && effectiveLevel == 0
                          && decrease == 0
            ? 1
            : Math.Max(0, effectiveLevel);
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
                var shouldCreateCarrier = activeLevel == 0 && i == 0
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
                var suppressedCarrier = activeLevel == 0 && i == 0;
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

                abilityTemplate ??= FindAbilityTemplate(manager, targetName!);
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
            abilityTemplate = manager.EffectApplicationSystem.RelevelAbilityCore(
                targetName!, targetEntity, manager,
                abilityTemplate);
            manager.EffectApplicationSystem.LogAbilityLevelChange(
                targetName!,
                targetEntity,
                manager,
                abilityTemplate,
                previousAbilityLevel);
        }
        else if (effectType == EffectType.DecreaseAbilityLevel)
        {
            manager.EffectApplicationSystem.ReconcileDecreasedAbilities(
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
            if (targetsRace && targetEntity.Being!.Races.Count == 0)
            {
                targetEntity.Being.HitPoints = 0;
            }

            return;
        }

        var expiration = GetExpiration(context);
        var updatedRecord = FindDecreaseRecord(
            activeEffects, abilityName, abilityType, sourceAbilityId);
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

        if (!targetsRace || targetEntity.Being!.Races.Count != 0)
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

    private static ActiveComponentEffect? FindDecreaseRecord(
        ActiveEffectsComponent activeEffects, string? abilityName,
        AbilityType? abilityType, int sourceAbilityId)
    {
        if (abilityName != null)
        {
            foreach (var record in activeEffects.ComponentEffects.GetValues(abilityName))
            {
                if (record.EffectType == EffectType.DecreaseAbilityLevel
                    && record.SourceAbilityEntityId == sourceAbilityId
                    && record.ConditionValue == null)
                {
                    return record;
                }
            }

            return null;
        }

        foreach (var record in activeEffects.ComponentEffects)
        {
            if (record.EffectType == EffectType.DecreaseAbilityLevel
                && record.SourceAbilityEntityId == sourceAbilityId
                && GetDecreasedAbilityType(record) == abilityType)
            {
                return record;
            }
        }

        return null;
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
                if (!IsUnconditionalAbilityGrant(record)
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
            if (!IsUnconditionalAbilityGrant(record)
                || record.TargetName is not { } targetName
                || IsSourceAbility(sourceAbilityId, targetName, manager)
                || abilityType != null && FindAbilityTemplate(manager, targetName)?.Type != abilityType)
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

            total = checked(total + GetAmount(
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
                || FindAbilityTemplate(manager, abilityName)?.Type != targetType
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

    private static int GetComponentEffectLevel(
        GameEntity targetEntity, string addedName, EffectType effectType,
        int? sourceAbilityEntityId = null, bool onlyUnconditional = false)
    {
        var level = 0;
        foreach (var record in targetEntity.ActiveEffects!.ComponentEffects.GetValues(addedName))
        {
            if (MatchesComponentEffect(
                    record, effectType, sourceAbilityEntityId, onlyUnconditional))
            {
                level = checked(level + record.Level);
            }
        }

        return level;
    }

    public static int GetAbilityLevelReduction(GameEntity targetEntity, string abilityName)
    {
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

    private static AbilityType? GetDecreasedAbilityType(ActiveComponentEffect record)
        => IsTypeWideAbilityDecrease(record)
            ? (AbilityType)record.ConditionValue!.Value
            : null;

    private static bool IsUnconditionalAbilityGrant(ActiveComponentEffect record)
        => record is { EffectType: EffectType.AddAbility, ConditionValue: null };

    private static bool IsTypeWideAbilityDecrease(ActiveComponentEffect record)
        => record.EffectType == EffectType.DecreaseAbilityLevel
           && record.ConditionValue != null;

    private static bool MatchesComponentEffect(
        ActiveComponentEffect record, EffectType effectType,
        int? sourceAbilityEntityId, bool onlyUnconditional)
        => record.EffectType == effectType
           && (!onlyUnconditional || record.ConditionValue == null)
           && (sourceAbilityEntityId == null
               || record.SourceAbilityEntityId == sourceAbilityEntityId);

    private static AbilityType? GetAbilityType(GameEntity targetEntity, string abilityName)
        => targetEntity.Manager.AffectableAbilitiesIndex[(targetEntity.Id, abilityName)]?.Ability?.Type
           ?? FindAbilityTemplate(targetEntity.Manager, abilityName)?.Type;

    private static Ability? FindAbilityTemplate(GameManager manager, string abilityName)
        => Ability.FindTemplate(manager, abilityName, item: false);

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

    public static IEnumerable<AbilityComponent> GetAbilitiesAffectingProperty(
        GameEntity targetEntity, string propertyName, AbilityType? abilityType = null)
    {
        var propertyEffects = targetEntity.ActiveEffects!.PropertyEffects.GetValues(propertyName);
        for (var i = 0; i < propertyEffects.Count; i++)
        {
            var propertyEffect = propertyEffects[i];
            if (propertyEffect.SourceAbilityEntityId is not { } sourceAbilityId
                || targetEntity.Manager.FindEntity(sourceAbilityId)?.Ability is not { } ability
                || abilityType != null && ability.Type != abilityType)
            {
                continue;
            }

            yield return ability;
        }
    }

    // Folds the level contributions of every AddAbility directive for the named ability on the target into
    // the ability's combined level.
    private static List<ActiveComponentEffect> GetOrderedComponentEffectRecords(
        string addedName, GameEntity targetEntity, EffectType effectType,
        int? sourceAbilityEntityId = null, bool onlyUnconditional = false)
    {
        var contributions = new List<ActiveComponentEffect>();
        foreach (var record in targetEntity.ActiveEffects!.ComponentEffects.GetValues(addedName))
        {
            if (MatchesComponentEffect(
                    record, effectType, sourceAbilityEntityId, onlyUnconditional))
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
        int? sourceAbilityEntityId = null, bool onlyUnconditional = false)
    {
        foreach (var record in targetEntity.ActiveEffects!.ComponentEffects.GetValues(addedName))
        {
            if (MatchesComponentEffect(
                    record, effectType, sourceAbilityEntityId, onlyUnconditional))
            {
                return record;
            }
        }

        return null;
    }

    /// <summary>
    ///     Transforms <paramref name="targetEntity" /> into <paramref name="form" /> for
    ///     <paramref name="duration" /> ticks: lays the form's stats on as timed Override instances
    ///     (<see cref="Creature.ApplyFormOverride" />) and records a property-less identity marker
    ///     so the being resolves as the form (<see cref="BeingComponent.Species" />). Every
    ///     instance self-expires together, reverting the being to its own form. A polymorph replaces any
    ///     transformation already in force rather than stacking with it.
    /// </summary>
    public void ApplyPolymorph(Creature form, GameEntity targetEntity, int duration, GameManager manager)
    {
        var activeEffects = targetEntity.ActiveEffects!;

        // Replace, don't stack: tear down any transformation already in force before laying on the new one.
        RemoveExistingPolymorph(targetEntity, manager);

        var being = targetEntity.Being!;
        var formAbilityTemplate = form.GetRaceAbilityTemplate();
        foreach (var raceEntity in being.Races)
        {
            if (ReferenceEquals(raceEntity.Ability?.Template, formAbilityTemplate))
            {
                return;
            }
        }

        var expirationTick = manager.Game.CurrentTick + duration;

        // Identity marker: a property-less Polymorph instance carrying the form's loader id, read by
        // BeingComponent.Species. It self-expires alongside the stat overrides.
        activeEffects.AddPropertyEffect(
            EffectType.Polymorph,
            magnitudeSecondary: Creature.Loader.GetId(form.Name!),
            expirationTarget: expirationTick,
            expirationEvent: EffectDuration.UntilTimeout);

        AdvanceNextEffectTick(
            activeEffects,
            new EffectExpiration(EffectDuration.UntilTimeout, expirationTick));

        form.ApplyFormOverride(targetEntity, duration, manager);

        // The form's own Race ability carries the suppressor, so DecreaseAbilityLevel's source exclusion
        // keeps the form active while lowering every other Race below zero. Removing the form carrier
        // removes this source-linked decrement and restores the original races automatically.
        var formRecord = AddAbilityEffect(targetEntity, formAbilityTemplate);
        var formAbility = manager.FindEntity(formRecord.CarrierId)!.Ability!;
        var raceSuppression = new DecreaseAbilityLevel
        {
            TargetActivator = true,
            AbilityType = AbilityType.Race,
            Amount = "3",
            ContainingAbility = formAbility.Template!
        };
        raceSuppression.AddToAbility(formAbility, manager);
    }

    // Ends a polymorph transformation by removing the form race carrier. Its source-linked Race decrement
    // disappears during deactivation, restoring the original races; stat overrides expire separately.
    private void EndPolymorph(GameEntity entity, int formId, GameManager manager)
    {
        if (Creature.Loader.FindById(formId) is not { } form
            || FindRaceComponentEffect(entity, form.GetRaceAbilityName()) is not { } formRecord)
        {
            return;
        }

        var formCarrierId = formRecord.CarrierId;

        // Detaching the carrier alone does not drop the abilities it granted (their AddAbility directives
        // live on the being, sourced by the carrier), so remove those first, then detach the carrier to
        // clear the form's being.Races identity.
        RemoveAbilityComponentEffects(entity, formCarrierId, manager);
        DetachComponentCarrier(formRecord, entity, manager);
    }

    private static ActiveComponentEffect? FindRaceComponentEffect(
        GameEntity entity, string raceAbilityName)
    {
        foreach (var record in entity.ActiveEffects!.ComponentEffects.GetValues(raceAbilityName))
        {
            if (record.EffectType == EffectType.AddAbility
                && record.SourceAbilityEntityId == 0)
            {
                return record;
            }
        }

        return null;
    }

    // Tears down every polymorph transformation currently in force on the entity so a freshly applied one
    // replaces rather than stacks with it: ends each form (dropping its granted abilities and carrier) and
    // removes the transformation's instances, reverting the stats they forced. The being's own race
    // abilities are deliberately left suppressed - the replacement keeps them so - and are restored only when
    // a transformation lapses on its own.
    private void RemoveExistingPolymorph(GameEntity entity, GameManager manager)
    {
        if (entity.Being == null)
        {
            return;
        }

        var activeEffects = entity.ActiveEffects!;

        var instances = activeEffects.PropertyEffects;

        List<(int FormId, int? ExpirationTarget)>? forms = null;
        foreach (var instance in instances)
        {
            if (instance is { EffectType: EffectType.Polymorph, TargetName: null })
            {
                (forms ??= []).Add((instance.MagnitudeSecondary, instance.ExpirationTarget));
            }
        }

        if (forms == null)
        {
            return;
        }

        foreach (var form in forms)
        {
            EndPolymorph(entity, form.FormId, manager);
        }

        // Drop the transformation's own instances (identity markers and stat overrides), reverting the
        // stats they forced.
        RemoveMatchingPropertyEffects(
            activeEffects, entity,
            instance => instance.EffectType == EffectType.Polymorph
                        || instance is
                        {
                            EffectType: EffectType.ChangeProperty,
                            CombinationFunction: ValueCombinationFunction.Override,
                            SourceAbilityEntityId: null,
                            SourceAbilityType: AbilityType.Debuff,
                            ExpirationEvent: EffectDuration.UntilTimeout
                        }
                        && forms.Any(form => form.ExpirationTarget == instance.ExpirationTarget),
            manager);
    }
}
