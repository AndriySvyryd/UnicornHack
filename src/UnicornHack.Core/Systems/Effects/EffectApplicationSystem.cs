using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects
{
    public class EffectApplicationSystem :
        IGameSystem<AbilityActivatedMessage>,
        IGameSystem<ApplyEffectMessage>,
        IGameSystem<EntityAddedMessage<GameEntity>>,
        IGameSystem<EntityRemovedMessage<GameEntity>>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, int?>>
    {
        public const string EffectsAppliedMessageName = "EffectsApplied";
        public const string ApplyEffectMessageName = "ApplyEffect";
        public const string InnateAbilityName = "innate";

        public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager manager)
        {
            if (message.TargetEntity?.Being.IsAlive != true)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var effectsAppliedMessage = manager.Queue.CreateMessage<EffectsAppliedMessage>(EffectsAppliedMessageName);
            effectsAppliedMessage.ActivatorEntity = message.ActivatorEntity;
            effectsAppliedMessage.TargetEntity = message.TargetEntity;
            effectsAppliedMessage.AbilityEntity = message.AbilityEntity;
            effectsAppliedMessage.AbilityTrigger = message.Trigger;
            effectsAppliedMessage.SuccessfulApplication = message.SuccessfulApplication;

            var appliedEffects = new ReferencingList<GameEntity>();
            // TODO: Group effects by duration so they can be applied in proper order
            // TODO: Fire a message for effects that are DuringApplication, so they get unapplied as soon as the previous messages are processed
            Dictionary<EffectType, (int Damage, GameEntity EffectEntity)> damageEffects = null;
            foreach (var effectEntity in message.EffectsToApply)
            {
                var effect = effectEntity.Effect;
                if (!message.SuccessfulApplication
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

                        // TODO: Take effectComponent.Function into account
                        damageEffects[key] = (effect.Amount.Value + previousDamage.Damage, effectEntity);
                        break;
                    default:
                        ApplyEffect(effectEntity, message.TargetEntity, message.TargetCell, message.ActivatorEntity,
                            null, appliedEffects, manager);
                        break;
                }
            }

            if (damageEffects != null)
            {
                foreach (var damageEffect in damageEffects)
                {
                    ApplyEffect(damageEffect.Value.EffectEntity, message.TargetEntity, message.TargetCell,
                        message.ActivatorEntity, damageEffect.Value.Damage, appliedEffects, manager);
                }
            }

            effectsAppliedMessage.AppliedEffects = appliedEffects;
            manager.Enqueue(effectsAppliedMessage);

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(ApplyEffectMessage message, GameManager manager)
        {
            ApplyEffect(message.EffectEntity, message.TargetEntity, message.TargetCell,
                message.ActivatorEntity, amountOverride: null, appliedEffects: null, manager);
            return MessageProcessingResult.ContinueProcessing;
        }

        private GameEntity ApplyEffect(
            GameEntity effectEntity,
            GameEntity affectedEntity,
            Point? targetCell,
            GameEntity activatorEntity,
            int? amountOverride,
            ReferencingList<GameEntity> appliedEffects,
            GameManager manager)
        {
            var effect = effectEntity.Effect;

            var being = affectedEntity?.Being;
            switch (effect.EffectType)
            {
                case EffectType.ChangeProperty:
                    var propertyDescription = PropertyDescription.Loader.Find(effect.TargetName);
                    Debug.Assert(propertyDescription.IsCalculated == (effect.Duration != EffectDuration.Instant));

                    ApplyEffect(effect, affectedEntity, amountOverride, appliedEffects, manager);

                    break;
                case EffectType.AddAbility:
                    var abilityAddedEntity = manager.AppliedEffectsToAffectableEntityRelationship[affectedEntity.Id]
                                                 .FirstOrDefault(e => e.Effect.SourceEffectId == effect.EntityId)
                                             ?? ApplyEffect(
                                                 effect, affectedEntity, amountOverride, appliedEffects, manager);
                    if (manager.AffectableAbilitiesIndex[
                            (affectedEntity.Id, effectEntity.Ability?.Name ?? effect.TargetName)] == null)
                    {
                        if (effectEntity.Ability != null)
                        {
                            var ability = effectEntity.Ability.AddToEffect(abilityAddedEntity);
                            ability.OwnerId = affectedEntity.Id;
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

                            ability.OwnerId = affectedEntity.Id;
                        }
                    }
                    else
                    {
                        abilityAddedEntity.Effect.EffectType = EffectType.AddDuplicateAbility;
                    }

                    return abilityAddedEntity;
                case EffectType.ChangeRace:
                    var remove = effect.Amount == -1;
                    var raceName = effect.TargetName;
                    if (remove)
                    {
                        Debug.Assert(effect.Duration == EffectDuration.Instant);

                        var raceEntity = manager.RacesToBeingRelationship[affectedEntity.Id].Values
                            .SingleOrDefault(r => r.Race.TemplateName == raceName);
                        raceEntity?.RemoveComponent(EntityComponent.Effect);
                    }
                    else
                    {
                        Debug.Assert(effect.Duration != EffectDuration.Instant);

                        if (!affectedEntity.HasComponent(EntityComponent.Player))
                        {
                            return null;
                        }

                        var raceDefinition = PlayerRace.Loader.Find(raceName);

                        var existingRace = manager.RacesToBeingRelationship[affectedEntity.Id].Values
                            .SingleOrDefault(r => r.Race.Species == raceDefinition.Species);
                        if (existingRace == null)
                        {
                            var addedRaceEntity =
                                ApplyEffect(effect, affectedEntity, amountOverride, appliedEffects, manager);
                            addedRaceEntity.Effect.Amount = null;

                            raceDefinition.AddToAppliedEffect(addedRaceEntity, affectedEntity.Id);
                            return addedRaceEntity;
                        }

                        // TODO: Replace subrace
                    }

                    break;
                case EffectType.Burn:
                    // TODO: Burns items
                    // TODO: Removes slime, wet, frozen

                    if (being == null)
                    {
                        return null;
                    }

                    var fireDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.FireResistance);
                    if (fireDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, fireDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.Corrode:
                    // TODO: Corrodes items
                    // TODO: Removes stoning

                    if (being == null)
                    {
                        return null;
                    }

                    var acidDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.AcidResistance);
                    if (acidDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, acidDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.Wither:
                    // TODO: Withers items

                    if (being == null)
                    {
                        return null;
                    }

                    var witherDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.VoidResistance);
                    if (witherDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, witherDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.Blight:
                    // TODO: Decays items

                    if (being == null)
                    {
                        return null;
                    }

                    var blightDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.ToxinResistance);
                    if (blightDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, blightDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.Freeze:
                    // TODO: Freezes items
                    // TODO: Slows, removes burning, dissolving

                    if (being == null)
                    {
                        return null;
                    }

                    var coldDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.ColdResistance);
                    if (coldDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, coldDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.Bleed:
                    if (being == null)
                    {
                        return null;
                    }

                    var bleedingDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.BleedingResistance);
                    if (bleedingDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, bleedingDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.Shock:
                    // TODO: Causing some mechanical and magical items to trigger
                    // TODO: Removes slow

                    if (being == null)
                    {
                        return null;
                    }

                    var shockDamage = GetDamage(effect, amountOverride,0,
                        being.MagicResistance + being.ElectricityResistance);
                    if (shockDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, shockDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.Soak:
                    // TODO: Rusts items
                    // TODO: Removes burning

                    if (being == null)
                    {
                        return null;
                    }

                    var waterDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.WaterResistance);
                    if (waterDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, waterDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.LightDamage:
                    if (being == null)
                    {
                        return null;
                    }

                    var lightDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.LightResistance);
                    if (lightDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, lightDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.SonicDamage:
                    if (being == null)
                    {
                        return null;
                    }

                    var sonicDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.SonicResistance);
                    if (sonicDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, sonicDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.PsychicDamage:
                    if (being == null)
                    {
                        return null;
                    }

                    var psychicDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.PsychicResistance);
                    if (psychicDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, psychicDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.PhysicalDamage:
                    if (being == null)
                    {
                        return null;
                    }

                    var physicalDamage = GetDamage(effect, amountOverride, being.Armor - effect.SecondaryAmount ?? 0,
                        being.PhysicalResistance);
                    if (physicalDamage != 0)
                    {
                        return ApplyEffect(effect, affectedEntity, physicalDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.DrainEnergy:
                    if (being == null)
                    {
                        return null;
                    }

                    var energyDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.VoidResistance);
                    if (energyDamage != 0)
                    {
                        var selfEffectEntity = ApplyEffect(effect, activatorEntity, energyDamage, null, manager);
                        selfEffectEntity.Effect.EffectType = EffectType.Recharge;
                        return ApplyEffect(effect, affectedEntity, energyDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.DrainLife:
                    if (being == null)
                    {
                        return null;
                    }

                    var drainDamage = GetDamage(effect, amountOverride, 0,
                        being.MagicResistance + being.BleedingResistance);
                    if (drainDamage != 0)
                    {
                        var selfEffectEntity = ApplyEffect(effect, activatorEntity, drainDamage, null, manager);
                        selfEffectEntity.Effect.EffectType = EffectType.Heal;
                        return ApplyEffect(effect, affectedEntity, drainDamage, appliedEffects, manager);
                    }

                    break;
                case EffectType.Move:
                    var movee = manager.FindEntity(effect.TargetEntityId.Value);
                    if (movee.HasComponent(EntityComponent.Item))
                    {
                        var moveItemMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
                        moveItemMessage.ItemEntity = movee;
                        moveItemMessage.SuppressLog = true;
                        moveItemMessage.Force = true;

                        if (affectedEntity != null)
                        {
                            var position = affectedEntity.Position;
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

                    return ApplyEffect(effect, affectedEntity, amountOverride, appliedEffects, manager);
                case EffectType.RemoveItem:
                    var itemToRemove = manager.FindEntity(effect.TargetEntityId);
                    var abilityId = effect.ContainingAbilityId;
                    while (abilityId != null)
                    {
                        var abilityEntity = manager.FindEntity(abilityId);
                        itemToRemove = abilityEntity.Ability.OwnerEntity;
                        if (itemToRemove?.HasComponent(EntityComponent.Item) != true)
                        {
                            abilityId = abilityEntity.Effect.ContainingAbilityId;
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
                        var referenceMessage = manager.CreateEntityReferenceMessage(itemReference.Referenced);
                        manager.Enqueue(referenceMessage, lowPriority: true);

                        var appliedEffectEntity = ApplyEffect(
                            effect, affectedEntity, amountOverride, appliedEffects, manager);
                        appliedEffectEntity.Effect.TargetEntityId = itemReference.Referenced.Id;

                        return appliedEffectEntity;
                    }
                case EffectType.Heal:
                case EffectType.Recharge:
                case EffectType.GainXP:
                case EffectType.Activate:
                    Debug.Assert(effect.Duration != EffectDuration.Infinite
                                 || manager.FindEntity(effect.TargetEntityId).Ability.IsActive);

                    return ApplyEffect(effect, affectedEntity, amountOverride, appliedEffects, manager);
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
                case EffectType.StealGold:
                case EffectType.StealItem:
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

        private GameEntity ApplyEffect(
            EffectComponent effect,
            GameEntity affectedEntity,
            int? amountOverride,
            ReferencingList<GameEntity> appliedEffects,
            GameManager manager)
        {
            using (var appliedEffectEntity = manager.CreateEntity())
            {
                var entity = appliedEffectEntity.Referenced;

                var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                appliedEffect.ShouldTargetActivator = effect.ShouldTargetActivator;
                appliedEffect.SourceEffectId = effect.EntityId;
                appliedEffect.SourceAbilityId = effect.ContainingAbilityId;
                appliedEffect.Amount = amountOverride ?? effect.Amount;
                appliedEffect.Duration = effect.Duration;
                appliedEffect.DurationAmount = effect.DurationAmount;
                appliedEffect.EffectType = effect.EffectType;
                appliedEffect.Function = effect.Function;
                appliedEffect.TargetName = effect.TargetName;
                appliedEffect.TargetEntityId = effect.TargetEntityId;
                appliedEffect.AffectedEntityId = affectedEntity.Id;

                switch (effect.Duration)
                {
                    case EffectDuration.Infinite:
                    case EffectDuration.Instant:
                    case EffectDuration.DuringApplication:
                        Debug.Assert(effect.DurationAmount == 0);
                        break;
                    case EffectDuration.UntilTimeout:
                        Debug.Assert(effect.DurationAmount > 0);

                        appliedEffect.ExpirationTick = manager.Game.CurrentTick + effect.DurationAmount;
                        break;
                    case EffectDuration.UntilXPGained:
                        Debug.Assert(effect.DurationAmount > 0);

                        var player = affectedEntity.Player;
                        if (player != null)
                        {
                            appliedEffect.ExpirationXp =
                                affectedEntity.Player.NextLevelXP * effect.DurationAmount / 100;
                        }
                        else
                        {
                            appliedEffect.ExpirationTick = manager.Game.CurrentTick + effect.DurationAmount * 100;
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

        private static int GetDamage(EffectComponent effect, int? amountOverride, int absorption, int resistance)
        {
            var damage = amountOverride ?? effect.Amount.Value;
            if (absorption > 0)
            {
                damage -= absorption;
            }

            if (damage < 0)
            {
                damage = 0;
            }

            damage = (damage * (100 - resistance)) / 100;
            return damage;
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
        {
            if (message.ChangedComponent?.ComponentId == (int)EntityComponent.Effect)
            {
                return ProcessEffectChanges(
                    message.Entity, (EffectComponent)message.ChangedComponent, manager, State.Added);
            }

            InitializeProperties(message.Entity, manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager manager)
            => ProcessEffectChanges(
                message.Entity, (EffectComponent)message.ChangedComponent, manager, State.Removed);

        public MessageProcessingResult Process(
            PropertyValueChangedMessage<GameEntity, int?> message, GameManager manager)
        {
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
                if (effect.ContainingAbilityId == null)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                var containingAbility = manager.FindEntity(effect.ContainingAbilityId)?.Ability;
                if (containingAbility?.IsActive != true)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                var appliedEffects = manager.AppliedEffectsToSourceAbilityRelationship[containingAbility.EntityId];
                if (state == State.Added)
                {
                    if (appliedEffects.Count != 0)
                    {
                        var appliedEffect = appliedEffects.First().Effect;
                        var affectedEntity = manager.FindEntity(appliedEffect.AffectedEntityId);

                        ApplyEffect(effectEntity, affectedEntity, targetCell: null, activatorEntity: null,
                            amountOverride: null, appliedEffects: null, manager);
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
                        appliedEffect.Amount = effect.Amount;
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
                    UpdateProperty(propertyDescription, targetEntity, effect, manager);

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

                        foreach (var duplicateEffectEntity in manager.AppliedEffectsToAffectableEntityRelationship[
                            targetEntity.Id])
                        {
                            var duplicateEffect = duplicateEffectEntity.Effect;
                            if (duplicateEffect.EffectType != EffectType.AddDuplicateAbility)
                            {
                                continue;
                            }

                            var sourceEffectEntity = manager.FindEntity(duplicateEffect.SourceEffectId);
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
                            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
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
                    var sourceEffectEntity = manager.FindEntity(effect.SourceEffectId);
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
                        if (manager.RacesToBeingRelationship[targetEntity.Id].Count == 0)
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
                        being.HitPoints -= effect.Amount.Value;
                    }

                    break;
                case EffectType.DrainEnergy:
                    if (state == State.Added)
                    {
                        var being = targetEntity.Being;
                        being.EnergyPoints -= effect.Amount.Value;
                    }

                    break;
                case EffectType.DrainLife:
                    if (state == State.Added)
                    {
                        var being = targetEntity.Being;
                        being.HitPoints -= effect.Amount.Value;
                    }

                    break;
                case EffectType.Heal:
                    if (state == State.Added)
                    {
                        var being = targetEntity.Being;
                        being.HitPoints += effect.Amount.Value;
                    }

                    break;
                case EffectType.Recharge:
                    if (state == State.Added)
                    {
                        var being = targetEntity.Being;
                        being.EnergyPoints += effect.Amount.Value;
                    }

                    break;
                case EffectType.GainXP:
                    if (state == State.Added)
                    {
                        manager.XPSystem.AddPlayerXP(effect.Amount.Value, manager);
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
                                manager.AbilityActivationSystem.CreateDeactivateAbilityMessage(manager);
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
                case EffectType.StealGold:
                case EffectType.StealItem:
                case EffectType.Stick:
                case EffectType.Stone:
                case EffectType.Stun:
                case EffectType.Suffocate:
                case EffectType.RemoveItem:
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
                effect.AffectedEntityId = null;
                var removeComponentMessage = manager.CreateRemoveComponentMessage();
                removeComponentMessage.Entity = effectEntity;
                removeComponentMessage.Component = EntityComponent.Effect;

                manager.Enqueue(removeComponentMessage, lowPriority: true);
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

                    var sourceEffectEntity = manager.FindEntity(abilityEntity.Effect.SourceEffectId);
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
            var sourceAbilityEntity = manager.FindEntity(effectEntity.Effect.ContainingAbilityId);
            var applyEffectMessage = manager.Queue.CreateMessage<ApplyEffectMessage>(ApplyEffectMessageName);
            applyEffectMessage.ActivatorEntity = sourceAbilityEntity.Ability.OwnerEntity;
            applyEffectMessage.TargetEntity = targetEntity;
            applyEffectMessage.EffectEntity = effectEntity;

            manager.Enqueue(applyEffectMessage, lowPriority: true);
        }

        private int CalculateAbilityLevel(EffectComponent abilityEffect, GameManager manager)
        {
            var abilityName = abilityEffect.TargetName;
            var level = 0;
            var activeEffects = new List<EffectComponent> {abilityEffect};
            foreach (var duplicateEffectEntity in
                manager.AppliedEffectsToAffectableEntityRelationship[abilityEffect.AffectedEntityId.Value])
            {
                var effect = duplicateEffectEntity.Effect;
                if (effect.EffectType != EffectType.AddDuplicateAbility)
                {
                    continue;
                }

                var sourceEffectEntity = manager.FindEntity(effect.SourceEffectId);
                if (sourceEffectEntity?.Effect?.TargetName != abilityName)
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

                ApplyOperation(activeEffect.Amount.Value, activeEffect.Function, ref level, ref runningSum,
                    ref summandCount);
            }

            return level;
        }

        /// <summary>
        ///     Returns a permanently applied effect that affects the specified property.
        ///     The effect amount can be changed without having to reapply it.
        /// </summary>
        public EffectComponent GetPropertyEffect(GameEntity entity, string propertyName, string abilityName)
        {
            var manager = entity.Manager;
            var abilityEntity = GetOrAddPermanentAbility(entity.Id, abilityName, manager);

            return manager.EffectsToContainingAbilityRelationship[abilityEntity.Id].Select(e => e.Effect)
                       .FirstOrDefault(e => e.EffectType == EffectType.ChangeProperty
                                            && e.TargetName == propertyName)
                   ?? AddPropertyEffect(abilityEntity.Id, propertyName, manager);
        }

        /// <summary>
        ///     Returns a permanently applied effect that affects the specified property.
        ///     The effect amount can be changed without having to reapply it.
        /// </summary>
        public EffectComponent GetAbilityEffect(GameEntity entity, string abilityName, string containingAbilityName)
        {
            var manager = entity.Manager;
            var abilityEntity = GetOrAddPermanentAbility(entity.Id, containingAbilityName, manager);

            return manager.EffectsToContainingAbilityRelationship[abilityEntity.Id].Select(e => e.Effect)
                       .FirstOrDefault(e => e.EffectType == EffectType.ChangeProperty
                                            && e.TargetName == abilityName)
                   ?? AddAbilityEffect(abilityEntity.Id, abilityName, manager);
        }

        private GameEntity GetOrAddPermanentAbility(int beingId, string abilityName, GameManager manager)
        {
            var propertyAbility = manager.AbilitiesToAffectableRelationship[beingId]
                .FirstOrDefault(a => a.Ability.Name == abilityName);
            if (propertyAbility == null)
            {
                using (var abilityReference = manager.CreateEntity())
                {
                    propertyAbility = abilityReference.Referenced;

                    var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                    ability.Name = abilityName;
                    ability.OwnerId = beingId;
                    ability.Activation = ActivationType.Always;

                    propertyAbility.Ability = ability;
                }
            }

            return propertyAbility;
        }

        private EffectComponent AddPropertyEffect(int abilityId, string propertyName, GameManager manager)
        {
            using (var effectReference = manager.CreateEntity())
            {
                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);

                effect.ContainingAbilityId = abilityId;
                effect.EffectType = EffectType.ChangeProperty;
                effect.Duration = EffectDuration.Infinite;
                effect.Function = ValueCombinationFunction.MeanRoundDown;
                effect.TargetName = propertyName;
                effect.Amount = 0;

                effectReference.Referenced.Effect = effect;

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
                effect.Function = ValueCombinationFunction.Sum;
                effect.TargetName = abilityName;
                effect.Amount = 0;

                effectReference.Referenced.Effect = effect;

                return effect;
            }
        }

        private void InitializeProperties(GameEntity entity, GameManager manager)
        {
            foreach (var propertyDescription in PropertyDescription.Loader.GetAll())
            {
                if (propertyDescription.IsCalculated
                    && entity.HasComponent(propertyDescription.ComponentId))
                {
                    UpdateProperty(propertyDescription, entity, null, manager);
                }
            }
        }

        private void UpdateProperty(
            PropertyDescription propertyDescription,
            GameEntity targetEntity,
            EffectComponent appliedEffectComponent,
            GameManager manager)
        {
            if (propertyDescription.PropertyType == typeof(bool))
            {
                UpdatePropertyValue(
                    (PropertyDescription<bool>)propertyDescription,
                    targetEntity,
                    appliedEffectComponent,
                    manager);
            }
            else
            {
                UpdatePropertyValue(
                    (PropertyDescription<int>)propertyDescription,
                    targetEntity,
                    appliedEffectComponent,
                    manager);
            }
        }

        private void UpdatePropertyValue(
            PropertyDescription<int> propertyDescription,
            GameEntity targetEntity,
            EffectComponent effect,
            GameManager manager)
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
                var activeEffects = GetSortedAppliedEffects(targetEntity, propertyDescription, effect, manager);
                newValue = propertyDescription.DefaultValue ?? default;
                if (activeEffects != null)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var index = 0; index < activeEffects.Count; index++)
                    {
                        var activeEffect = activeEffects[index];

                        ApplyOperation(activeEffect.Amount.Value, activeEffect.Function, ref newValue, ref runningSum,
                            ref summandCount);
                    }
                }
                else if (effect?.Entity?.Effect?.AffectedEntityId != null)
                {
                    ApplyOperation(effect.Amount.Value, effect.Function, ref newValue, ref runningSum,
                        ref summandCount);
                }

                propertyDescription.SetValue(newValue, targetComponent);
            }
            else
            {
                newValue = propertyDescription.GetValue(targetComponent);
                ApplyOperation(effect.Amount.Value, effect.Function, ref newValue, ref runningSum, ref summandCount);
                propertyDescription.SetValue(newValue, targetComponent);
            }
        }

        private void UpdatePropertyValue(
            PropertyDescription<bool> propertyDescription,
            GameEntity targetEntity,
            EffectComponent effect,
            GameManager manager)
        {
            bool newValue;
            var runningSum = 0;
            var summandCount = 0;
            var targetComponent = targetEntity.FindComponent(propertyDescription.ComponentId);
            if (propertyDescription.IsCalculated)
            {
                var activeEffects = GetSortedAppliedEffects(targetEntity, propertyDescription, effect, manager);
                newValue = propertyDescription.DefaultValue ?? default;
                if (activeEffects != null)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var index = 0; index < activeEffects.Count; index++)
                    {
                        var activeEffect = activeEffects[index];

                        Apply(activeEffect.Amount.Value != 0, activeEffect.Function, ref newValue, ref runningSum,
                            ref summandCount);
                    }
                }
                else if (effect != null)
                {
                    Apply(effect.Amount.Value != 0, effect.Function, ref newValue, ref runningSum, ref summandCount);
                }

                propertyDescription.SetValue(newValue, targetComponent);
            }
            else
            {
                newValue = propertyDescription.GetValue(targetComponent);
                Apply(effect.Amount.Value != 0, effect.Function, ref newValue, ref runningSum, ref summandCount);
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

        private List<EffectComponent> GetSortedAppliedEffects(
            GameEntity targetEntity, PropertyDescription propertyDescription, EffectComponent effect,
            GameManager manager)
        {
            List<EffectComponent> activeEffects = null;
            foreach (var otherEffectEntity in manager.AppliedEffectsToAffectableEntityRelationship[targetEntity.Id])
            {
                var otherEffect = otherEffectEntity.Effect;
                if (otherEffect != effect
                    && otherEffect.EffectType == EffectType.ChangeProperty
                    && otherEffect.TargetName.Equals(propertyDescription.Name))
                {
                    activeEffects = new List<EffectComponent>();
                    activeEffects.Add(otherEffect);
                }
            }

            if (activeEffects == null)
            {
                return null;
            }

            if (effect == null)
            {
                activeEffects.Add(effect);
            }

            activeEffects.Sort(AppliedEffectComponentComparer.Instance);
            return activeEffects;
        }

        private class AppliedEffectComponentComparer : IComparer<EffectComponent>
        {
            public static readonly AppliedEffectComponentComparer Instance = new AppliedEffectComponentComparer();

            private AppliedEffectComponentComparer()
            {
            }

            public int Compare(EffectComponent x, EffectComponent y)
            {
                var game = x.Entity.Manager;
                var xAbility = x.SourceAbilityId.HasValue
                    ? game.FindEntity(x.SourceAbilityId.Value).Ability
                    : null;
                var yAbility = y.SourceAbilityId.HasValue
                    ? game.FindEntity(y.SourceAbilityId.Value).Ability
                    : null;

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

                var result = y.Function - x.Function;
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
}
