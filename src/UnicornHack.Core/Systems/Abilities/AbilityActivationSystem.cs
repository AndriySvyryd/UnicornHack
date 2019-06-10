using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class AbilityActivationSystem :
        IGameSystem<ActivateAbilityMessage>,
        IGameSystem<DeactivateAbilityMessage>,
        IGameSystem<ItemEquippedMessage>,
        IGameSystem<DiedMessage>,
        IGameSystem<LeveledUpMessage>,
        IGameSystem<EntityAddedMessage<GameEntity>>,
        IGameSystem<EntityRemovedMessage<GameEntity>>
    {
        public bool CanActivateAbility(ActivateAbilityMessage activateAbilityMessage, bool shouldThrow)
        {
            using (var message = Activate(activateAbilityMessage, pretend: true, stats: null))
            {
                if (!string.IsNullOrEmpty(message.ActivationError) && shouldThrow)
                {
                    throw new InvalidOperationException(message.ActivationError);
                }

                return string.IsNullOrEmpty(message.ActivationError);
            }
        }

        public AttackStats GetAttackStats(ActivateAbilityMessage activateAbilityMessage)
        {
            var stats = new AttackStats();
            var activated = Activate(activateAbilityMessage, pretend: true, stats);
            return string.IsNullOrEmpty(activated?.ActivationError) ? stats : null;
        }

        MessageProcessingResult IMessageConsumer<ActivateAbilityMessage, GameManager>.Process(
            ActivateAbilityMessage message, GameManager manager)
        {
            var abilityActivatedMessage = Activate(message, pretend: false, stats: null);
            if (abilityActivatedMessage != null)
            {
                if (!string.IsNullOrEmpty(abilityActivatedMessage.ActivationError))
                {
                    throw new InvalidOperationException(abilityActivatedMessage.ActivationError);
                }

                if (abilityActivatedMessage.ActivationError == null)
                {
                    manager.Enqueue(abilityActivatedMessage);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private AbilityActivatedMessage Activate(ActivateAbilityMessage activateMessage, bool pretend, AttackStats stats)
        {
            var manager = activateMessage.AbilityEntity.Manager;
            var targetEffectsMessage = AbilityActivatedMessage.Create(manager);
            targetEffectsMessage.AbilityEntity = activateMessage.AbilityEntity;
            targetEffectsMessage.ActivatorEntity = activateMessage.ActivatorEntity;
            targetEffectsMessage.TargetCell = activateMessage.TargetCell;
            targetEffectsMessage.TargetEntity = activateMessage.TargetEntity;

            var ability = activateMessage.AbilityEntity.Ability;
            if (!ability.IsUsable)
            {
                targetEffectsMessage.ActivationError = $"Ability {ability.EntityId} is not usable or is not slotted.";
                return targetEffectsMessage;
            }

            if (ability.Slot == null
                && (ability.Activation & ActivationType.Slottable) != 0
                && (ability.OwnerEntity.Being?.AbilitySlotCount ?? 0) != 0)
            {
                targetEffectsMessage.ActivationError = $"Ability {ability.EntityId} is not slotted.";
                return targetEffectsMessage;
            }

            if (ability.IsActive)
            {
                targetEffectsMessage.ActivationError = $"Ability {ability.EntityId} is already active.";
                return targetEffectsMessage;
            }

            if (ability.CooldownTick != null
                || ability.CooldownXpLeft != null)
            {
                targetEffectsMessage.ActivationError = $"Ability {ability.EntityId} is on cooldown.";
                return targetEffectsMessage;
            }

            if (manager.EffectsToContainingAbilityRelationship[ability.EntityId].Count == 0)
            {
                targetEffectsMessage.ActivationError = "";
                return targetEffectsMessage;
            }

            var being = ability.OwnerEntity.Being ?? targetEffectsMessage.ActivatorEntity.Being;
            if (ability.EnergyCost > 0
                && being != null)
            {
                if (being.EnergyPoints < ability.EnergyCost)
                {
                    targetEffectsMessage.ActivationError = $"Not enough EP to activate ability {ability.EntityId}.";
                    return targetEffectsMessage;
                }

                if (!pretend)
                {
                    if ((ability.Activation & ActivationType.Continuous) != 0)
                    {
                        being.ReservedEnergyPoints += ability.EnergyCost;
                    }
                    else
                    {
                        being.EnergyPoints -= ability.EnergyCost;
                    }
                }
            }

            var delay = 0;
            if ((ability.Activation & ActivationType.Slottable) != 0)
            {
                delay = ability.GetActualDelay(activateMessage.ActivatorEntity);
                if (delay == -1)
                {
                    targetEffectsMessage.ActivationError = $"Speed too low to activate ability {ability.EntityId}.";
                    return targetEffectsMessage;
                }

                if (stats != null)
                {
                    stats.Delay = delay;
                }

                var activatorPosition = activateMessage.ActivatorEntity.Position;
                if (activatorPosition != null)
                {
                    var requiredHeading = GetRequiredHeading(ability, activatorPosition,
                        activateMessage.TargetCell ?? activateMessage.TargetEntity.Position.LevelCell);
                    if (requiredHeading != activatorPosition.Heading)
                    {
                        var travelMessage = TravelMessage.Create(manager);
                        travelMessage.Entity = activateMessage.ActivatorEntity;
                        travelMessage.TargetHeading = requiredHeading;
                        travelMessage.TargetCell = activatorPosition.LevelCell;

                        if (!manager.TravelSystem.CanTravel(travelMessage, manager))
                        {
                            targetEffectsMessage.ActivationError = $"Unable to turn in order to activate ability {ability.EntityId}.";
                            manager.ReturnMessage(travelMessage);
                            return targetEffectsMessage;
                        }

                        if (!pretend)
                        {
                            manager.Process(travelMessage);
                        }
                    }
                }
            }

            if (pretend
                && stats == null)
            {
                return targetEffectsMessage;
            }

            var selfEffectsMessage = AbilityActivatedMessage.Create(manager);
            selfEffectsMessage.AbilityEntity = activateMessage.AbilityEntity;
            selfEffectsMessage.ActivatorEntity = activateMessage.ActivatorEntity;
            selfEffectsMessage.TargetEntity = activateMessage.ActivatorEntity;
            selfEffectsMessage.EffectsToApply = ImmutableList.Create<GameEntity>();

            var abilityTrigger = ability.Trigger == ActivationType.Default
                ? GetTrigger(ability)
                : ability.Trigger;

            targetEffectsMessage.Trigger = abilityTrigger;
            targetEffectsMessage.EffectsToApply = ImmutableList.Create<GameEntity>();

            AddTriggeredEffects(targetEffectsMessage, selfEffectsMessage, ability.EntityId, abilityTrigger, manager);

            var activatedAbilities = new HashSet<AbilityComponent>();
            var activations = GetActivations(targetEffectsMessage, selfEffectsMessage, activatedAbilities);

            if (!pretend)
            {
                foreach (var activatedAbility in activatedAbilities)
                {
                    ChangeState(active: true, activatedAbility, activateMessage.ActivatorEntity, manager);
                }

                if (!selfEffectsMessage.EffectsToApply.IsEmpty)
                {
                    manager.Process(selfEffectsMessage);
                }
                else
                {
                    manager.ReturnMessage(selfEffectsMessage);
                }
            }
            else
            {
                manager.ReturnMessage(selfEffectsMessage);
            }

            foreach (var activation in activations)
            {
                if (!pretend
                    && activation.ActivationMessage.TargetEntity == null)
                {
                    manager.Enqueue(activation.ActivationMessage);
                }

                foreach (var (target, exposure) in GetTargets(activation.ActivationMessage))
                {
                    var targetMessage = (activation.TargetMessage ?? activation.ActivationMessage).Clone(manager);
                    targetMessage.TargetEntity = target;

                    if (pretend)
                    {
                        if (target == activateMessage.TargetEntity)
                        {
                            var melee = activation.ActivationMessage.AbilityEntity.Ability.Range == 1;
                            var hitProbability = DetermineOutcome(
                                targetMessage,
                                melee ? BeveledVisibilityCalculator.MaxVisibility : exposure,
                                pretend: true);
                            stats.HitProbabilities.Add(hitProbability);

                            var damage = manager.EffectApplicationSystem.GetExpectedDamage(targetMessage, manager);
                            stats.Damages.Add(damage);

                            manager.ReturnMessage(targetMessage);
                        }
                        else
                        {
                            manager.ReturnMessage(targetMessage);
                        }
                    }
                    else
                    {
                        DetermineOutcome(targetMessage, exposure, pretend: false);

                        // TODO: Trigger abilities on target
                        manager.Enqueue(targetMessage);
                    }
                }

                if (activation.ActivationMessage.TargetEntity != null)
                {
                    manager.ReturnMessage(activation.ActivationMessage);
                }

                if (activation.TargetMessage != null)
                {
                    manager.ReturnMessage(activation.TargetMessage);
                }
            }

            if (!pretend)
            {
                DelayMessage.Enqueue(activateMessage.ActivatorEntity, delay, manager);
            }

            return null;
        }

        private Direction GetRequiredHeading(
            AbilityComponent ability,
            PositionComponent activatorPosition,
            Point targetCell)
        {
            switch (ability.Activation)
            {
                case ActivationType.Manual:
                case ActivationType.WhileToggled:
                    return activatorPosition.Heading.Value;
                case ActivationType.Targeted:
                    var targetDirection = activatorPosition.LevelCell.DifferenceTo(targetCell);
                    if (targetDirection.Length() == 0)
                    {
                        return activatorPosition.Heading.Value;
                    }

                    var octantsToTurn = 0;
                    var targetOctantDifference = targetDirection.OctantsTo(activatorPosition.Heading.Value);
                    var maxOctantDifference = ability.HeadingDeviation;
                    if (targetOctantDifference > maxOctantDifference)
                    {
                        octantsToTurn = targetOctantDifference - maxOctantDifference;
                    }
                    else if (targetOctantDifference < -maxOctantDifference)
                    {
                        octantsToTurn = targetOctantDifference + maxOctantDifference;
                    }

                    var newDirection = (int)activatorPosition.Heading.Value - octantsToTurn;
                    if (newDirection < 0)
                    {
                        newDirection += 8;
                    }
                    else if (newDirection > 8)
                    {
                        newDirection -= 8;
                    }

                    return (Direction)newDirection;
                default:
                    throw new InvalidOperationException(
                        $"Ability {ability.Name} on entity {activatorPosition.EntityId} cannot be activated manually.");
            }
        }

        private List<AbilityActivation> GetActivations(
            AbilityActivatedMessage activationMessage,
            AbilityActivatedMessage selfEffectsMessage,
            HashSet<AbilityComponent> activatedAbilities)
        {
            var activations = new List<AbilityActivation>();

            activatedAbilities.Add(activationMessage.AbilityEntity.Ability);
            GetActivations(activationMessage, null, selfEffectsMessage, null, activatedAbilities, activations);

            return activations;
        }

        private void GetActivations(
            AbilityActivatedMessage activationMessage,
            AbilityActivatedMessage targetMessage,
            AbilityActivatedMessage selfEffectsMessage,
            GameEntity activatingEffect,
            HashSet<AbilityComponent> activatedAbilities,
            List<AbilityActivation> activations)
        {
            var manager = activationMessage.AbilityEntity.Manager;
            var messageToProcess = targetMessage ?? activationMessage;
            var perAbilityActivations = GetActivatedAbilities(messageToProcess, selfEffectsMessage);
            if (perAbilityActivations != null)
            {
                foreach (var subActivation in perAbilityActivations)
                {
                    activatedAbilities.Add(subActivation.ActivatedAbility.Ability);

                    var subAbilityMessage = messageToProcess.Clone(manager);
                    subAbilityMessage.AbilityEntity = subActivation.ActivatedAbility;

                    AbilityActivatedMessage nextActivationMessage;
                    var nextTargetMessage = targetMessage;
                    if (subActivation.ActivatedAbility.Ability.Range != 0
                        && nextTargetMessage == null)
                    {
                        nextActivationMessage = subAbilityMessage;
                    }
                    else
                    {
                        nextActivationMessage = activationMessage.Clone(manager);
                        if (nextTargetMessage == null
                            && activatingEffect != null)
                        {
                            activationMessage.EffectsToApply = activationMessage.EffectsToApply.Add(activatingEffect);
                        }

                        nextTargetMessage = subAbilityMessage;
                    }

                    GetActivations(nextActivationMessage, nextTargetMessage, selfEffectsMessage,
                        subActivation.ActivatingEffect, activatedAbilities, activations);
                }

                manager.ReturnMessage(activationMessage);
                if (targetMessage != null)
                {
                    manager.ReturnMessage(targetMessage);
                }
            }
            else
            {
                Debug.Assert(activationMessage.AbilityEntity.Ability.Action != AbilityAction.Modifier);
                if (activatingEffect != null)
                {
                    if (targetMessage == null)
                    {
                        activationMessage.EffectsToApply = activationMessage.EffectsToApply.Add(activatingEffect);
                    }
                    else
                    {
                        targetMessage.EffectsToApply = targetMessage.EffectsToApply.Add(activatingEffect);
                    }
                }

                activations.Add(new AbilityActivation
                {
                    ActivationMessage = activationMessage,
                    TargetMessage = targetMessage
                });
            }
        }

        private List<(GameEntity ActivatingEffect, GameEntity ActivatedAbility)> GetActivatedAbilities(
            AbilityActivatedMessage targetEffectsMessage,
            AbilityActivatedMessage selfEffectsMessage)
        {
            var manager = targetEffectsMessage.ActivatorEntity.Manager;
            var abilityTrigger = targetEffectsMessage.Trigger;
            List<(GameEntity Effect, GameEntity Ability)> activatedAbilities = null;

            foreach (var effectEntity in
                manager.EffectsToContainingAbilityRelationship[targetEffectsMessage.AbilityEntity.Id])
            {
                var effect = effectEntity.Effect;
                if (effect.EffectType == EffectType.Activate)
                {
                    var activatableId = effect.TargetEntityId.Value;
                    var activatable = manager.FindEntity(activatableId);
                    if (activatable == null)
                    {
                        throw new InvalidOperationException(
                            $"Couldn't find the entity to activate '{activatableId}' referenced from the effect '{effectEntity.Id}'"
                            + $" on ability '{effect.ContainingAbilityId}'");
                    }

                    Debug.Assert(abilityTrigger != ActivationType.Default
                                 || activatable.HasComponent(EntityComponent.Ability));

                    var triggeredAbility = activatable.HasComponent(EntityComponent.Ability)
                        ? activatable
                        : GetSingleTriggeredAbility(abilityTrigger, activatableId, manager);

                    if (activatedAbilities == null)
                    {
                        activatedAbilities = new List<(GameEntity, GameEntity)>();
                    }

                    activatedAbilities.Add((effectEntity, triggeredAbility));
                }
                else if (effect.ShouldTargetActivator)
                {
                    selfEffectsMessage.EffectsToApply = selfEffectsMessage.EffectsToApply.Add(effectEntity);
                }
                else
                {
                    targetEffectsMessage.EffectsToApply = targetEffectsMessage.EffectsToApply.Add(effectEntity);
                }
            }

            return activatedAbilities;
        }

        public MessageProcessingResult Process(ItemEquippedMessage message, GameManager manager)
        {
            if (!message.Successful)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            if (message.Slot != EquipmentSlot.None)
            {
                var activation = ActivateAbilityMessage.Create(manager);
                activation.ActivatorEntity = message.ActorEntity;
                activation.TargetEntity = message.ActorEntity;

                if (ActivateAbilities(
                    message.ItemEntity.Id, ActivationType.WhileEquipped, activation, manager, pretend: true))
                {
                    ActivateAbilities(
                        message.ItemEntity.Id, ActivationType.WhileEquipped, activation, manager, pretend: false);
                    manager.ReturnMessage(activation);
                }
                else
                {
                    manager.ReturnMessage(activation);

                    var unequipMessage = EquipItemMessage.Create(manager);
                    unequipMessage.ActorEntity = message.ActorEntity;
                    unequipMessage.ItemEntity = message.ItemEntity;
                    unequipMessage.SuppressLog = true;

                    // TODO: Log message
                    manager.Enqueue(unequipMessage);
                    return MessageProcessingResult.StopProcessing;
                }
            }
            else
            {
                DeactivateAbilities(message.ItemEntity.Id, ActivationType.WhileEquipped, message.ActorEntity, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            var beingEntity = message.BeingEntity;
            if (!beingEntity.HasComponent(EntityComponent.Player))
            {
                DeactivateAbilities(beingEntity.Id, ActivationType.Continuous, beingEntity, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(LeveledUpMessage message, GameManager manager)
        {
            // TODO: Use a leveled ability
            foreach (var abilityEntity in GetTriggeredAbilities(
                message.Entity.Id, ActivationType.WhileAboveLevel, manager))
            {
                var ability = abilityEntity.Ability;
                if (ability.IsActive)
                {
                    continue;
                }

                var newLevel = GetSourceRace(abilityEntity)?.Level ??
                               manager.XPSystem.GetXPLevel(message.Entity, manager);
                if (newLevel < ability.ActivationCondition)
                {
                    continue;
                }

                var activation = ActivateAbilityMessage.Create(manager);
                activation.ActivatorEntity = ability.OwnerEntity;
                activation.TargetEntity = ability.OwnerEntity;
                activation.AbilityEntity = abilityEntity;
                manager.Process(activation);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private RaceComponent GetSourceRace(GameEntity abilityEntity)
        {
            var manager = abilityEntity.Manager;

            while (true)
            {
                var race = abilityEntity.Race;
                if (race != null)
                {
                    return race;
                }

                var effect = abilityEntity.Effect;
                if (effect == null)
                {
                    return null;
                }

                abilityEntity = manager.FindEntity(effect.SourceAbilityId);
            }
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
        {
            switch (message.Group.Name)
            {
                case nameof(GameManager.AbilitiesToAffectableRelationship):
                    var ability = message.Entity.Ability;
                    if (ability != null
                        && (ability.Activation & ActivationType.Always) != 0
                        && !ability.IsActive
                        && manager.EffectsToContainingAbilityRelationship[ability.EntityId].Count != 0)
                    {
                        Debug.Assert(!ability.IsActive);

                        Activate(ability, manager);
                    }

                    break;
                case nameof(GameManager.EntityItemsToContainerRelationship):
                    if (message.ReferencedEntity != null
                        && message.ReferencedEntity.HasComponent(EntityComponent.Being))
                    {
                        var activation = ActivateAbilityMessage.Create(manager);
                        activation.ActivatorEntity = message.ReferencedEntity;
                        activation.TargetEntity = message.ReferencedEntity;

                        ActivateAbilities(message.Entity.Id, ActivationType.WhilePossessed, activation, manager, pretend: false);
                        manager.ReturnMessage(activation);
                    }

                    break;
                case nameof(GameManager.Effects):
                    var effect = message.Entity.Effect;
                    if (effect.ContainingAbilityId != null)
                    {
                        var containingAbility = manager.FindEntity(effect.ContainingAbilityId)?.Ability;
                        if (containingAbility != null
                            && (containingAbility.Activation & ActivationType.Always) != 0
                            && !containingAbility.IsActive
                            && manager.EffectsToContainingAbilityRelationship[containingAbility.EntityId].Count == 1)
                        {
                            Activate(containingAbility, manager);
                        }
                    }

                    break;
                default:
                    throw new InvalidOperationException(message.Group.Name);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        // TODO: Detect when IsUsable changes to false and deactivate

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager manager)
        {
            switch (message.Group.Name)
            {
                case nameof(GameManager.AbilitiesToAffectableRelationship):
                    var ability = message.ChangedComponent as AbilityComponent ?? message.Entity.Ability;
                    if (ability != null
                        && (ability.Activation & ActivationType.Continuous) != 0
                        && ability.IsActive)
                    {
                        Deactivate(ability, message.ReferencedEntity, manager);
                    }

                    break;
                case nameof(GameManager.EntityItemsToContainerRelationship):
                    if (message.ReferencedEntity?.HasComponent(EntityComponent.Being) == true)
                    {
                        DeactivateAbilities(
                            message.Entity.Id, ActivationType.WhilePossessed, message.ReferencedEntity, manager);
                    }

                    break;
                case nameof(GameManager.Effects):
                    var effect = (EffectComponent)message.ChangedComponent;
                    if (effect.ContainingAbilityId != null)
                    {
                        var containingAbility = manager
                            .FindEntity(effect.ContainingAbilityId)?.Ability;
                        if (containingAbility != null
                            && (containingAbility.Activation & ActivationType.Continuous) != 0
                            && containingAbility.IsActive
                            && manager.EffectsToContainingAbilityRelationship[containingAbility.EntityId].Count == 0)
                        {
                            Deactivate(containingAbility, message.ReferencedEntity, manager);
                        }
                    }

                    break;
                default:
                    throw new InvalidOperationException(message.Group.Name);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private void Activate(AbilityComponent ability, GameManager manager)
        {
            var activation = ActivateAbilityMessage.Create(manager);
            activation.ActivatorEntity = ability.OwnerEntity;
            activation.TargetEntity = ability.OwnerEntity;
            activation.AbilityEntity = ability.Entity;

            manager.Process(activation);
        }

        public bool ActivateAbilities(
            int activatableId,
            ActivationType trigger,
            ActivateAbilityMessage activationMessage,
            GameManager manager,
            bool pretend)
        {
            foreach (var triggeredAbility in GetTriggeredAbilities(activatableId, trigger, manager))
            {
                var newActivation = activationMessage.Clone(activationMessage);
                newActivation.AbilityEntity = triggeredAbility;
                if (!pretend)
                {
                    manager.Process(newActivation);
                }
                else if (!CanActivateAbility(newActivation, shouldThrow: false))
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<GameEntity> GetTriggeredAbilities(
            int entityId, ActivationType trigger, GameManager manager)
            => manager.AbilitiesToAffectableRelationship[entityId]
                .Select(a => a.Ability)
                .Where(a => (a.Activation & trigger) != ActivationType.Default)
                .Select(a => a.Entity);

        private GameEntity GetSingleTriggeredAbility(ActivationType trigger, int itemId, GameManager manager)
        {
            var triggeredAbilities = GetTriggeredAbilities(itemId, trigger, manager);

            GameEntity triggeredAbility;
            using (var abilityEnumerator = triggeredAbilities.GetEnumerator())
            {
                if (!abilityEnumerator.MoveNext())
                {
                    throw new InvalidOperationException(
                        $"Item {itemId} has no abilities matching {trigger}");
                }

                triggeredAbility = abilityEnumerator.Current;

                if (abilityEnumerator.MoveNext())
                {
                    throw new InvalidOperationException(
                        $"Item {itemId} has multiple abilities matching {trigger}");
                }
            }

            return triggeredAbility;
        }

        private void AddTriggeredEffects(
            AbilityActivatedMessage abilityActivatedMessage,
            AbilityActivatedMessage selfEffectsMessage,
            int entityId,
            ActivationType trigger,
            GameManager manager)
        {
            foreach (var ability in GetTriggeredAbilities(entityId, trigger, manager))
            {
                // TODO: Trigger non-modifier abilities as separate activations
                Debug.Assert(ability.Ability.Action != AbilityAction.Modifier);

                foreach (var effectEntity in manager.EffectsToContainingAbilityRelationship[ability.Id])
                {
                    if (effectEntity.Effect.ShouldTargetActivator)
                    {
                        selfEffectsMessage.EffectsToApply =
                            selfEffectsMessage.EffectsToApply.Add(effectEntity);
                    }
                    else
                    {
                        abilityActivatedMessage.EffectsToApply =
                            abilityActivatedMessage.EffectsToApply.Add(effectEntity);
                    }
                }
            }
        }

        public static ActivationType GetTrigger(AbilityComponent ability)
        {
            switch (ability.Action)
            {
                case AbilityAction.Default:
                case AbilityAction.Modifier:
                case AbilityAction.Drink:
                    return ActivationType.Default;
                case AbilityAction.Hit:
                case AbilityAction.Slash:
                case AbilityAction.Chop:
                case AbilityAction.Stab:
                case AbilityAction.Poke:
                case AbilityAction.Impale:
                case AbilityAction.Bludgeon:
                case AbilityAction.Punch:
                case AbilityAction.Kick:
                case AbilityAction.Touch:
                case AbilityAction.Headbutt:
                case AbilityAction.Claw:
                case AbilityAction.Bite:
                case AbilityAction.Suck:
                case AbilityAction.Sting:
                case AbilityAction.Hug:
                case AbilityAction.Trample:
                case AbilityAction.Digestion:
                    return ActivationType.OnMeleeAttack;
                case AbilityAction.Shoot:
                case AbilityAction.Throw:
                case AbilityAction.Spit:
                case AbilityAction.Breath:
                case AbilityAction.Gaze:
                case AbilityAction.Scream:
                case AbilityAction.Spell:
                case AbilityAction.Explosion:
                    return ActivationType.OnRangedAttack;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ability), ability.Action.ToString());
            }
        }

        private IReadOnlyCollection<(GameEntity, byte)> GetTargets(in AbilityActivatedMessage activationMessage)
        {
            if (activationMessage.TargetEntity != null)
            {
                return new[] {(activationMessage.TargetEntity, BeveledVisibilityCalculator.MaxVisibility)};
            }

            var activator = activationMessage.ActivatorEntity;
            var targetCell = activationMessage.TargetCell.Value;
            var vectorToTarget = activator.Position.LevelCell.DifferenceTo(targetCell);
            var targetDistance = vectorToTarget.Length();
            if (targetDistance == 0)
            {
                return new[] {(activator, BeveledVisibilityCalculator.MaxVisibility)};
            }

            var manager = activator.Manager;
            var levelId = activator.Position.LevelId;
            var ability = activationMessage.AbilityEntity.Ability;

            switch (ability.TargetingShape)
            {
                // TODO: Handle other shapes
                case TargetingShape.ThreeOctants:
                    // TODO: Handle different ranges
                    if (targetDistance > 1)
                    {
                        return new (GameEntity, byte)[0];
                    }

                    var direction = vectorToTarget.AsDirection();
                    var firstNeighbor = targetCell.Translate(direction.Rotate(1).AsVector());
                    var secondNeighbor = targetCell.Translate(direction.Rotate(-1).AsVector());

                    var targets = new List<(GameEntity, byte)>();
                    var arcTarget = manager.LevelActorToLevelCellIndex[(levelId, targetCell.X, targetCell.Y)];
                    if (arcTarget != null)
                    {
                        targets.Add((arcTarget, BeveledVisibilityCalculator.MaxVisibility));
                    }

                    arcTarget = manager.LevelActorToLevelCellIndex[(levelId, firstNeighbor.X, firstNeighbor.Y)];
                    if (arcTarget != null)
                    {
                        targets.Add((arcTarget, BeveledVisibilityCalculator.MaxVisibility));
                    }

                    arcTarget = manager.LevelActorToLevelCellIndex[(levelId, secondNeighbor.X, secondNeighbor.Y)];
                    if (arcTarget != null)
                    {
                        targets.Add((arcTarget, BeveledVisibilityCalculator.MaxVisibility));
                    }

                    return targets;
                case TargetingShape.Line:
                    return GetLOSTargets(
                        activator,
                        targetCell,
                        activator.Position.LevelEntity,
                        ability.TargetingType,
                        ability.Range);
                default:
                    throw new NotSupportedException("TargetingShape " + ability.TargetingShape + " not supported");
            }
        }

        private static IReadOnlyCollection<(GameEntity, byte)> GetLOSTargets(
            GameEntity sensor, Point targetCell, GameEntity levelEntity, TargetingType targetingType, int range)
        {
            var manager = levelEntity.Manager;
            var los = manager.SensorySystem.GetLOS(sensor, targetCell);
            var level = levelEntity.Level;
            switch (targetingType)
            {
                case TargetingType.Area:
                    var targets = new List<(GameEntity, byte)>();
                    foreach (var (targetIndex, beamExposure) in los)
                    {
                        var beamTargetCell = level.IndexToPoint[targetIndex];
                        var beamTarget =
                            manager.LevelActorToLevelCellIndex[(levelEntity.Id, beamTargetCell.X, beamTargetCell.Y)];
                        if (beamTarget != null
                            && sensor.Position.LevelCell.DifferenceTo(beamTargetCell).Length() <= range)
                        {
                            targets.Add((beamTarget, beamExposure));
                        }
                    }

                    return targets;
                case TargetingType.Edge:
                case TargetingType.Single:
                    // TODO: Target for Single should be the first entity

                    var (lastIndex, exposure) = los.Last();
                    var target = manager.LevelActorToLevelCellIndex[(levelEntity.Id, targetCell.X, targetCell.Y)];
                    if (level.IndexToPoint[lastIndex] != targetCell
                        || target == null
                        || sensor.Position.LevelCell.DifferenceTo(targetCell).Length() > range)
                    {
                        return new (GameEntity, byte)[0];
                    }

                    return new[] {(target, exposure)};
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int DetermineOutcome(AbilityActivatedMessage message, byte exposure, bool pretend)
        {
            if (message.TargetEntity == null)
            {
                message.Outcome = ApplicationOutcome.Miss;
                return 0;
            }

            var deflectionProbability = GetDeflectionProbability(
                message.AbilityEntity, message.ActivatorEntity, message.TargetEntity, exposure);
            if (!pretend
                && DetermineSuccess(message.TargetEntity, deflectionProbability))
            {
                message.Outcome = ApplicationOutcome.Deflection;
                return 0;
            }

            var evasionProbability = GetEvasionProbability(
                message.AbilityEntity, message.ActivatorEntity, message.TargetEntity, exposure);
            if (!pretend
                && DetermineSuccess(message.TargetEntity, evasionProbability))
            {
                message.Outcome = ApplicationOutcome.Miss;
                return 0;
            }

            message.Outcome = ApplicationOutcome.Success;
            return pretend ? (100 - deflectionProbability) * (100 - evasionProbability) / 100 : 100;
        }

        private static AbilitySuccessCondition GetSuccessCondition(AbilityComponent ability)
        {
            var successCondition = ability.SuccessCondition;
            if (successCondition == AbilitySuccessCondition.Default)
            {
                successCondition = (ability.Activation & ActivationType.OnAttack) != 0
                    ? AbilitySuccessCondition.Attack
                    : (GetTrigger(ability) & ActivationType.OnAttack) != 0
                        ? AbilitySuccessCondition.Attack
                        : AbilitySuccessCondition.Always;
            }

            return successCondition;
        }

        private int GetAttackRating(
            GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure)
        {
            if (exposure == null)
            {
                var position = targetEntity.Position;
                var ability = abilityEntity.Ability;
                var los = GetLOSTargets(
                    activatorEntity, position.LevelCell, position.LevelEntity, ability.TargetingType, ability.Range);

                if (los.Count == 0
                    || los.Last().Item1 != targetEntity)
                {
                    exposure = 0;
                }
                else
                {
                    exposure = los.Last().Item2;
                }
            }

            var attackRating = activatorEntity.Being.Accuracy;
            if (exposure.Value < BeveledVisibilityCalculator.MaxVisibility)
            {
                attackRating = attackRating * exposure.Value / BeveledVisibilityCalculator.MaxVisibility;
            }

            // TODO: Add perception penalty using best sense
            return attackRating;
        }

        public int GetDeflectionProbability(GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure)
        {
            switch (GetSuccessCondition(abilityEntity.Ability))
            {
                case AbilitySuccessCondition.Attack:
                case AbilitySuccessCondition.UnavoidableAttack:
                    var attackRating = GetAttackRating(abilityEntity, activatorEntity, targetEntity, exposure);
                    return GetMissProbability(attackRating, targetEntity.Being.Deflection);
                case AbilitySuccessCondition.UnblockableAttack:
                case AbilitySuccessCondition.Always:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int GetEvasionProbability(GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure)
        {
            switch (GetSuccessCondition(abilityEntity.Ability))
            {
                case AbilitySuccessCondition.Attack:
                case AbilitySuccessCondition.UnblockableAttack:
                    var attackRating = GetAttackRating(abilityEntity, activatorEntity, targetEntity, exposure);
                    return GetMissProbability(attackRating, targetEntity.Being.Evasion);
                case AbilitySuccessCondition.UnavoidableAttack:
                case AbilitySuccessCondition.Always:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool DetermineSuccess(GameEntity victimEntity, int successProbability)
        {
            var entropyState = victimEntity.Being.EntropyState;
            var success = victimEntity.Game.Random.NextBool(successProbability, ref entropyState);
            victimEntity.Being.EntropyState = entropyState;
            return success;
        }

        private int GetMissProbability(int attackRating, int defenseRating)
        {
            if (attackRating <= 0)
            {
                return 100;
            }

            if (defenseRating <= 0)
            {
                return 0;
            }

            return (int)Math.Round(100 / (1 + Math.Pow(2, (attackRating - defenseRating) / 16.0)));
        }

        private void DeactivateAbilities(
            int activatableId, ActivationType activation, GameEntity activatorEntity, GameManager manager)
        {
            foreach (var abilityEntity in manager.AbilitiesToAffectableRelationship[activatableId])
            {
                var ability = abilityEntity.Ability;
                if ((ability.Activation & activation) == 0 || !ability.IsActive)
                {
                    continue;
                }

                Deactivate(ability, activatorEntity, manager);
            }
        }

        MessageProcessingResult IMessageConsumer<DeactivateAbilityMessage, GameManager>.Process(
            DeactivateAbilityMessage message, GameManager manager)
        {
            Deactivate(message.AbilityEntity.Ability, message.ActivatorEntity, manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        private void Deactivate(AbilityComponent ability, GameEntity activatorEntity, GameManager manager)
        {
            ChangeState(active: false, ability, activatorEntity, manager);

            if (ability.EnergyCost > 0
                && (ability.Activation & ActivationType.Continuous) != 0)
            {
                var being = ability.OwnerEntity.Being ?? activatorEntity.Being;
                if (being != null)
                {
                    being.ReservedEnergyPoints -= ability.EnergyCost;
                }
            }

            foreach (var appliedEffect in manager.AppliedEffectsToSourceAbilityRelationship[ability.EntityId].ToList())
            {
                RemoveComponentMessage.Enqueue(appliedEffect, EntityComponent.Effect, manager);
            }

            if (ability.Slot != null
                && (ability.Activation & ActivationType.WhileToggled) != 0)
            {
                ability.Slot = null;
            }
        }

        private void ChangeState(bool active, AbilityComponent ability, GameEntity activatorEntity, GameManager manager)
        {
            if (active)
            {
                if ((ability.Activation & ActivationType.Continuous) == 0)
                {
                    if (ability.Cooldown > 0)
                    {
                        var delay = ability.GetActualDelay(activatorEntity);
                        Debug.Assert(delay != -1);

                        ability.CooldownTick = manager.Game.CurrentTick + ability.Cooldown + delay;
                    }

                    if (ability.XPCooldown > 0)
                    {
                        ability.CooldownXpLeft = activatorEntity.Player.NextLevelXP * ability.XPCooldown / 100;
                    }
                }
                else if (!ability.IsActive)
                {
                    ability.IsActive = true;
                }
            }
            else
            {
                Debug.Assert((ability.Activation & ActivationType.Continuous) != 0);
                Debug.Assert(ability.IsActive);

                ability.IsActive = false;
                if (ability.Cooldown > 0)
                {
                    ability.CooldownTick = manager.Game.CurrentTick + ability.Cooldown;
                }

                if (ability.XPCooldown > 0)
                {
                    ability.CooldownXpLeft = activatorEntity.Player.NextLevelXP * ability.XPCooldown / 100;
                }
            }
        }

        private class AbilityActivation
        {
            public AbilityActivatedMessage ActivationMessage { get; set; }
            public AbilityActivatedMessage TargetMessage { get; set; }
        }
    }
}
