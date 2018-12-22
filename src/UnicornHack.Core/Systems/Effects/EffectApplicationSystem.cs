﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects
{
    public class EffectApplicationSystem :
        IGameSystem<AbilityActivatedMessage>,
        IGameSystem<EntityAddedMessage<GameEntity>>,
        IGameSystem<EntityRemovedMessage<GameEntity>>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, int?>>
    {
        public const string EffectsAppliedMessageName = "EffectsApplied";

        private readonly PropertyValueCalculator _propertyCalculator;

        public EffectApplicationSystem(PropertyValueCalculator propertyCalculator)
            => _propertyCalculator = propertyCalculator;

        public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager manager)
        {
            var effectsAppliedMessage = manager.Queue.CreateMessage<EffectsAppliedMessage>(EffectsAppliedMessageName);
            effectsAppliedMessage.ActivatorEntity = message.ActivatorEntity;
            effectsAppliedMessage.TargetEntity = message.TargetEntity;
            effectsAppliedMessage.AbilityEntity = message.AbilityEntity;
            effectsAppliedMessage.AbilityTrigger = message.Trigger;
            effectsAppliedMessage.SuccessfulApplication = message.SuccessfulApplication;

            var appliedEffects = new ReferencingList<GameEntity>();
            // TODO: Group effects by duration so they can be applied in proper order
            // TODO: Fire a message for effects that are DuringApplication, so they get unapplied as soon as the previous messages are processed
            Dictionary<EffectType, (int Damage, EffectComponent Effect)> damageEffects = null;
            foreach (var effectEntity in message.EffectsToApply)
            {
                var effect = effectEntity.Effect;
                Debug.Assert(!effect.ShouldTargetActivator || message.ActivatorEntity == message.TargetEntity);
                if (!message.SuccessfulApplication
                    && effect.EffectType != EffectType.Activate
                    && effect.EffectType != EffectType.Move)
                {
                    continue;
                }

                switch (effect.EffectType)
                {
                    case EffectType.ChangeProperty:
                        var propertyDescription = PropertyDescription.Loader.Find(effect.TargetName);
                        Debug.Assert(propertyDescription.IsCalculated == (effect.DurationTicks != 0));

                        using (var changedPropertyReference = ApplyEffect(effect, appliedEffects, manager))
                        {
                            changedPropertyReference.Referenced.Effect.AffectedEntityId = message.TargetEntity.Id;
                        }

                        break;
                    case EffectType.AddAbility:
                        using (var abilityAddedReference = ApplyEffect(effect, appliedEffects, manager))
                        {
                            var ability = effectEntity.Ability.AddToEffect(abilityAddedReference.Referenced);

                            ability.OwnerId = message.TargetEntity.Id;
                            abilityAddedReference.Referenced.Effect.AffectedEntityId = message.TargetEntity.Id;
                        }

                        break;
                    case EffectType.ChangeRace:
                        var remove = effect.Amount == -1;
                        var raceName = effect.TargetName;
                        if (remove)
                        {
                            Debug.Assert(effect.DurationTicks == 0);

                            var raceEntity = manager.RacesToBeingRelationship[message.TargetEntity.Id].Values
                                .SingleOrDefault(r => r.Race.TemplateName == raceName);
                            raceEntity?.RemoveComponent(EntityComponent.Effect);
                        }
                        else
                        {
                            Debug.Assert(effect.DurationTicks != 0);

                            if (!message.TargetEntity.HasComponent(EntityComponent.Player))
                            {
                                continue;
                            }

                            var raceDefinition = PlayerRace.Loader.Find(raceName);

                            var existingRace = manager.RacesToBeingRelationship[message.TargetEntity.Id].Values
                                .SingleOrDefault(r => r.Race.Species == raceDefinition.Species);
                            if (existingRace == null)
                            {
                                using (var addedRaceEntityReference =
                                    ApplyEffect(effect, appliedEffects, manager))
                                {
                                    var addedRaceEffect = addedRaceEntityReference.Referenced.Effect;
                                    addedRaceEffect.Amount = null;

                                    var addedRace = raceDefinition.AddToAppliedEffect(
                                        addedRaceEntityReference.Referenced, message.TargetEntity.Id);

                                    addedRaceEffect.AffectedEntityId = message.TargetEntity.Id;
                                }
                            }

                            // TODO: Replace subrace
                        }

                        break;
                    case EffectType.Burn:
                    case EffectType.Corrode:
                    case EffectType.Disintegrate:
                    case EffectType.Blight:
                    case EffectType.Freeze:
                    case EffectType.Bleed:
                    case EffectType.Shock:
                    case EffectType.Soak:
                    case EffectType.MagicalDamage:
                    case EffectType.PhysicalDamage:
                        Debug.Assert(effect.DurationTicks == 0);

                        if (damageEffects == null)
                        {
                            damageEffects = new Dictionary<EffectType, (int Damage, EffectComponent Effect)>();
                        }

                        var key = effect.EffectType;
                        if (!damageEffects.TryGetValue(key, out var previousDamage))
                        {
                            previousDamage = (0, null);
                        }

                        // TODO: Take effectComponent.Function into account
                        damageEffects[key] = (effect.Amount.Value + previousDamage.Damage, effect);
                        break;
                    case EffectType.Move:
                        var movee = manager.FindEntity(effect.TargetEntityId.Value);
                        if (movee.HasComponent(EntityComponent.Item))
                        {
                            var moveItemMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
                            moveItemMessage.ItemEntity = movee;
                            moveItemMessage.SuppressLog = true;

                            if (message.TargetEntity != null)
                            {
                                var position = message.TargetEntity.Position;
                                moveItemMessage.TargetLevelEntity = position.LevelEntity;
                                moveItemMessage.TargetCell = position.LevelCell;
                            }
                            else
                            {
                                Debug.Assert(message.TargetCell != null);

                                moveItemMessage.TargetLevelEntity = message.ActivatorEntity.Position.LevelEntity;
                                moveItemMessage.TargetCell = message.TargetCell;
                            }

                            manager.Enqueue(moveItemMessage);
                        }

                        break;
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
                        }

                        break;
                    case EffectType.Heal:
                    case EffectType.GainXP:
                    case EffectType.Activate:
                        using (var effectReference = ApplyEffect(effect, appliedEffects, manager))
                        {
                            effectReference.Referenced.Effect.AffectedEntityId = message.TargetEntity.Id;
                        }
                        break;
                    case EffectType.DrainEnergy:
                    case EffectType.DrainLife:
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
                }
            }

            if (damageEffects != null)
            {
                foreach (var damageEffect in damageEffects)
                {
                    var being = message.TargetEntity.Being;
                    if (being == null)
                    {
                        continue;
                    }

                    switch (damageEffect.Key)
                    {
                        case EffectType.Burn:
                        case EffectType.Corrode:
                        case EffectType.Disintegrate:
                        case EffectType.Blight:
                        case EffectType.Freeze:
                        case EffectType.Bleed:
                        case EffectType.Shock:
                        case EffectType.Soak:
                        case EffectType.MagicalDamage:
                        case EffectType.PhysicalDamage:
                            using (var appliedEffectEntityReference = ApplyDamageEffect(
                                damageEffect.Value.Effect,
                                damageEffect.Value.Damage,
                                being, appliedEffects, manager))
                            {
                                if (appliedEffectEntityReference != null)
                                {
                                    appliedEffectEntityReference.Referenced.Effect.AffectedEntityId =
                                        message.TargetEntity.Id;
                                }
                            }

                            break;
                    }
                }
            }

            effectsAppliedMessage.AppliedEffects = appliedEffects;
            manager.Enqueue(effectsAppliedMessage);

            return MessageProcessingResult.ContinueProcessing;
        }

        private ITransientReference<GameEntity> ApplyEffect(
            EffectComponent effectComponent,
            ReferencingList<GameEntity> appliedEffects,
            GameManager manager)
        {
            var appliedEffectEntity = manager.CreateEntity();
            var entity = appliedEffectEntity.Referenced;
            entity.Effect = CreateAppliedEffect(effectComponent, manager);

            appliedEffects.Add(entity);

            return appliedEffectEntity;
        }

        private EffectComponent CreateAppliedEffect(EffectComponent effectComponent, GameManager manager)
        {
            var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            appliedEffect.ShouldTargetActivator = effectComponent.ShouldTargetActivator;
            appliedEffect.SourceAbilityId = effectComponent.ContainingAbilityId;
            appliedEffect.Amount = effectComponent.Amount;
            appliedEffect.DurationTicks = effectComponent.DurationTicks;
            if (effectComponent.DurationTicks > 0)
            {
                appliedEffect.ExpirationTick = manager.Game.CurrentTick + effectComponent.DurationTicks;
            }

            appliedEffect.EffectType = effectComponent.EffectType;
            appliedEffect.Function = effectComponent.Function;
            appliedEffect.TargetName = effectComponent.TargetName;
            appliedEffect.TargetEntityId = effectComponent.TargetEntityId;

            return appliedEffect;
        }

        private ITransientReference<GameEntity> ApplyDamageEffect(
            EffectComponent effect, int damage, BeingComponent being, ReferencingList<GameEntity> appliedEffects,
            GameManager manager)
        {
            var absorption = 0;
            var resistance = 0;
            switch (effect.EffectType)
            {
                case EffectType.Burn:
                    // TODO: Burns items
                    // TODO: Removes slime, wet, frozen
                    resistance = being.FireResistance;
                    break;
                case EffectType.Corrode:
                    // TODO: Corrodes items
                    // TODO: Removes stoning
                    resistance = being.AcidResistance;
                    break;
                case EffectType.Disintegrate:
                    // TODO: Withers items
                    resistance = being.DisintegrationResistance;
                    break;
                case EffectType.Blight:
                    // TODO: Decays items
                    resistance = being.BlightResistance;
                    break;
                case EffectType.Freeze:
                    // TODO: Freezes items
                    // TODO: Slows, removes burning, dissolving
                    resistance = being.ColdResistance;
                    break;
                case EffectType.Bleed:
                    resistance = being.BleedingResistance;
                    break;
                case EffectType.Shock:
                    // TODO: Causing some mechanical and magical items to trigger
                    // TODO: Removes slow
                    resistance = being.ElectricityResistance;
                    break;
                case EffectType.Soak:
                    // TODO: Rusts items
                    // TODO: Removes burning
                    resistance = being.WaterResistance;
                    break;
                case EffectType.MagicalDamage:
                    absorption = being.MagicAbsorption;
                    resistance = being.MagicResistance;
                    break;
                case EffectType.PhysicalDamage:
                    absorption = being.PhysicalAbsorption;
                    resistance = being.PhysicalResistance;
                    break;
            }

            damage -= absorption;

            if (damage < 0)
            {
                damage = 0;
            }

            damage = (damage * (100 - resistance)) / 100;

            if (damage != 0)
            {
                var appliedEffectEntityReference = being.Entity.Manager.CreateEntity();
                var appliedEffect = CreateAppliedEffect(effect, manager);
                appliedEffect.Amount = damage;

                appliedEffectEntityReference.Referenced.Effect = appliedEffect;
                appliedEffects.Add(appliedEffectEntityReference.Referenced);

                return appliedEffectEntityReference;
            }

            return null;
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
            var effect = message.Entity.Effect;
            if (effect.AffectedEntityId != null)
            {
                Debug.Assert(effect.DurationTicks != (int)EffectDuration.Instant);
                ProcessEffectChanges(message.Entity, effect, manager, State.Modified);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private MessageProcessingResult ProcessEffectChanges(
            GameEntity effectEntity, EffectComponent appliedEffectComponent, GameManager manager, State state)
        {
            if (appliedEffectComponent.AffectedEntityId == null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var targetEntity = manager.FindEntity(appliedEffectComponent.AffectedEntityId.Value);
            if (targetEntity == null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            switch (appliedEffectComponent.EffectType)
            {
                case EffectType.ChangeProperty:
                    if (state == State.Removed
                        && appliedEffectComponent.DurationTicks == (int)EffectDuration.Instant)
                    {
                        break;
                    }

                    var propertyDescription = PropertyDescription.Loader.Find(appliedEffectComponent.TargetName);
                    UpdateProperty(propertyDescription, targetEntity, appliedEffectComponent, manager);

                    break;
                case EffectType.AddAbility:
                    if (state == State.Removed)
                    {
                        effectEntity.RemoveComponent(EntityComponent.Ability);
                    }

                    break;
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
                case EffectType.Disintegrate:
                case EffectType.Blight:
                case EffectType.Freeze:
                case EffectType.Bleed:
                case EffectType.Shock:
                case EffectType.Soak:
                case EffectType.MagicalDamage:
                case EffectType.PhysicalDamage:
                    if (state == State.Added)
                    {
                        var being = targetEntity.Being;
                        being.HitPoints -= appliedEffectComponent.Amount.Value;
                    }

                    break;
                case EffectType.Heal:
                    if (state == State.Added)
                    {
                        var being = targetEntity.Being;
                        being.HitPoints += appliedEffectComponent.Amount.Value;
                    }

                    break;
                case EffectType.GainXP:
                    if (state == State.Added)
                    {
                        manager.XPSystem.AddPlayerXP(appliedEffectComponent.Amount.Value, manager);
                    }

                    break;
                case EffectType.Activate:
                    break;
                case EffectType.DrainEnergy:
                case EffectType.DrainLife:
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
                    throw new InvalidOperationException($"Effect {effectEntity.Id} of type {appliedEffectComponent.EffectType} not handled.");
            }

            if (state == State.Added && appliedEffectComponent.DurationTicks == 0)
            {
                appliedEffectComponent.AffectedEntityId = null;
                var removeComponentMessage = manager.CreateRemoveComponentMessage();
                removeComponentMessage.Entity = effectEntity;
                removeComponentMessage.Component = EntityComponent.Effect;

                manager.Enqueue(removeComponentMessage, lowPriority: true);
            }

            return MessageProcessingResult.ContinueProcessing;
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

        private void UpdateProperty(PropertyDescription propertyDescription, GameEntity targetEntity,
            EffectComponent appliedEffectComponent, GameManager manager)
        {
            if (propertyDescription.PropertyType == typeof(bool))
            {
                _propertyCalculator.UpdatePropertyValue(
                    (PropertyDescription<bool>)propertyDescription,
                    targetEntity,
                    appliedEffectComponent,
                    manager);
            }
            else
            {
                _propertyCalculator.UpdatePropertyValue(
                    (PropertyDescription<int>)propertyDescription,
                    targetEntity,
                    appliedEffectComponent,
                    manager);
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
