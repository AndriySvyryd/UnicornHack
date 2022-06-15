using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Items;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects;

public class EffectApplicationSystem :
    IGameSystem<AbilityActivatedMessage>,
    IGameSystem<ApplyEffectMessage>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<EntityRemovedMessage<GameEntity>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int?>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, string>>
{
    public const string InnateAbilityName = "innate";

    private static readonly ParameterExpression EffectParameter =
        Expression.Parameter(typeof(GameEntity), name: "effect");

    private static readonly ParameterExpression ActivatorParameter =
        Expression.Parameter(typeof(GameEntity), name: "activator");

    private static readonly EffectExpressionVisitor _translator =
        new(new[] { EffectParameter, ActivatorParameter });

    private static readonly MethodInfo _requirementsModifierMethod = typeof(EffectApplicationSystem)
        .GetMethod(nameof(GetRequirementsModifier), BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo _mightModifierMethod = typeof(EffectApplicationSystem)
        .GetMethod(nameof(GetMightModifier), BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo _focusModifierMethod = typeof(EffectApplicationSystem)
        .GetMethod(nameof(GetFocusModifier), BindingFlags.NonPublic | BindingFlags.Static);

    public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager manager)
    {
        var effectsAppliedMessage = ApplyEffects(message, pretend: false);
        if (effectsAppliedMessage != null)
        {
            manager.Enqueue(effectsAppliedMessage);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private EffectsAppliedMessage ApplyEffects(AbilityActivatedMessage message, bool pretend)
    {
        if (message.TargetEntity == null
            || message.TargetEntity.Being?.IsAlive == false)
        {
            return null;
        }

        var manager = message.ActivatorEntity.Manager;
        var effectsAppliedMessage = EffectsAppliedMessage.Create(manager);
        effectsAppliedMessage.ActivatorEntity = message.ActivatorEntity;
        effectsAppliedMessage.TargetEntity = message.TargetEntity;
        effectsAppliedMessage.AbilityEntity = message.AbilityEntity;
        effectsAppliedMessage.AbilityTrigger = message.Trigger;
        effectsAppliedMessage.SuccessfulApplication = message.Outcome == ApplicationOutcome.Success;

        // TODO: Group effects by duration so they can be applied in proper order
        // TODO: Fire a message for effects that are DuringApplication, so they get unapplied as soon as the previous messages are processed
        Dictionary<EffectType, (int Damage, GameEntity EffectEntity)> damageEffects = null;
        foreach (var effectEntity in message.EffectsToApply)
        {
            var effect = effectEntity.Effect;
            if (!effectsAppliedMessage.SuccessfulApplication
                && effect.EffectType != EffectType.Activate
                && effect.EffectType != EffectType.Move)
            {
                continue;
            }

            switch (effect.EffectType)
            {
                case EffectType.Burn:
                case EffectType.Corrode:
                case EffectType.Blight:
                case EffectType.Freeze:
                case EffectType.Bleed:
                case EffectType.Shock:
                case EffectType.Soak:
                case EffectType.LightDamage:
                case EffectType.PsychicDamage:
                case EffectType.SonicDamage:
                case EffectType.Wither:
                case EffectType.PhysicalDamage:
                case EffectType.DrainEnergy:
                case EffectType.DrainLife:
                    Debug.Assert(effect.Duration == EffectDuration.Instant);

                    if (damageEffects == null)
                    {
                        damageEffects = new Dictionary<EffectType, (int Damage, GameEntity EffectEntity)>();
                    }

                    var key = effect.EffectType;
                    if (!damageEffects.TryGetValue(key, out var previousDamage))
                    {
                        previousDamage = (0, null);
                    }

                    // TODO: Take effectComponent.CombinationFunction into account
                    damageEffects[key] = (GetAmount(effect, message.ActivatorEntity) + previousDamage.Damage,
                        effectEntity);
                    break;
                default:
                    ApplyEffect(effectEntity, message.TargetEntity, message.TargetCell, message.ActivatorEntity,
                        null, effectsAppliedMessage.AppliedEffects, manager, pretend);
                    break;
            }
        }

        if (damageEffects != null)
        {
            foreach (var damageEffect in damageEffects)
            {
                ApplyEffect(damageEffect.Value.EffectEntity, message.TargetEntity, message.TargetCell,
                    message.ActivatorEntity, damageEffect.Value.Damage, effectsAppliedMessage.AppliedEffects,
                    manager, pretend);
            }
        }

        return effectsAppliedMessage;
    }

    public MessageProcessingResult Process(ApplyEffectMessage message, GameManager manager)
    {
        ApplyEffect(message.EffectEntity, message.TargetEntity, message.TargetCell,
            message.ActivatorEntity, amountOverride: null, appliedEffects: null, manager, pretend: false);
        return MessageProcessingResult.ContinueProcessing;
    }

    private GameEntity ApplyEffect(
        GameEntity effectEntity,
        GameEntity targetEntity,
        Point? targetCell,
        GameEntity activatorEntity,
        int? amountOverride,
        ReferencingList<GameEntity> appliedEffects,
        GameManager manager,
        bool pretend)
    {
        var effect = effectEntity.Effect;
        var being = targetEntity?.Being;
        switch (effect.EffectType)
        {
            case EffectType.ChangeProperty:
                var propertyDescription = PropertyDescription.Loader.Find(effect.TargetName);
                Debug.Assert(propertyDescription.IsCalculated == (effect.Duration != EffectDuration.Instant));

                ApplyEffect(effect, targetEntity, activatorEntity, amountOverride, appliedEffects, manager,
                    pretend);

                break;
            case EffectType.AddAbility:
                var abilityAddedEntity = (targetEntity.Physical?.AppliedEffects
                                          ?? targetEntity.Item?.AppliedEffects
                                          ?? targetEntity.Sensor?.AppliedEffects
                                          ?? being?.AppliedEffects)
                                         .FirstOrDefault(e => e.Effect.SourceEffectId == effect.EntityId)
                                         ?? ApplyEffect(
                                             effect, targetEntity, activatorEntity, amountOverride,
                                             appliedEffects, manager, pretend);
                if (manager.AffectableAbilitiesIndex[
                        (targetEntity.Id, effectEntity.Ability?.Name ?? effect.TargetName)] == null)
                {
                    if (effectEntity.Ability != null)
                    {
                        var ability = effectEntity.Ability.AddToEffect(abilityAddedEntity);
                        ability.OwnerId = targetEntity.Id;
                    }
                    else
                    {
                        var level = 0;
                        var template = Ability.Loader.Get(effect.TargetName);
                        if (template is LeveledAbility)
                        {
                            level = CalculateAbilityLevel(abilityAddedEntity.Effect, manager);
                        }

                        var ability = template.AddToEffect(abilityAddedEntity, level);

                        ability.OwnerId = targetEntity.Id;
                    }
                }
                else
                {
                    abilityAddedEntity.Effect.EffectType = EffectType.AddDuplicateAbility;
                }

                return abilityAddedEntity;
            case EffectType.ChangeRace:
                var remove = effect.AppliedAmount == -1;
                var raceName = effect.TargetName;
                if (remove)
                {
                    Debug.Assert(effect.Duration == EffectDuration.Instant);

                    var raceEntity = targetEntity.Being.Races
                        .SingleOrDefault(r => r.Race.TemplateName == raceName);
                    raceEntity?.RemoveComponent(EntityComponent.Effect);
                }
                else
                {
                    Debug.Assert(effect.Duration != EffectDuration.Instant);

                    if (!targetEntity.HasComponent(EntityComponent.Player))
                    {
                        return null;
                    }

                    var raceDefinition = PlayerRace.Loader.Find(raceName);

                    var existingRace = targetEntity.Being.Races
                        ?.SingleOrDefault(r => r.Race.Species == raceDefinition.Species);
                    if (existingRace == null)
                    {
                        var addedRaceEntity = ApplyEffect(
                            effect, targetEntity, activatorEntity, amountOverride, appliedEffects, manager,
                            pretend);
                        addedRaceEntity.Effect.AppliedAmount = null;

                        raceDefinition.AddToAppliedEffect(addedRaceEntity, targetEntity);
                        return addedRaceEntity;
                    }

                    // TODO: Replace subrace
                }

                break;
            case EffectType.Burn:
            // TODO: Removes slime, wet, frozen
            case EffectType.Corrode:
            case EffectType.Wither:
            case EffectType.Blight:
            case EffectType.Freeze:
            // TODO: Slows, removes burning, dissolving
            case EffectType.Bleed:
            case EffectType.Shock:
            // TODO: Removes slow
            case EffectType.Soak:
            // TODO: Removes burning
            case EffectType.LightDamage:
            case EffectType.SonicDamage:
            case EffectType.PsychicDamage:
            case EffectType.PhysicalDamage:
                if (being == null)
                {
                    return null;
                }

                var damage = GetDamage(effect, activatorEntity, targetEntity, amountOverride);
                if (damage != 0)
                {
                    return ApplyEffect(effect, targetEntity, activatorEntity, damage, appliedEffects, manager,
                        pretend);
                }

                break;
            case EffectType.DrainEnergy:
                if (being == null)
                {
                    return null;
                }

                var energyDamage = GetDamage(effect, activatorEntity, targetEntity, amountOverride);
                if (energyDamage != 0)
                {
                    var selfEffectEntity = ApplyEffect(
                        effect, activatorEntity, activatorEntity, energyDamage, null, manager, pretend);
                    selfEffectEntity.Effect.EffectType = EffectType.Recharge;
                    return ApplyEffect(
                        effect, targetEntity, activatorEntity, energyDamage, appliedEffects, manager, pretend);
                }

                break;
            case EffectType.DrainLife:
                if (being == null)
                {
                    return null;
                }

                var drainDamage = GetDamage(effect, activatorEntity, targetEntity, amountOverride);
                if (drainDamage != 0)
                {
                    var selfEffectEntity = ApplyEffect(
                        effect, activatorEntity, activatorEntity, drainDamage, null, manager, pretend);
                    selfEffectEntity.Effect.EffectType = EffectType.Heal;
                    return ApplyEffect(
                        effect, targetEntity, activatorEntity, drainDamage, appliedEffects, manager, pretend);
                }

                break;
            case EffectType.Move:
                var movee = manager.FindEntity(effect.TargetEntityId.Value);
                if (movee.HasComponent(EntityComponent.Item))
                {
                    var moveItemMessage = MoveItemMessage.Create(manager);
                    moveItemMessage.ItemEntity = movee;
                    moveItemMessage.SuppressLog = targetEntity != activatorEntity;
                    moveItemMessage.Force = true;

                    if (targetEntity != null)
                    {
                        var position = targetEntity.Position;
                        moveItemMessage.TargetLevelEntity = position.LevelEntity;
                        moveItemMessage.TargetCell = position.LevelCell;
                    }
                    else
                    {
                        Debug.Assert(targetCell != null);

                        moveItemMessage.TargetLevelEntity = activatorEntity.Position.LevelEntity;
                        moveItemMessage.TargetCell = targetCell;
                    }

                    manager.Enqueue(moveItemMessage);
                }

                return ApplyEffect(
                    effect, targetEntity, activatorEntity, amountOverride, appliedEffects, manager, pretend);
            case EffectType.RemoveItem:
                var itemToRemove = manager.FindEntity(effect.TargetEntityId);
                var containingAbilityEntity = effect.ContainingAbility;
                while (containingAbilityEntity != null)
                {
                    itemToRemove = containingAbilityEntity.Ability.OwnerEntity;
                    if (itemToRemove?.HasComponent(EntityComponent.Item) != true)
                    {
                        containingAbilityEntity = containingAbilityEntity.Effect?.ContainingAbility;
                    }
                    else
                    {
                        break;
                    }
                }

                if (itemToRemove?.HasComponent(EntityComponent.Item) != true)
                {
                    throw new InvalidOperationException("Couldn't find item to remove");
                }

                using (var itemReference = manager.ItemMovingSystem.Split(itemToRemove.Item, 1))
                {
                    EntityReferenceMessage<GameEntity>.Enqueue(itemReference.Referenced, manager);

                    var appliedEffectEntity = ApplyEffect(
                        effect, targetEntity, activatorEntity, amountOverride, appliedEffects, manager, pretend);
                    appliedEffectEntity.Effect.TargetEntityId = itemReference.Referenced.Id;

                    return appliedEffectEntity;
                }
            case EffectType.EquipItem:
                var itemEntity = manager.FindEntity(effect.TargetEntityId.Value);

                var equipped = false;
                foreach (var slot in manager.ItemUsageSystem.GetEquipableSlots(itemEntity.Item, targetEntity))
                {
                    if (!manager.ItemUsageSystem.TryEquip(itemEntity, slot, targetEntity, force: false))
                    {
                        equipped = true;
                        break;
                    }
                }

                if (!equipped)
                {
                    foreach (var slot in manager.ItemUsageSystem.GetEquipableSlots(itemEntity.Item, targetEntity))
                    {
                        if (!manager.ItemUsageSystem.TryEquip(itemEntity, slot, targetEntity, force: true))
                        {
                            break;
                        }
                    }
                }

                break;
            case EffectType.Heal:
            case EffectType.Recharge:
            case EffectType.GainXP:
            case EffectType.Activate:
                Debug.Assert(effect.Duration != EffectDuration.Infinite
                             || manager.FindEntity(effect.TargetEntityId).Ability.IsActive);

                return ApplyEffect(
                    effect, targetEntity, activatorEntity, amountOverride, appliedEffects, manager, pretend);
            case EffectType.Bind:
            case EffectType.Blind:
            case EffectType.ConferLycanthropy:
            case EffectType.Confuse:
            case EffectType.Cripple:
            case EffectType.Curse:
            case EffectType.Deafen:
            case EffectType.Disarm:
            case EffectType.Engulf:
            case EffectType.LevelTeleport:
            case EffectType.Paralyze:
            case EffectType.Sedate:
            case EffectType.Slime:
            case EffectType.Slow:
            case EffectType.DisableItem:
            case EffectType.Stick:
            case EffectType.Stone:
            case EffectType.Stun:
            case EffectType.Suffocate:
            case EffectType.Teleport:
                // TODO: Handle these
                break;
            default:
                throw new InvalidOperationException(
                    $"Adding effect {effectEntity.Id} of type {effect.EffectType} not handled.");
        }

        return null;
    }

    private GameEntity ApplyEffect(EffectComponent effect,
        GameEntity affectedEntity,
        GameEntity activatorEntity,
        int? amountOverride,
        ReferencingList<GameEntity> appliedEffects,
        GameManager manager,
        bool pretend)
    {
        using (var appliedEffectEntity = manager.CreateEntity())
        {
            var entity = appliedEffectEntity.Referenced;

            var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            appliedEffect.SourceEffectId = effect.EntityId;
            appliedEffect.SourceAbility = effect.ContainingAbility;
            appliedEffect.AppliedAmount = amountOverride
                                          ?? (effect.AppliedAmount != null || effect.Amount != null
                                              ? GetAmount(effect, activatorEntity)
                                              : null);
            appliedEffect.Duration = effect.Duration;
            appliedEffect.EffectType = effect.EffectType;
            appliedEffect.CombinationFunction = effect.CombinationFunction;
            appliedEffect.TargetName = effect.TargetName;
            appliedEffect.TargetEntityId = effect.TargetEntityId;
            if (!pretend)
            {
                appliedEffect.AffectedEntity = affectedEntity;
            }

            switch (effect.Duration)
            {
                case EffectDuration.Infinite:
                case EffectDuration.Instant:
                case EffectDuration.DuringApplication:
                    Debug.Assert(effect.DurationAmount == null);
                    break;
                case EffectDuration.UntilTimeout:
                    appliedEffect.ExpirationTick =
                        manager.Game.CurrentTick + GetDurationAmount(effect, activatorEntity);
                    break;
                case EffectDuration.UntilXPGained:
                    var player = affectedEntity.Player;
                    if (player != null)
                    {
                        appliedEffect.ExpirationXp =
                            affectedEntity.Position.LevelEntity.Level.Difficulty *
                            GetDurationAmount(effect, activatorEntity);
                    }
                    else
                    {
                        appliedEffect.ExpirationTick =
                            manager.Game.CurrentTick + 20 * GetDurationAmount(effect, activatorEntity);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            entity.Effect = appliedEffect;

            appliedEffects?.Add(entity);

            return appliedEffectEntity.Referenced;
        }
    }

    private int GetDamage(
        EffectComponent effect,
        GameEntity activatorEntity,
        GameEntity targetEntity,
        int? amountOverride)
    {
        var being = targetEntity.Being;
        var absorption = 0;
        var resistance = 0;
        switch (effect.EffectType)
        {
            case EffectType.PhysicalDamage:
                absorption = being.Armor - GetSecondaryAmount(effect, activatorEntity);
                resistance = being.PhysicalResistance;
                break;
            case EffectType.Burn:
                resistance = being.MagicResistance + being.FireResistance;
                break;
            case EffectType.Bleed:
                resistance = being.MagicResistance + being.BleedingResistance;
                break;
            case EffectType.Blight:
                resistance = being.MagicResistance + being.ToxinResistance;
                break;
            case EffectType.Corrode:
                resistance = being.MagicResistance + being.AcidResistance;
                break;
            case EffectType.Freeze:
                resistance = being.MagicResistance + being.ColdResistance;
                break;
            case EffectType.LightDamage:
                resistance = being.MagicResistance + being.LightResistance;
                break;
            case EffectType.PsychicDamage:
                resistance = being.MagicResistance + being.PsychicResistance;
                break;
            case EffectType.Shock:
                resistance = being.MagicResistance + being.ElectricityResistance;
                break;
            case EffectType.Soak:
                resistance = being.MagicResistance + being.WaterResistance;
                break;
            case EffectType.SonicDamage:
                resistance = being.MagicResistance + being.SonicResistance;
                break;
            case EffectType.Wither:
                resistance = being.MagicResistance + being.VoidResistance;
                break;
            case EffectType.DrainLife:
                resistance = being.MagicResistance + being.BleedingResistance;
                break;
            case EffectType.DrainEnergy:
                resistance = being.MagicResistance + being.VoidResistance;
                break;
        }

        var damage = amountOverride ?? GetAmount(effect, activatorEntity);
        if (absorption > 0
            && damage > 0)
        {
            damage = Math.Max(1, damage - absorption * damage / (absorption + damage));
        }

        if (damage < 0)
        {
            damage = 0;
        }

        damage = damage * (100 - resistance) / 100;
        return damage;
    }

    public int GetExpectedDamage(IEnumerable<GameEntity> effects, GameEntity activatorEntity,
        GameEntity targetEntity)
    {
        var totalDamage = 0;
        var damageEffects = new Dictionary<EffectType, (int Damage, GameEntity EffectEntity)>();
        foreach (var effectEntity in effects)
        {
            var effect = effectEntity.Effect;
            switch (effect.EffectType)
            {
                case EffectType.PhysicalDamage:
                case EffectType.Burn:
                case EffectType.Bleed:
                case EffectType.Blight:
                case EffectType.Corrode:
                case EffectType.Freeze:
                case EffectType.LightDamage:
                case EffectType.PsychicDamage:
                case EffectType.Shock:
                case EffectType.Soak:
                case EffectType.SonicDamage:
                case EffectType.Wither:
                case EffectType.DrainLife:
                    var key = effect.EffectType;
                    if (!damageEffects.TryGetValue(key, out var previousDamage))
                    {
                        previousDamage = (0, null);
                    }

                    // TODO: Take effectComponent.Function into account
                    damageEffects[key] = (GetAmount(effect, activatorEntity) + previousDamage.Damage,
                        effectEntity);
                    break;
                default:
                    continue;
            }
        }

        foreach (var damageEffect in damageEffects)
        {
            totalDamage += GetDamage(
                damageEffect.Value.EffectEntity.Effect, activatorEntity, targetEntity, damageEffect.Value.Damage);
        }

        return totalDamage;
    }

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        if (message.Group.Name == nameof(manager.Effects))
        {
            return ProcessEffectChanges(message.Entity, message.Entity.Effect, manager, State.Added);
        }

        InitializeProperties(message.Entity);

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager manager)
        => ProcessEffectChanges(
            message.Entity, (EffectComponent)message.RemovedComponent ?? message.Entity.Effect, manager,
            State.Removed);

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, int?> message, GameManager manager)
    {
        // EffectComponent.AppliedAmount
        var effect = (EffectComponent)message.ChangedComponent;
        if (effect.Duration != EffectDuration.Instant)
        {
            ProcessEffectChanges(message.Entity, effect, manager, State.Modified);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, string> message, GameManager manager)
    {
        // EffectComponent.Amount
        var effect = (EffectComponent)message.ChangedComponent;
        if (effect.Duration != EffectDuration.Instant)
        {
            ProcessEffectChanges(message.Entity, effect, manager, State.Modified);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private MessageProcessingResult ProcessEffectChanges(
        GameEntity effectEntity, EffectComponent effect, GameManager manager, State state)
    {
        if (effect.AffectedEntityId == null)
        {
            var containingAbility = effect.ContainingAbility?.Ability;
            if (containingAbility?.IsActive != true)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var activator = containingAbility.OwnerEntity;

            var appliedEffects =
                manager.AppliedEffectsToSourceAbilityRelationship.GetDependents(containingAbility.Entity);
            if (state == State.Added)
            {
                if (appliedEffects.Count != 0)
                {
                    var affectedEntity = appliedEffects.First().Effect.AffectedEntity;

                    // Handle the effect added to an ability that's already active
                    ApplyEffect(effectEntity, affectedEntity, targetCell: null, activatorEntity: activator,
                        amountOverride: null, appliedEffects: null, manager, pretend: false);
                }

                // Otherwise ability activation must be already in the queue
            }
            else
            {
                var appliedEffect = appliedEffects.Select(e => e.Effect)
                    .FirstOrDefault(e => e.SourceEffectId == effect.EntityId);
                if (appliedEffect == null)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                if (state == State.Removed)
                {
                    appliedEffect.Entity.Effect = null;
                }
                else
                {
                    appliedEffect.AppliedAmount = GetAmount(effect, activator);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        var targetEntity = manager.FindEntity(effect.AffectedEntityId.Value);
        if (targetEntity == null)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        switch (effect.EffectType)
        {
            case EffectType.ChangeProperty:
                if (state == State.Removed
                    && effect.Duration == EffectDuration.Instant)
                {
                    break;
                }

                var propertyDescription = PropertyDescription.Loader.Find(effect.TargetName);
                UpdateProperty(propertyDescription, targetEntity, effect);

                break;
            case EffectType.AddAbility:
                if (state == State.Removed)
                {
                    if (effectEntity.Ability == null)
                    {
                        break;
                    }

                    var abilitySlot = (effectEntity.Ability.Activation & ActivationType.WhileToggled) == 0
                        ? effectEntity.Ability.Slot
                        : null;
                    var abilityName = effectEntity.Ability.Name;
                    effectEntity.Ability = null;

                    if (abilityName == null)
                    {
                        break;
                    }

                    var appliedEffects = targetEntity.Physical?.AppliedEffects
                                         ?? targetEntity.Item?.AppliedEffects
                                         ?? targetEntity.Sensor?.AppliedEffects
                                         ?? targetEntity.Being.AppliedEffects;
                    foreach (var duplicateEffectEntity in appliedEffects)
                    {
                        var duplicateEffect = duplicateEffectEntity.Effect;
                        if (duplicateEffect.EffectType != EffectType.AddDuplicateAbility)
                        {
                            continue;
                        }

                        var sourceEffectEntity = duplicateEffect.SourceEffect;
                        if (sourceEffectEntity?.Effect == null
                            || (sourceEffectEntity.Ability?.Name ?? sourceEffectEntity.Effect.TargetName) !=
                            abilityName)
                        {
                            continue;
                        }

                        duplicateEffectEntity.Effect.EffectType = EffectType.AddAbility;

                        EnqueueApplyEffect(targetEntity, sourceEffectEntity, manager);

                        break;
                    }

                    if (abilitySlot != null)
                    {
                        // TODO: Try set slot even if no duplicate ability present yet
                        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
                        // TODO: use Id instead of the name
                        setSlotMessage.AbilityName = abilityName;
                        setSlotMessage.OwnerEntity = targetEntity;
                        setSlotMessage.Slot = abilitySlot;

                        manager.Enqueue(setSlotMessage, lowPriority: true);
                    }
                }
                else if (state == State.Modified)
                {
                    RecalculateAbilityLevel(effectEntity, targetEntity, manager);
                }

                break;
            case EffectType.AddDuplicateAbility:
            {
                var sourceEffectEntity = effect.SourceEffect;
                if (sourceEffectEntity?.Effect != null)
                {
                    var abilityName = sourceEffectEntity.Ability?.Name ?? sourceEffectEntity.Effect.TargetName;
                    var currentAbilityEntity = manager.AffectableAbilitiesIndex[(targetEntity.Id, abilityName)];

                    RecalculateAbilityLevel(currentAbilityEntity, targetEntity, manager);
                }

                break;
            }
            case EffectType.ChangeRace:
                if (state == State.Removed)
                {
                    effectEntity.RemoveComponent(EntityComponent.Race);
                    if (targetEntity.Being.Races.Count == 0)
                    {
                        var being = targetEntity.Being;
                        if (being != null)
                        {
                            // TODO: Add death cause
                            being.HitPoints = 0;
                        }
                    }
                }

                break;
            case EffectType.Burn:
            case EffectType.Corrode:
            case EffectType.Blight:
            case EffectType.Freeze:
            case EffectType.Bleed:
            case EffectType.Shock:
            case EffectType.Soak:
            case EffectType.LightDamage:
            case EffectType.PsychicDamage:
            case EffectType.SonicDamage:
            case EffectType.Wither:
            case EffectType.PhysicalDamage:
                if (state == State.Added)
                {
                    var being = targetEntity.Being;
                    being.HitPoints -= effect.AppliedAmount.Value;
                }

                break;
            case EffectType.DrainEnergy:
                if (state == State.Added)
                {
                    var being = targetEntity.Being;
                    being.EnergyPoints -= effect.AppliedAmount.Value;
                }

                break;
            case EffectType.DrainLife:
                if (state == State.Added)
                {
                    var being = targetEntity.Being;
                    being.HitPoints -= effect.AppliedAmount.Value;
                }

                break;
            case EffectType.Heal:
                if (state == State.Added)
                {
                    var being = targetEntity.Being;
                    being.HitPoints += effect.AppliedAmount.Value;
                }

                break;
            case EffectType.Recharge:
                if (state == State.Added)
                {
                    var being = targetEntity.Being;
                    being.EnergyPoints += effect.AppliedAmount.Value;
                }

                break;
            case EffectType.GainXP:
                if (state == State.Added)
                {
                    manager.XPSystem.AddPlayerXP(effect.AppliedAmount.Value, manager);
                }

                break;
            case EffectType.Activate:
                if (state == State.Removed)
                {
                    if (effect.Duration == EffectDuration.Instant)
                    {
                        break;
                    }

                    var abilityEntity = manager.FindEntity(effect.TargetEntityId);
                    if (abilityEntity != null)
                    {
                        var deactivateMessage =
                            DeactivateAbilityMessage.Create(manager);
                        deactivateMessage.AbilityEntity = abilityEntity;
                        deactivateMessage.ActivatorEntity = targetEntity;
                        manager.Enqueue(deactivateMessage);
                    }
                }

                break;
            case EffectType.Bind:
            case EffectType.Blind:
            case EffectType.ConferLycanthropy:
            case EffectType.Confuse:
            case EffectType.Cripple:
            case EffectType.Curse:
            case EffectType.Deafen:
            case EffectType.Disarm:
            case EffectType.Engulf:
            case EffectType.LevelTeleport:
            case EffectType.Paralyze:
            case EffectType.Sedate:
            case EffectType.Slime:
            case EffectType.Slow:
            case EffectType.DisableItem:
            case EffectType.Stick:
            case EffectType.Stone:
            case EffectType.Stun:
            case EffectType.Suffocate:
            case EffectType.RemoveItem:
            case EffectType.EquipItem:
            case EffectType.Teleport:
            case EffectType.Move:
                // TODO: Handle these effects
                break;
            default:
                throw new InvalidOperationException(
                    $"Effect {effectEntity.Id} of type {effect.EffectType} not handled.");
        }

        if (state == State.Added
            && (effect.Duration == EffectDuration.Instant
                || effect.Duration == EffectDuration.DuringApplication))
        {
            effect.AffectedEntity = null;

            RemoveComponentMessage.Enqueue(effectEntity, EntityComponent.Effect, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private void RecalculateAbilityLevel(GameEntity abilityEntity, GameEntity targetEntity, GameManager manager)
    {
        if (abilityEntity?.Ability.Template is LeveledAbility)
        {
            var level = CalculateAbilityLevel(abilityEntity.Effect, manager);
            if (level != abilityEntity.Ability.Level)
            {
                // TODO: If cumulative and leveling up just add the extra effects to the existing ability

                var sourceEffectEntity = abilityEntity.Effect.SourceEffect;
                if (sourceEffectEntity != null)
                {
                    abilityEntity.Ability = null;
                    EnqueueApplyEffect(targetEntity, sourceEffectEntity, manager);
                }
            }
        }
    }

    private static void EnqueueApplyEffect(GameEntity targetEntity, GameEntity effectEntity, GameManager manager)
    {
        var sourceAbilityEntity = effectEntity.Effect.ContainingAbility;
        var applyEffectMessage = ApplyEffectMessage.Create(manager);
        applyEffectMessage.ActivatorEntity = sourceAbilityEntity.Ability.OwnerEntity;
        applyEffectMessage.TargetEntity = targetEntity;
        applyEffectMessage.EffectEntity = effectEntity;

        manager.Enqueue(applyEffectMessage, lowPriority: true);
    }

    private int CalculateAbilityLevel(EffectComponent abilityEffect, GameManager manager)
    {
        var abilityName = abilityEffect.TargetName;
        var level = 0;
        var activeEffects = new List<EffectComponent> { abilityEffect };
        var appliedEffects = abilityEffect.AffectedEntity.Physical?.AppliedEffects
                             ?? abilityEffect.AffectedEntity.Item?.AppliedEffects
                             ?? abilityEffect.AffectedEntity.Sensor?.AppliedEffects
                             ?? abilityEffect.AffectedEntity.Being.AppliedEffects;
        foreach (var duplicateEffectEntity in appliedEffects)
        {
            var effect = duplicateEffectEntity.Effect;
            if (effect.EffectType != EffectType.AddDuplicateAbility)
            {
                continue;
            }

            if (effect.SourceEffect?.Effect?.TargetName != abilityName)
            {
                continue;
            }

            activeEffects.Add(effect);
        }

        activeEffects.Sort(AppliedEffectComponentComparer.Instance);

        var runningSum = 0;
        var summandCount = 0;
        for (var index = 0; index < activeEffects.Count; index++)
        {
            var activeEffect = activeEffects[index];

            ApplyOperation(activeEffect.AppliedAmount.Value, activeEffect.CombinationFunction, ref level,
                ref runningSum,
                ref summandCount);
        }

        return level;
    }

    public int GetDurationAmount(EffectComponent effect, GameEntity activatorEntity)
    {
        if (effect.DurationAmount == null)
        {
            return 0;
        }

        var ability = effect.ContainingAbility.Ability;
        effect.DurationAmountFunction ??= CreateAmountFunction(effect.DurationAmount, ability.Name);

        try
        {
            return (int)effect.DurationAmountFunction(effect.Entity, activatorEntity);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Duration for " + ability.Name, e);
        }
    }

    public static Func<GameEntity, GameEntity, float> CreateDurationFunction(string expression, string name)
    {
        try
        {
            return _translator.Translate<Func<GameEntity, GameEntity, float>, float>(expression);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while parsing the Duration for " + name, e);
        }
    }

    public int GetAmount(EffectComponent effect, GameEntity activatorEntity)
    {
        if (effect.Amount == null)
        {
            return effect.AppliedAmount.Value;
        }

        var ability = effect.ContainingAbility.Ability;
        effect.AmountFunction ??= CreateAmountFunction(effect.Amount, ability.Name);

        try
        {
            return (int)effect.AmountFunction(effect.Entity, activatorEntity);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Amount for " + ability.Name, e);
        }
    }

    public int GetSecondaryAmount(EffectComponent effect, GameEntity activatorEntity)
    {
        if (effect.SecondaryAmount == null)
        {
            return 0;
        }

        var ability = effect.ContainingAbility.Ability;
        effect.SecondaryAmountFunction ??= CreateAmountFunction(effect.SecondaryAmount, ability.Name);

        try
        {
            return (int)effect.SecondaryAmountFunction(effect.Entity, activatorEntity);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the SecondaryAmount for " + ability.Name,
                e);
        }
    }

    public static Func<GameEntity, GameEntity, float> CreateAmountFunction(string expression, string name)
    {
        try
        {
            return _translator.Translate<Func<GameEntity, GameEntity, float>, float>(expression);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while parsing the Amount for " + name, e);
        }
    }

    private static float GetRequirementsModifier(GameEntity effectEntity, GameEntity activatorEntity)
    {
        var manager = effectEntity.Manager;
        var item = effectEntity.Effect.ContainingAbility.Ability.OwnerEntity.Item;

        //TODO: Show stats for each equipable slot
        var slot = item.EquippedSlot == EquipmentSlot.None
            ? EquipmentSlot.GraspPrimaryMelee
            : item.EquippedSlot;
        var handnessMultiplier = manager.SkillAbilitiesSystem.GetHandnessMultiplier(slot);
        if (handnessMultiplier <= 0)
        {
            return 0;
        }

        var baseMultiplier =
            manager.SkillAbilitiesSystem.GetHandnessMultiplier(EquipmentSlot.GraspPrimaryMelee);
        var template = Item.Loader.Get(item.TemplateName);
        var skillBonus = manager.SkillAbilitiesSystem.GetItemSkillBonus(template, activatorEntity.Player);

        var requiredMight = template?.RequiredMight ?? 0;
        var mightDifference = activatorEntity.Being.Might * handnessMultiplier +
                              (skillBonus - requiredMight) * baseMultiplier;
        var requiredFocus = template?.RequiredFocus ?? 0;
        var focusDifference = activatorEntity.Being.Focus * handnessMultiplier +
                              (skillBonus - requiredFocus) * baseMultiplier;
        var scale = 100
                    + ((requiredMight * (mightDifference + Math.Min(0, mightDifference)))
                       + (requiredFocus * (focusDifference + Math.Min(0, focusDifference)))) /
                    handnessMultiplier;

        if (scale <= 0)
        {
            return 0;
        }

        return scale / 100f;
    }

    private static float GetMightModifier(GameEntity effectEntity, GameEntity activatorEntity)
        => (10 + activatorEntity.Being.Might) / 10f;

    private static float GetFocusModifier(GameEntity effectEntity, GameEntity activatorEntity)
        => (10 + activatorEntity.Being.Focus) / 10f;

    /// <summary>
    ///     Returns a permanently applied effect that affects the specified property.
    ///     The effect amount can be changed without having to reapply it.
    /// </summary>
    public EffectComponent UpdateOrAddPropertyEffect(
        int? amount, string propertyName, string abilityName, GameEntity entity,
        ValueCombinationFunction? function = null)
    {
        var manager = entity.Manager;
        var abilityEntity = GetOrAddPermanentAbility(entity, abilityName, manager);

        var effect = abilityEntity.Ability.Effects.Select(e => e.Effect)
            .FirstOrDefault(e => e.EffectType == EffectType.ChangeProperty && e.TargetName == propertyName);
        if (effect != null)
        {
            effect.AppliedAmount = amount;
            return effect;
        }

        return AddPropertyEffect(propertyName, amount, abilityEntity.Id, manager, function);
    }

    /// <summary>
    ///     Returns a permanently applied effect that affects the specified property.
    ///     The effect amount can be changed without having to reapply it.
    /// </summary>
    public EffectComponent GetOrAddAbilityEffect(GameEntity entity, string abilityName,
        string containingAbilityName)
    {
        var manager = entity.Manager;
        var abilityEntity = GetOrAddPermanentAbility(entity, containingAbilityName, manager);

        return abilityEntity.Ability.Effects.Select(e => e.Effect)
                   .FirstOrDefault(e => e.EffectType == EffectType.ChangeProperty
                                        && e.TargetName == abilityName)
               ?? AddAbilityEffect(abilityEntity.Id, abilityName, manager);
    }

    private GameEntity GetOrAddPermanentAbility(GameEntity entity, string abilityName, GameManager manager)
    {
        var propertyAbility = manager.AffectableAbilitiesIndex[(entity.Id, abilityName)];
        if (propertyAbility == null)
        {
            using (var abilityReference = manager.CreateEntity())
            {
                propertyAbility = abilityReference.Referenced;

                var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                ability.Name = abilityName;
                ability.OwnerId = entity.Id;
                ability.Activation = ActivationType.Always;

                propertyAbility.Ability = ability;
            }
        }

        return propertyAbility;
    }

    public EffectComponent AddPropertyEffect(
        string propertyName, int? value, int abilityId, GameManager manager,
        ValueCombinationFunction? function = null)
    {
        if (value == null)
        {
            return null;
        }

        using (var effectEntityReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);

            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = propertyName;
            effect.AppliedAmount = value;
            effect.CombinationFunction = function ?? ValueCombinationFunction.Sum;
            effect.ContainingAbilityId = abilityId;

            effectEntityReference.Referenced.Effect = effect;

            return effect;
        }
    }

    private EffectComponent AddAbilityEffect(int abilityId, string abilityName, GameManager manager)
    {
        using (var effectReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);

            effect.ContainingAbilityId = abilityId;
            effect.EffectType = EffectType.AddAbility;
            effect.Duration = EffectDuration.Infinite;
            effect.CombinationFunction = ValueCombinationFunction.Sum;
            effect.TargetName = abilityName;
            effect.AppliedAmount = 0;

            effectReference.Referenced.Effect = effect;

            return effect;
        }
    }

    private void InitializeProperties(GameEntity entity)
    {
        foreach (var propertyDescription in PropertyDescription.Loader.GetAll())
        {
            if (propertyDescription.IsCalculated
                && entity.HasComponent(propertyDescription.ComponentId))
            {
                UpdateProperty(propertyDescription, entity, null);
            }
        }
    }

    private void UpdateProperty(
        PropertyDescription propertyDescription,
        GameEntity targetEntity,
        EffectComponent appliedEffectComponent)
    {
        if (propertyDescription.PropertyType == typeof(bool))
        {
            UpdatePropertyValue(
                (PropertyDescription<bool>)propertyDescription,
                targetEntity,
                appliedEffectComponent);
        }
        else
        {
            UpdatePropertyValue(
                (PropertyDescription<int>)propertyDescription,
                targetEntity,
                appliedEffectComponent);
        }
    }

    private void UpdatePropertyValue(
        PropertyDescription<int> propertyDescription,
        GameEntity targetEntity,
        EffectComponent effect)
    {
        var targetComponent = targetEntity.FindComponent(propertyDescription.ComponentId);
        if (targetComponent == null)
        {
            return;
        }

        int newValue;
        var runningSum = 0;
        var summandCount = 0;
        if (propertyDescription.IsCalculated)
        {
            var activeEffects = GetSortedAppliedEffects(targetEntity, propertyDescription, effect);
            newValue = propertyDescription.DefaultValue ?? default;
            if (activeEffects != null)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var index = 0; index < activeEffects.Count; index++)
                {
                    var activeEffect = activeEffects[index];

                    ApplyOperation(activeEffect.AppliedAmount.Value, activeEffect.CombinationFunction,
                        ref newValue, ref runningSum, ref summandCount);
                }
            }
            else if (effect?.Entity?.Effect?.AffectedEntityId != null)
            {
                ApplyOperation(effect.AppliedAmount.Value, effect.CombinationFunction,
                    ref newValue, ref runningSum, ref summandCount);
            }

            propertyDescription.SetValue(newValue, targetComponent);
        }
        else
        {
            newValue = propertyDescription.GetValue(targetComponent);
            ApplyOperation(effect.AppliedAmount.Value, effect.CombinationFunction, ref newValue, ref runningSum,
                ref summandCount);
            propertyDescription.SetValue(newValue, targetComponent);
        }
    }

    private void UpdatePropertyValue(
        PropertyDescription<bool> propertyDescription,
        GameEntity targetEntity,
        EffectComponent effect)
    {
        var targetComponent = targetEntity.FindComponent(propertyDescription.ComponentId);
        if (targetComponent == null)
        {
            return;
        }

        bool newValue;
        var runningSum = 0;
        var summandCount = 0;
        if (propertyDescription.IsCalculated)
        {
            var activeEffects = GetSortedAppliedEffects(targetEntity, propertyDescription, effect);
            newValue = propertyDescription.DefaultValue ?? default;
            if (activeEffects != null)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var index = 0; index < activeEffects.Count; index++)
                {
                    var activeEffect = activeEffects[index];

                    Apply(activeEffect.AppliedAmount.Value != 0, activeEffect.CombinationFunction,
                        ref newValue, ref runningSum, ref summandCount);
                }
            }
            else if (effect?.Entity?.Effect?.AffectedEntityId != null)
            {
                Apply(effect.AppliedAmount.Value != 0, effect.CombinationFunction,
                    ref newValue, ref runningSum, ref summandCount);
            }

            propertyDescription.SetValue(newValue, targetComponent);
        }
        else
        {
            newValue = propertyDescription.GetValue(targetComponent);
            Apply(effect.AppliedAmount.Value != 0, effect.CombinationFunction, ref newValue, ref runningSum,
                ref summandCount);
            propertyDescription.SetValue(newValue, targetComponent);
        }
    }

    private void ApplyOperation(
        int amount,
        ValueCombinationFunction function,
        ref int propertyValue,
        ref int runningSum,
        ref int summandCount)
    {
        if (summandCount == 0)
        {
            switch (function)
            {
                case ValueCombinationFunction.Sum:
                case ValueCombinationFunction.Percent:
                    break;
                default:
                    propertyValue = amount;
                    runningSum = propertyValue;
                    summandCount = 1;
                    return;
            }
        }

        switch (function)
        {
            case ValueCombinationFunction.Sum:
                propertyValue += amount;
                break;
            case ValueCombinationFunction.Percent:
                propertyValue = (propertyValue * amount) / 100;
                break;
            case ValueCombinationFunction.Override:
                propertyValue = amount;
                break;
            case ValueCombinationFunction.Max:
                propertyValue = propertyValue > amount ? propertyValue : amount;
                break;
            case ValueCombinationFunction.Min:
                propertyValue = propertyValue < amount ? propertyValue : amount;
                break;
            case ValueCombinationFunction.MeanRoundDown:
                runningSum += amount;
                summandCount++;
                propertyValue = runningSum / summandCount;
                break;
            case ValueCombinationFunction.MeanRoundUp:
                runningSum += amount;
                summandCount++;
                propertyValue = (runningSum + summandCount - 1) / summandCount;
                break;
        }

        runningSum = propertyValue;
        summandCount = 1;
    }

    private void Apply(
        bool amount,
        ValueCombinationFunction function,
        ref bool propertyValue,
        ref int runningSum,
        ref int summandCount)
    {
        if (summandCount == 0)
        {
            switch (function)
            {
                case ValueCombinationFunction.Sum:
                case ValueCombinationFunction.Percent:
                    break;
                default:
                    propertyValue = amount;
                    runningSum = propertyValue ? 1 : 0;
                    summandCount = 1;
                    return;
            }
        }

        switch (function)
        {
            case ValueCombinationFunction.Sum:
                propertyValue |= amount;
                break;
            case ValueCombinationFunction.Percent:
                propertyValue &= amount;
                break;
            case ValueCombinationFunction.Override:
                propertyValue = amount;
                break;
            case ValueCombinationFunction.Max:
                propertyValue |= amount;
                break;
            case ValueCombinationFunction.Min:
                propertyValue &= amount;
                break;
            case ValueCombinationFunction.MeanRoundDown:
                runningSum += amount ? 1 : 0;
                summandCount++;
                propertyValue = runningSum * 2 > summandCount;
                break;
            case ValueCombinationFunction.MeanRoundUp:
                runningSum += amount ? 1 : 0;
                summandCount++;
                propertyValue = runningSum * 2 >= summandCount;
                break;
        }

        runningSum = propertyValue ? 1 : 0;
        summandCount = 1;
    }

    private List<EffectComponent> GetSortedAppliedEffects(GameEntity targetEntity,
        PropertyDescription propertyDescription, EffectComponent effect)
    {
        List<EffectComponent> activeEffects = null;
        var appliedEffects = targetEntity.Physical?.AppliedEffects
                             ?? targetEntity.Item?.AppliedEffects
                             ?? targetEntity.Sensor?.AppliedEffects
                             ?? targetEntity.Being.AppliedEffects;
        foreach (var otherEffectEntity in appliedEffects)
        {
            var otherEffect = otherEffectEntity.Effect;
            if (otherEffect != effect
                && otherEffect.EffectType == EffectType.ChangeProperty
                && otherEffect.TargetName.Equals(propertyDescription.Name))
            {
                activeEffects = new List<EffectComponent> { otherEffect };
            }
        }

        if (activeEffects == null)
        {
            return null;
        }

        if (effect != null)
        {
            activeEffects.Add(effect);
        }

        activeEffects.Sort(AppliedEffectComponentComparer.Instance);
        return activeEffects;
    }

    private class EffectExpressionVisitor : UnicornExpressionVisitor
    {
        public EffectExpressionVisitor(IEnumerable<ParameterExpression> parameters)
            : base(parameters)
        {
        }

        protected override Expression CreateInvocation(string function, IEnumerable<Expression> children,
            Type childrenCommonType)
        {
            MethodInfo method;
            switch (function)
            {
                case "RequirementsModifier":
                    method = _requirementsModifierMethod;
                    break;
                case "MightModifier":
                    method = _mightModifierMethod;
                    break;
                case "FocusModifier":
                    method = _focusModifierMethod;
                    break;
                default:
                    return base.CreateInvocation(function, children, childrenCommonType);
            }

            if (children.Any())
            {
                throw new InvalidOperationException($"Expected 0 parameters for function {function}");
            }

            return Expression.Call(null, method, EffectParameter, ActivatorParameter);
        }
    }

    private class AppliedEffectComponentComparer : IComparer<EffectComponent>
    {
        public static readonly AppliedEffectComponentComparer Instance = new();

        private AppliedEffectComponentComparer()
        {
        }

        public int Compare(EffectComponent x, EffectComponent y)
        {
            var xAbility = x.SourceAbility?.Ability;
            var yAbility = y.SourceAbility?.Ability;

            if (xAbility != null
                && yAbility != null)
            {
                switch (xAbility.Activation)
                {
                    case ActivationType.Always:
                        switch (yAbility.Activation)
                        {
                            case ActivationType.Always:
                                if (xAbility.Name == InnateAbilityName)
                                {
                                    if (yAbility.Name != InnateAbilityName)
                                    {
                                        return -1;
                                    }
                                }
                                else if (yAbility.Name == InnateAbilityName)
                                {
                                    return 1;
                                }

                                break;
                            default:
                                return -1;
                        }

                        break;
                    case ActivationType.WhileToggled:
                        switch (yAbility.Activation)
                        {
                            case ActivationType.Always:
                                return 1;
                            case ActivationType.WhileToggled:
                                break;
                            default:
                                return -1;
                        }

                        break;
                    case ActivationType.WhilePossessed:
                        switch (yAbility.Activation)
                        {
                            case ActivationType.Always:
                            case ActivationType.WhileToggled:
                                return 1;
                            case ActivationType.WhilePossessed:
                                break;
                            default:
                                return -1;
                        }

                        break;
                    case ActivationType.WhileEquipped:
                        switch (yAbility.Activation)
                        {
                            case ActivationType.Always:
                            case ActivationType.WhileToggled:
                            case ActivationType.WhilePossessed:
                                return 1;
                            case ActivationType.WhileEquipped:
                                break;
                            default:
                                return -1;
                        }

                        break;
                    default:
                        switch (yAbility.Activation)
                        {
                            case ActivationType.Always:
                            case ActivationType.WhileToggled:
                            case ActivationType.WhilePossessed:
                            case ActivationType.WhileEquipped:
                                return 1;
                        }

                        break;
                }
            }

            var result = y.CombinationFunction - x.CombinationFunction;
            if (result != 0)
            {
                return result;
            }

            return x.EntityId - y.EntityId;
        }
    }

    private enum State
    {
        Added,
        Removed,
        Modified
    }
}
