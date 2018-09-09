﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class AbilityActivationSystem :
        IGameSystem<ActivateAbilityMessage>,
        IGameSystem<ItemMovedMessage>,
        IGameSystem<ItemEquippedMessage>,
        IGameSystem<DiedMessage>,
        IGameSystem<EntityAddedMessage<GameEntity>>,
        IGameSystem<EntityRemovedMessage<GameEntity>>
    {
        public const string ActivateAbilityMessageName = "ActivateAbility";
        public const string AbilityActivatedMessageName = "AbilityActivated";

        public ActivateAbilityMessage CreateActivateAbilityMessage(GameManager manager)
            => manager.Queue.CreateMessage<ActivateAbilityMessage>(ActivateAbilityMessageName);

        public bool CanActivateAbility(ActivateAbilityMessage activateAbilityMessage)
        {
            using (var message = Activate(activateAbilityMessage, pretend: true))
            {
                return message.SuccessfulActivation;
            }
        }

        private void EnqueueAbilityActivated(AbilityActivatedMessage abilityActivatedMessage, GameManager manager)
        {
            if (abilityActivatedMessage.EffectsToApply != null
                && !abilityActivatedMessage.EffectsToApply.IsEmpty)
            {
                manager.Enqueue(abilityActivatedMessage);
            }
            else
            {
                manager.Queue.ReturnMessage(abilityActivatedMessage);
            }
        }

        public MessageProcessingResult Process(ActivateAbilityMessage message, GameManager manager)
        {
            var abilityActivatedMessage = Activate(message, pretend: false);
            if (abilityActivatedMessage != null)
            {
                EnqueueAbilityActivated(abilityActivatedMessage, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private AbilityActivatedMessage Activate(ActivateAbilityMessage activateMessage, bool pretend)
        {
            var manager = activateMessage.AbilityEntity.Manager;
            var targetEffectsMessage =
                manager.Queue.CreateMessage<AbilityActivatedMessage>(AbilityActivatedMessageName);
            targetEffectsMessage.AbilityEntity = activateMessage.AbilityEntity;
            targetEffectsMessage.ActivatorEntity = activateMessage.ActivatorEntity;
            targetEffectsMessage.TargetCell = activateMessage.TargetCell;
            targetEffectsMessage.TargetEntity = activateMessage.TargetEntity;

            var ability = activateMessage.AbilityEntity.Ability;
            if (!ability.IsUsable)
            {
                return targetEffectsMessage;
            }

            targetEffectsMessage.SuccessfulActivation = true;

            var activator = activateMessage.ActivatorEntity;
            if (ability.Activation == ActivationType.ManualActivation
                || ability.Activation == ActivationType.Targeted)
            {
                if (ability.EnergyPointCost > 0)
                {
                    var activatorCombatComponent = activator.Being;
                    if (activatorCombatComponent.EnergyPoints < ability.EnergyPointCost)
                    {
                        targetEffectsMessage.SuccessfulActivation = false;
                        return targetEffectsMessage;
                    }

                    if (!pretend)
                    {
                        manager.LivingSystem.ChangeCurrentEP(activatorCombatComponent, -1 * ability.EnergyPointCost);
                    }
                }

                // TODO: Specify correct delay in the abilities
                targetEffectsMessage.Delay = ability.Delay == 0 ? TimeSystem.DefaultActionDelay : ability.Delay;

                var activatorPosition = activateMessage.ActivatorEntity.Position;
                if (activatorPosition != null)
                {
                    var requiredHeading = GetRequiredHeading(ability, activatorPosition,
                        activateMessage.TargetCell ?? activateMessage.TargetEntity.Position.LevelCell);
                    if (requiredHeading != activatorPosition.Heading)
                    {
                        var travelMesage = manager.TravelSystem.CreateTravelMessage(manager);
                        travelMesage.Entity = activateMessage.ActivatorEntity;
                        travelMesage.TargetHeading = requiredHeading;
                        travelMesage.TargetCell = activatorPosition.LevelCell;

                        if (!manager.TravelSystem.CanTravel(travelMesage, manager))
                        {
                            targetEffectsMessage.SuccessfulActivation = false;
                            manager.Queue.ReturnMessage(travelMesage);
                            return targetEffectsMessage;
                        }

                        if (!pretend)
                        {
                            manager.Enqueue(travelMesage);
                        }
                    }
                }
            }

            if (pretend)
            {
                return targetEffectsMessage;
            }

            // TODO: Set the timeout

            if ((ability.Activation & ActivationType.Continuous) != 0)
            {
                Debug.Assert(!ability.IsActive);
                ability.IsActive = true;
            }

            var selfEffectsMessage = manager.Queue.CreateMessage<AbilityActivatedMessage>(AbilityActivatedMessageName);
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

            var activations = GetActivations(targetEffectsMessage, selfEffectsMessage);

            if (!selfEffectsMessage.EffectsToApply.IsEmpty)
            {
                manager.EffectApplicationSystem.Process(selfEffectsMessage, manager);
            }

            manager.Queue.ReturnMessage(selfEffectsMessage);

            foreach (var activation in activations)
            {
                if (activation.ActivationMessage.TargetEntity == null)
                {
                    EnqueueAbilityActivated(activation.ActivationMessage, manager);
                }

                foreach (var target in GetTargets(activation.ActivationMessage))
                {
                    var targetMessage = (activation.TargetMessage ?? activation.ActivationMessage).Clone(manager);
                    targetMessage.TargetEntity = target;

                    DetermineSuccess(targetMessage, manager);
                    // TODO: Trigger abilities on target
                    EnqueueAbilityActivated(targetMessage, manager);
                }

                if (activation.ActivationMessage.TargetEntity != null)
                {
                    manager.Queue.ReturnMessage(activation.ActivationMessage);
                }

                if (activation.TargetMessage != null)
                {
                    manager.Queue.ReturnMessage(activation.TargetMessage);
                }
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
                case ActivationType.ManualActivation:
                    return activatorPosition.Heading.Value;
                case ActivationType.Targeted:
                    var maxOctantDifference = 4;
                    switch (ability.TargetingAngle)
                    {
                        case TargetingAngle.Front2Octants:
                            maxOctantDifference = 0;
                            break;
                        case TargetingAngle.Front4Octants:
                            maxOctantDifference = 1;
                            break;
                        case TargetingAngle.Omnidirectional:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(
                                "Invalid ability direction " + ability.TargetingAngle);
                    }

                    var targetDirection = activatorPosition.LevelCell.DifferenceTo(targetCell);
                    if (targetDirection.Length() == 0)
                    {
                        return activatorPosition.Heading.Value;
                    }

                    var octantsToTurn = 0;
                    var targetAngleDifference =
                        (int)Math.Truncate(targetDirection.OctantsTo(activatorPosition.Heading.Value));
                    if (targetAngleDifference > maxOctantDifference)
                    {
                        octantsToTurn = targetAngleDifference - maxOctantDifference;
                    }
                    else if (targetAngleDifference < -maxOctantDifference)
                    {
                        octantsToTurn = targetAngleDifference + maxOctantDifference;
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
            AbilityActivatedMessage selfEffectsMessage)
        {
            var activations = new List<AbilityActivation>();

            GetActivations(activationMessage, null, selfEffectsMessage, null, activations);

            return activations;
        }

        private void GetActivations(
            AbilityActivatedMessage activationMessage,
            AbilityActivatedMessage targetMessage,
            AbilityActivatedMessage selfEffectsMessage,
            GameEntity activatingEffect,
            List<AbilityActivation> activations)
        {
            var manager = activationMessage.AbilityEntity.Manager;
            var queue = manager.Queue;
            var messageToProcess = targetMessage ?? activationMessage;
            var perAbilityActivations = GetActivatedAbilities(messageToProcess, selfEffectsMessage);
            if (perAbilityActivations != null)
            {
                foreach (var subActivation in perAbilityActivations)
                {
                    var subAbilityMessage = messageToProcess.Clone(manager);
                    subAbilityMessage.AbilityEntity = subActivation.ActivatedAbility;

                    AbilityActivatedMessage nextActivationMessage;
                    var nextTargetMessage = targetMessage;
                    if (subActivation.ActivatedAbility.Ability.TargetingType != TargetingType.None
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
                        subActivation.ActivatingEffect, activations);
                }

                queue.ReturnMessage(activationMessage);
                if (targetMessage != null)
                {
                    queue.ReturnMessage(targetMessage);
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

            foreach (var effectEntity in manager.EffectsToContainingAbilityRelationship[
                targetEffectsMessage.AbilityEntity.Id])
            {
                var effect = effectEntity.Effect;
                if (effect.EffectType == EffectType.Activate)
                {
                    Debug.Assert(abilityTrigger != ActivationType.Default);

                    var activatableId = effect.ActivatableEntityId.Value;
                    var activatable = manager.FindEntity(activatableId);
                    if (activatable == null)
                    {
                        throw new InvalidOperationException(
                            $"Couldn't find the entity to activate '{activatableId}' referenced from the effect '{effectEntity.Id}'"
                            + $" on ability '{effect.ContainingAbilityId}'");
                    }

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

        public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
        {
            var item = message.ItemEntity.Item;
            if (!message.Successful
                || message.InitialContainer?.Id == item.ContainerId)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            if (message.InitialContainer != null
                && message.InitialContainer.HasComponent(EntityComponent.Being))
            {
                var activatableId = message.ItemEntity.Id;
                DeactivateAbilities(activatableId, ActivationType.WhilePossessed, manager);
            }

            if (item.ContainerId != null)
            {
                var containerEntity = manager.FindEntity(item.ContainerId.Value);
                if (containerEntity.HasComponent(EntityComponent.Being))
                {
                    var activation = CreateActivateAbilityMessage(manager);
                    activation.ActivatorEntity = containerEntity;
                    activation.TargetEntity = containerEntity;

                    ActivateAbilities(message.ItemEntity.Id, ActivationType.WhilePossessed, activation, manager);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(ItemEquippedMessage message, GameManager manager)
        {
            if (!message.Successful)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            if (message.Slot != EquipmentSlot.None)
            {
                var activation = CreateActivateAbilityMessage(manager);
                activation.ActivatorEntity = message.ActorEntity;
                activation.TargetEntity = message.ActorEntity;

                ActivateAbilities(message.ItemEntity.Id, ActivationType.WhileEquipped, activation, manager);
            }
            else
            {
                var activatableId = message.ItemEntity.Id;
                DeactivateAbilities(activatableId, ActivationType.WhileEquipped, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
        {
            var ability = message.Entity.Ability;
            if ((ability.Activation & ActivationType.Always) != 0)
            {
                Debug.Assert(!ability.IsActive);

                var activation = CreateActivateAbilityMessage(manager);
                var owner = manager.FindEntity(ability.OwnerId);
                activation.ActivatorEntity = owner;
                activation.TargetEntity = owner;
                activation.AbilityEntity = message.Entity;

                Process(activation, manager);

                manager.Queue.ReturnMessage(activation);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        // TODO: Detect when IsUsable changes to false and deactivate

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager manager)
        {
            var ability = message.Entity.Ability;
            if (ability != null
                && (ability.Activation & ActivationType.Always) != 0)
            {
                Debug.Assert(ability.IsActive);

                Deactivate(ability, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            DeactivateAbilities(message.BeingEntity.Id, ActivationType.Continuous, manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        public void ActivateAbilities(
            int activatableId,
            ActivationType trigger,
            ActivateAbilityMessage activationMessage,
            GameManager manager)
        {
            foreach (var triggeredAbility in GetTriggeredAbilities(activatableId, trigger, manager))
            {
                var newActivation = activationMessage.Clone(activationMessage);
                newActivation.AbilityEntity = triggeredAbility;
                manager.Enqueue(newActivation);
            }

            manager.Queue.ReturnMessage(activationMessage);
        }

        private IEnumerable<GameEntity> GetTriggeredAbilities(
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
                        selfEffectsMessage.EffectsToApply = selfEffectsMessage.EffectsToApply.Add(effectEntity);
                    }
                    else
                    {
                        abilityActivatedMessage.EffectsToApply =
                            abilityActivatedMessage.EffectsToApply.Add(effectEntity);
                    }
                }
            }
        }

        private ActivationType GetTrigger(AbilityComponent ability)
        {
            switch (ability.Action)
            {
                case AbilityAction.Default:
                case AbilityAction.Modifier:
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
                    return ActivationType.OnPhysicalMeleeAttack;
                case AbilityAction.Shoot:
                case AbilityAction.Throw:
                case AbilityAction.Spit:
                    return ActivationType.OnPhysicalRangedAttack;
                case AbilityAction.Digestion:
                    return ActivationType.OnMagicalMeleeAttack;
                case AbilityAction.Breath:
                case AbilityAction.Gaze:
                case AbilityAction.Scream:
                case AbilityAction.Spell:
                case AbilityAction.Explosion:
                    return ActivationType.OnMagicalRangedAttack;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IReadOnlyCollection<GameEntity> GetTargets(in AbilityActivatedMessage activationMessage)
        {
            if (activationMessage.TargetEntity != null)
            {
                return new[] {activationMessage.TargetEntity};
            }

            var manager = activationMessage.ActivatorEntity.Manager;
            var levelId = activationMessage.ActivatorEntity.Position.LevelId;
            var targetCell = activationMessage.TargetCell.Value;
            var vectorToTarget = activationMessage.ActivatorEntity.Position.LevelCell.DifferenceTo(targetCell);
            var ability = activationMessage.AbilityEntity.Ability;
            switch (ability.TargetingType)
            {
                case TargetingType.AdjacentSingle:
                    if (vectorToTarget.Length() > 1)
                    {
                        return new GameEntity[0];
                    }

                    var target = manager.LevelActorToLevelCellIndex[(levelId, targetCell.X, targetCell.Y)];
                    return target == null ? new GameEntity[0] : new[] {target};
                case TargetingType.AdjacentArc:
                    if (vectorToTarget.Length() > 1)
                    {
                        return new GameEntity[0];
                    }

                    var direction = vectorToTarget.AsDirection();
                    var firstNeighbour = targetCell.Translate(Vector.Convert(direction.Rotate(1)));
                    var secondNeighbour = targetCell.Translate(Vector.Convert(direction.Rotate(-1)));

                    var targets = new List<GameEntity>();
                    var arcTarget = manager.LevelActorToLevelCellIndex[(levelId, targetCell.X, targetCell.Y)];
                    if (arcTarget != null)
                    {
                        targets.Add(arcTarget);
                    }

                    arcTarget = manager.LevelActorToLevelCellIndex[(levelId, firstNeighbour.X, firstNeighbour.Y)];
                    if (arcTarget != null)
                    {
                        targets.Add(arcTarget);
                    }

                    arcTarget = manager.LevelActorToLevelCellIndex[(levelId, secondNeighbour.X, secondNeighbour.Y)];
                    if (arcTarget != null)
                    {
                        targets.Add(arcTarget);
                    }

                    return targets;
                case TargetingType.Projectile:
                case TargetingType.GuidedProjectile:
                case TargetingType.Beam:
                case TargetingType.LineOfSight:
                    // TODO: set message.TargetCell to the final projectile position
                    // TODO: check LOS
                    var beamTarget = manager.LevelActorToLevelCellIndex[(levelId, targetCell.X, targetCell.Y)];
                    return beamTarget == null ? new GameEntity[0] : new[] {beamTarget};
                default:
                    throw new ArgumentOutOfRangeException("TargetingType " + ability.TargetingType + " not supported");
            }
        }

        private void DetermineSuccess(AbilityActivatedMessage message, GameManager manager)
        {
            var game = manager.Game;
            var ability = message.AbilityEntity.Ability;
            var successCondition = ability.SuccessCondition;
            if (successCondition == AbilitySuccessCondition.Default)
            {
                switch (ability.Activation)
                {
                    case ActivationType.OnPhysicalMeleeAttack:
                    case ActivationType.OnPhysicalRangedAttack:
                    case ActivationType.OnPhysicalAttack:
                        successCondition = AbilitySuccessCondition.PhysicalAttack;
                        break;
                    case ActivationType.OnMagicalMeleeAttack:
                    case ActivationType.OnMagicalRangedAttack:
                    case ActivationType.OnMagicalAttack:
                        successCondition = AbilitySuccessCondition.MagicAttack;
                        break;
                    default:
                        switch (GetTrigger(ability))
                        {
                            case ActivationType.OnPhysicalMeleeAttack:
                            case ActivationType.OnPhysicalRangedAttack:
                            case ActivationType.OnPhysicalAttack:
                                successCondition = AbilitySuccessCondition.PhysicalAttack;
                                break;
                            case ActivationType.OnMagicalMeleeAttack:
                            case ActivationType.OnMagicalRangedAttack:
                            case ActivationType.OnMagicalAttack:
                                successCondition = AbilitySuccessCondition.MagicAttack;
                                break;
                            default:
                                successCondition = AbilitySuccessCondition.Always;
                                break;
                        }
                        break;
                }
            }

            // TODO: Allow hit even if attacker doesn't see the victim, but it's in LOS
            var success = true;
            var targetPosition = message.TargetEntity.Position;
            switch (successCondition)
            {
                case AbilitySuccessCondition.Always:
                    break;
                case AbilitySuccessCondition.PhysicalAttack:
                    success =
                        message.TargetEntity != null
                        && manager.SensorySystem.GetVisibility(message.ActivatorEntity, targetPosition.LevelCell) > 0
                        && game.Random.Next(message.TargetEntity.Being.PhysicalDeflection + 20) < 15;
                    break;
                case AbilitySuccessCondition.MagicAttack:
                    success =
                        message.TargetEntity != null
                        && manager.SensorySystem.GetVisibility(message.ActivatorEntity, targetPosition.LevelCell) > 0
                        && game.Random.Next(message.TargetEntity.Being.MagicDeflection + 20) < 15;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            message.SuccessfulApplication = success;
        }

        private void DeactivateAbilities(int activatableId, ActivationType activation, GameManager manager)
        {
            foreach (var abilityEntity in manager.AbilitiesToAffectableRelationship[activatableId])
            {
                var ability = abilityEntity.Ability;
                if ((ability.Activation & activation) != 0 || !ability.IsActive)
                {
                    continue;
                }

                Deactivate(ability, manager);
            }
        }

        private void Deactivate(AbilityComponent ability, GameManager manager)
        {
            Debug.Assert((ability.Activation & ActivationType.Continuous) != 0);
            Debug.Assert(ability.IsActive);

            ability.IsActive = false;

            foreach (var appliedEffect in manager.AppliedEffectsToSourceAbilityRelationship[ability.EntityId].ToList())
            {
                var removeComponentMessage = manager.CreateRemoveComponentMessage();
                removeComponentMessage.Entity = appliedEffect;
                removeComponentMessage.Component = EntityComponent.Effect;

                manager.Enqueue(removeComponentMessage, lowPriority: true);
            }
        }

        private class AbilityActivation
        {
            public AbilityActivatedMessage ActivationMessage { get; set; }
            public AbilityActivatedMessage TargetMessage { get; set; }
        }
    }
}