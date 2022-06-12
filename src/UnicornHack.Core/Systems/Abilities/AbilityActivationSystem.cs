using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;
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
        private static readonly ParameterExpression AbilityParameter =
            Expression.Parameter(typeof(GameEntity), name: "ability");

        private static readonly ParameterExpression ActivatorParameter =
            Expression.Parameter(typeof(GameEntity), name: "activator");

        private static readonly AccuracyExpressionVisitor _accuracyTranslator =
            new(new[] { AbilityParameter, ActivatorParameter });

        private static readonly DelayExpressionVisitor _delayTranslator =
            new(new[] { AbilityParameter, ActivatorParameter });

        private static readonly MethodInfo _accuracyRequirementsModifierMethod = typeof(AbilityActivationSystem)
            .GetMethod(nameof(GetAccuracyRequirementsModifierMethod), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _accuracyModifierMethod = typeof(AbilityActivationSystem)
            .GetMethod(nameof(GetAccuracyModifierMethod), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _delayRequirementModifierMethod = typeof(AbilityActivationSystem)
            .GetMethod(nameof(GetDelayRequirementModifierMethod), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _speedModifierMethod = typeof(AbilityActivationSystem)
            .GetMethod(nameof(GetSpeedModifierMethod), BindingFlags.NonPublic | BindingFlags.Static);

        public bool CanActivateAbility(ActivateAbilityMessage activateAbilityMessage, bool shouldThrow)
        {
            var message = Activate(activateAbilityMessage, pretend: true, stats: null);

            var hasError = !string.IsNullOrEmpty(message.ActivationError);
            if (hasError && shouldThrow)
            {
                throw new InvalidOperationException(message.ActivationError);
            }

            activateAbilityMessage.AbilityEntity.Manager.ReturnMessage(message);
            return !hasError;
        }

        public AttackStats GetAttackStats(ActivateAbilityMessage activateAbilityMessage)
        {
            var stats = new AttackStats();
            var activated = Activate(activateAbilityMessage, pretend: true, stats);
            if (!string.IsNullOrEmpty(activated?.ActivationError))
            {
                throw new InvalidOperationException(activated.ActivationError);
            }

            return stats;
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
            if (stats == null)
            {
                if (!ability.IsUsable)
                {
                    targetEffectsMessage.ActivationError =
                        $"Ability {ability.EntityId} is not usable or is not slotted.";
                    return targetEffectsMessage;
                }

                if (ability.Slot == null
                    && (ability.Activation & ActivationType.Slottable) != 0
                    && ability.OwnerEntity.Physical?.Capacity != 0)
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
            }

            if ((ability.Effects?.Count ?? 0) == 0)
            {
                targetEffectsMessage.ActivationError = $"The ability {ability.EntityId} has no effects.";
                return targetEffectsMessage;
            }

            var being = ability.OwnerEntity.Being ?? targetEffectsMessage.ActivatorEntity.Being;
            if (ability.EnergyCost > 0
                && being != null)
            {
                if (being.EnergyPoints < ability.EnergyCost
                    && stats == null)
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
                delay = GetDelay(ability, activateMessage.ActivatorEntity);
                if (delay < 0
                    && stats == null)
                {
                    targetEffectsMessage.ActivationError = $"Speed too low to activate ability {ability.EntityId}.";
                    return targetEffectsMessage;
                }

                if (stats != null)
                {
                    stats.Delay = delay;
                }

                var activatorPosition = activateMessage.ActivatorEntity.Position;
                if (activatorPosition != null
                    && stats == null)
                {
                    var targetCell = activateMessage.TargetCell ?? activateMessage.TargetEntity?.Position.LevelCell;
                    if (targetCell == null)
                    {
                        targetEffectsMessage.ActivationError = $"Speed too low to activate ability {ability.EntityId}.";
                        return targetEffectsMessage;
                    }

                    var requiredHeading = GetRequiredHeading(ability, activatorPosition, targetCell.Value);
                    if (requiredHeading != activatorPosition.Heading)
                    {
                        var travelMessage = TravelMessage.Create(manager);
                        travelMessage.ActorEntity = activateMessage.ActivatorEntity;
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
                        else
                        {
                            manager.ReturnMessage(travelMessage);
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

            targetEffectsMessage.Trigger = ability.Trigger;
            targetEffectsMessage.EffectsToApply = ImmutableList.Create<GameEntity>();

            AddTriggeredEffects(targetEffectsMessage, selfEffectsMessage, ability.Entity, ability.Trigger);

            var activatedAbilities = new HashSet<AbilityComponent>();
            var activations = GetActivations(targetEffectsMessage, selfEffectsMessage, activatedAbilities);

            if (!pretend)
            {
                foreach (var activatedAbility in activatedAbilities)
                {
                    ChangeState(active: true, activatedAbility, activateMessage.ActivatorEntity);
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
                stats.SelfEffects = selfEffectsMessage.EffectsToApply;
                manager.ReturnMessage(selfEffectsMessage);
            }

            foreach (var activation in activations)
            {
                if (!pretend
                    && activation.ActivationMessage.TargetEntity == null)
                {
                    manager.Enqueue(activation.ActivationMessage);
                }

                var targets = GetTargets(activation.ActivationMessage);
                foreach (var (target, exposure) in targets)
                {
                    var targetMessage = (activation.TargetMessage ?? activation.ActivationMessage).Clone(manager);
                    targetMessage.TargetEntity = target;

                    if (pretend)
                    {
                        if (target == activateMessage.TargetEntity)
                        {
                            var subStats = new SubAttackStats();
                            subStats.SuccessCondition = targetMessage.AbilityEntity.Ability.SuccessCondition;

                            var accuracy = manager.AbilityActivationSystem.GetAccuracy(
                                ability, targetMessage.ActivatorEntity);
                            subStats.Accuracy = accuracy;

                            var melee = activation.ActivationMessage.AbilityEntity.Ability.Range == 1;
                            subStats.HitProbability = DetermineOutcome(
                                targetMessage,
                                melee ? BeveledVisibilityCalculator.MaxVisibility : exposure,
                                pretend: true,
                                accuracy);

                            subStats.Effects = targetMessage.EffectsToApply;

                            stats.SubAttacks.Add(subStats);

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

                ((List<(GameEntity, byte)>)targets).Clear();
                manager.GameEntityByteListArrayPool.Return((List<(GameEntity, byte)>)targets);

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
                    var maxOctantDifference = ability.MaxHeadingDeviation;
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

            foreach (var effectEntity in targetEffectsMessage.AbilityEntity.Ability.Effects)
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
                        : GetSingleTriggeredAbility(abilityTrigger, activatable);

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
                DeactivateAbilities(
                    message.ItemEntity, ActivationType.WhilePossessed, message.ActorEntity, manager);

                var activation = ActivateAbilityMessage.Create(manager);
                activation.ActivatorEntity = message.ActorEntity;
                activation.TargetEntity = message.ActorEntity;

                if (ActivateAbilities(
                    message.ItemEntity, ActivationType.WhileEquipped, activation, manager, pretend: true))
                {
                    ActivateAbilities(
                        message.ItemEntity, ActivationType.WhileEquipped, activation, manager);
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
                DeactivateAbilities(message.ItemEntity, ActivationType.WhileEquipped, message.ActorEntity, manager);

                if (message.ItemEntity.Item.ContainerEntity == message.ActorEntity)
                {
                    var activationTemplate = ActivateAbilityMessage.Create(manager);
                    activationTemplate.ActivatorEntity = message.ActorEntity;
                    activationTemplate.TargetEntity = message.ActorEntity;

                    ActivateAbilities(message.ItemEntity, ActivationType.WhilePossessed, activationTemplate, manager);
                    manager.ReturnMessage(activationTemplate);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            var beingEntity = message.BeingEntity;
            if (!beingEntity.HasComponent(EntityComponent.Player))
            {
                DeactivateAbilities(beingEntity, ActivationType.Continuous, beingEntity, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(LeveledUpMessage message, GameManager manager)
        {
            // TODO: Use a leveled ability
            foreach (var abilityEntity in GetTriggeredAbilities(
                message.Entity, ActivationType.WhileAboveLevel))
            {
                var ability = abilityEntity.Ability;
                if (ability.IsActive)
                {
                    continue;
                }

                var newLevel = GetSourceRace(abilityEntity)?.Level ??
                               manager.XPSystem.GetXPLevel(message.Entity);
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

                abilityEntity = effect.SourceAbility;
            }
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
        {
            switch (message.Group.Name)
            {
                case nameof(GameManager.AbilitiesToAffectableRelationship):
                    var ability = message.Entity.Ability;
                    if ((ability.Activation & ActivationType.Always) != 0
                        && !ability.IsActive)
                    {
                        SelfActivate(ability, manager);
                    }

                    break;
                case nameof(GameManager.EntityItemsToContainerRelationship):
                    if (message.PrincipalEntity != null
                        && message.PrincipalEntity.HasComponent(EntityComponent.Being)
                        && message.Entity.Item.EquippedSlot == EquipmentSlot.None)
                    {
                        var activationTemplate = ActivateAbilityMessage.Create(manager);
                        activationTemplate.ActivatorEntity = message.PrincipalEntity;
                        activationTemplate.TargetEntity = message.PrincipalEntity;

                        ActivateAbilities(message.Entity, ActivationType.WhilePossessed, activationTemplate, manager);
                        manager.ReturnMessage(activationTemplate);
                    }

                    break;
                case nameof(GameManager.Effects):
                    var effect = message.Entity.Effect;
                    var containingAbilityEntity = effect.ContainingAbility;
                    if (containingAbilityEntity != null)
                    {
                        var containingAbility = containingAbilityEntity.Ability;
                        if (containingAbility != null
                            && (containingAbility.Activation & ActivationType.Always) != 0
                            && !containingAbility.IsActive
                            && containingAbility.Effects.Count == 1)
                        {
                            // TODO: If ability contains other effects deactivate then reactivate
                            SelfActivate(containingAbility, manager);
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
                    var ability = message.RemovedComponent as AbilityComponent ?? message.Entity.Ability;
                    if (ability != null
                        && (ability.Activation & ActivationType.Continuous) != 0
                        && ability.IsActive)
                    {
                        Deactivate(ability, message.PrincipalEntity, manager);
                    }

                    break;
                case nameof(GameManager.EntityItemsToContainerRelationship):
                    if (message.PrincipalEntity?.HasComponent(EntityComponent.Being) == true)
                    {
                        DeactivateAbilities(
                            message.Entity, ActivationType.WhilePossessed, message.PrincipalEntity, manager);
                    }

                    break;
                case nameof(GameManager.Effects):
                    var effect = (EffectComponent)message.RemovedComponent ?? message.Entity.Effect;
                    var containingAbility = effect.ContainingAbility?.Ability;
                    if (containingAbility != null
                        && (containingAbility.Activation & ActivationType.Continuous) != 0
                        && containingAbility.IsActive
                        && containingAbility.Effects.Count == 0)
                    {
                        Deactivate(containingAbility, message.PrincipalEntity, manager);
                    }

                    break;
                default:
                    throw new InvalidOperationException(message.Group.Name);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private void SelfActivate(AbilityComponent ability, GameManager manager)
            => manager.Process(ActivateAbilityMessage.Create(ability.Entity, ability.OwnerEntity, ability.OwnerEntity));

        public bool ActivateAbilities(
            GameEntity activatableEntity,
            ActivationType trigger,
            ActivateAbilityMessage activationMessage,
            GameManager manager,
            bool pretend = false)
        {
            foreach (var triggeredAbility in GetTriggeredAbilities(activatableEntity, trigger))
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

        public static IEnumerable<GameEntity> GetTriggeredAbilities(GameEntity activatableEntity, ActivationType trigger)
            => activatableEntity.Physical?.Abilities
                .Select(a => a.Ability)
                .Where(a => (a.Activation & trigger) != ActivationType.Default)
                .Select(a => a.Entity)
            ?? Enumerable.Empty<GameEntity>();

        private GameEntity GetSingleTriggeredAbility(ActivationType trigger, GameEntity activatableEntity)
        {
            var triggeredAbilities = GetTriggeredAbilities(activatableEntity, trigger);

            GameEntity triggeredAbility;
            using (var abilityEnumerator = triggeredAbilities.GetEnumerator())
            {
                if (!abilityEnumerator.MoveNext())
                {
                    throw new InvalidOperationException(
                        $"Item {activatableEntity.Id} has no abilities matching {trigger}");
                }

                triggeredAbility = abilityEnumerator.Current;

                if (abilityEnumerator.MoveNext())
                {
                    throw new InvalidOperationException(
                        $"Item {activatableEntity.Id} has multiple abilities matching {trigger}");
                }
            }

            return triggeredAbility;
        }

        private void AddTriggeredEffects(
            AbilityActivatedMessage abilityActivatedMessage,
            AbilityActivatedMessage selfEffectsMessage,
            GameEntity activatableEntity,
            ActivationType trigger)
        {
            foreach (var abilityEntity in GetTriggeredAbilities(activatableEntity, trigger))
            {
                // TODO: Trigger non-modifier abilities as separate activations
                Debug.Assert(abilityEntity.Ability.Action != AbilityAction.Modifier);

                foreach (var effectEntity in abilityEntity.Ability.Effects)
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

        private IReadOnlyCollection<(GameEntity, byte)> GetTargets(in AbilityActivatedMessage activationMessage)
        {
            var activator = activationMessage.ActivatorEntity;
            var manager = activator.Manager;
            var targets = manager.GameEntityByteListArrayPool.Rent();
            var ability = activationMessage.AbilityEntity.Ability;
            if (activationMessage.TargetEntity != null
                && ability.Activation != ActivationType.Targeted)
            {
                targets.Add((activationMessage.TargetEntity, BeveledVisibilityCalculator.MaxVisibility));
                return targets;
            }

            var level = activator.Position.LevelEntity.Level;
            var targetCell = activationMessage.TargetCell ?? activationMessage.TargetEntity.Position.LevelCell;
            var cells = GetTargetedCells(
                activator.Position.LevelCell,
                targetCell,
                activator.Position.Heading.Value,
                ability.Range,
                ability.TargetingShape,
                ability.TargetingShapeSize,
                level);

            foreach (var (visibleTargetCell, exposure) in cells)
            {
                var target = level.Actors.GetValueOrDefault(visibleTargetCell);
                if (target != null)
                {
                    targets.Add((target, exposure));

                    if (ability.TargetingShape == TargetingShape.Line)
                    {
                        return targets;
                    }
                }
            }

            ((List<(Point, byte)>)cells).Clear();
            manager.PointByteListArrayPool.Return((List<(Point, byte)>)cells);

            return targets;
        }

        public IReadOnlyCollection<(Point, byte)> GetTargetedCells(
            Point originCell,
            Point targetCell,
            Direction heading,
            int range,
            TargetingShape targetingShape,
            int targetShapeSize,
            LevelComponent level)
        {
            var manager = level.Entity.Manager;
            var targetVector = originCell.DifferenceTo(targetCell);
            var targetDistance = targetVector.Length();
            if (targetDistance == 0)
            {
                targetVector = heading.AsVector();
            }

            switch (targetingShape)
            {
                case TargetingShape.Line:
                case TargetingShape.Beam:
                    {
                        if (targetShapeSize != 1)
                        {
                            throw new NotSupportedException($"TargetingShape {targetingShape} with size {targetShapeSize} not supported");
                        }

                        return manager.SensorySystem.GetLOS(originCell, targetVector, range, level);
                    }
                case TargetingShape.Cone:
                    {
                        var targets = manager.PointByteListArrayPool.Rent();
                        if (targetShapeSize != 1)
                        {
                            throw new NotSupportedException($"TargetingShape {targetingShape} with size {targetShapeSize} not supported");
                        }

                        if (range != 1)
                        {
                            throw new NotSupportedException($"TargetingShape {targetingShape} with range {range} not supported");
                        }

                        var direction = targetVector.AsDirection();
                        var firstNeighbor = targetCell.Translate(direction.Rotate(1).AsVector());
                        var secondNeighbor = targetCell.Translate(direction.Rotate(-1).AsVector());

                        if (range == 1 && targetShapeSize == 1)
                        {
                            targets.Add((targetCell, BeveledVisibilityCalculator.MaxVisibility));
                            targets.Add((firstNeighbor, BeveledVisibilityCalculator.MaxVisibility));
                            targets.Add((secondNeighbor, BeveledVisibilityCalculator.MaxVisibility));
                        }

                        return targets;
                    }
                case TargetingShape.Square:
                    {
                        var targets = manager.PointByteListArrayPool.Rent();
                        if (targetShapeSize == 1)
                        {
                            if (targetDistance > range)
                            {
                                return targets;
                            }

                            if (targetDistance == 0)
                            {
                                targets.Add((originCell, BeveledVisibilityCalculator.MaxVisibility));
                                return targets;
                            }

                            var losCells = manager.SensorySystem.GetLOS(originCell, targetCell, level);
                            var last = losCells.LastOrDefault(c => c.Item1 == targetCell);
                            if (last.Item2 != 0)
                            {
                                targets.Add(last);
                            }

                            ((List<(Point, byte)>)losCells).Clear();
                            manager.PointByteListArrayPool.Return((List<(Point, byte)>)losCells);
                        }

                        return targets;
                    }
                default:
                    // TODO: Handle other shapes
                    throw new NotSupportedException("TargetingShape " + targetingShape + " not supported");
            }
        }

        private int DetermineOutcome(AbilityActivatedMessage message, byte exposure, bool pretend, int? accuracy = null)
        {
            if (message.TargetEntity == null)
            {
                message.Outcome = ApplicationOutcome.Miss;
                return 0;
            }

            if (message.AbilityEntity.Ability.SuccessCondition == AbilitySuccessCondition.Always
                || !message.ActivatorEntity.HasComponent(EntityComponent.Being)
                || !message.TargetEntity.HasComponent(EntityComponent.Being))
            {
                message.Outcome = ApplicationOutcome.Success;
                return 100;
            }

            accuracy ??= GetAccuracy(message.AbilityEntity.Ability, message.ActivatorEntity);
            var deflectionProbability = GetDeflectionProbability(
                message.AbilityEntity, message.ActivatorEntity, message.TargetEntity, exposure, accuracy);
            if (!pretend
                && DetermineSuccess(message.TargetEntity, deflectionProbability))
            {
                message.Outcome = ApplicationOutcome.Deflection;
                return 0;
            }

            var evasionProbability = GetEvasionProbability(
                message.AbilityEntity, message.ActivatorEntity, message.TargetEntity, exposure, accuracy);
            if (!pretend
                && DetermineSuccess(message.TargetEntity, evasionProbability))
            {
                message.Outcome = ApplicationOutcome.Miss;
                return 0;
            }

            message.Outcome = ApplicationOutcome.Success;
            return pretend ? (100 - deflectionProbability) * (100 - evasionProbability) / 100 : 100;
        }

        private int GetAttackRating(
            GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure, int? accuracy)
        {
            var ability = abilityEntity.Ability;
            var attackRating = accuracy ?? GetAccuracy(ability, activatorEntity);

            if (exposure == null)
            {
                var originPosition = activatorEntity.Position;
                var targetPosition = targetEntity.Position;
                var targetCell = targetPosition.LevelCell;
                var level = originPosition.LevelEntity.Level;
                var cells = GetTargetedCells(
                    originPosition.LevelCell,
                    targetCell,
                    originPosition.Heading.Value,
                    ability.Range,
                    TargetingShape.Square,
                    1,
                    level);

                exposure = cells.LastOrDefault().Item2;

                ((List<(Point, byte)>)cells).Clear();
                abilityEntity.Manager.PointByteListArrayPool.Return((List<(Point, byte)>)cells);
            }

            if (exposure.Value < BeveledVisibilityCalculator.MaxVisibility)
            {
                attackRating = attackRating * exposure.Value / BeveledVisibilityCalculator.MaxVisibility;
            }

            // TODO: Add perception penalty using best sense
            return attackRating;
        }

        public int GetDeflectionProbability(
            GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure, int? accuracy)
        {
            switch (abilityEntity.Ability.SuccessCondition)
            {
                case AbilitySuccessCondition.NormalAttack:
                case AbilitySuccessCondition.UnavoidableAttack:
                    var attackRating = GetAttackRating(abilityEntity, activatorEntity, targetEntity, exposure, accuracy);
                    return GetMissProbability(attackRating, targetEntity.Being.Deflection);
                case AbilitySuccessCondition.UnblockableAttack:
                case AbilitySuccessCondition.Always:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int GetEvasionProbability(
            GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure, int? accuracy)
        {
            switch (abilityEntity.Ability.SuccessCondition)
            {
                case AbilitySuccessCondition.NormalAttack:
                case AbilitySuccessCondition.UnblockableAttack:
                    var attackRating = GetAttackRating(abilityEntity, activatorEntity, targetEntity, exposure, accuracy);
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
            GameEntity activatableEntity, ActivationType activation, GameEntity activatorEntity, GameManager manager)
        {
            foreach (var abilityEntity in activatableEntity.Physical.Abilities)
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
            ChangeState(active: false, ability, activatorEntity);

            if (ability.EnergyCost > 0
                && (ability.Activation & ActivationType.Continuous) != 0)
            {
                var being = ability.OwnerEntity.Being ?? activatorEntity.Being;
                if (being != null)
                {
                    being.ReservedEnergyPoints -= ability.EnergyCost;
                }
            }

            foreach (var appliedEffect in manager.AppliedEffectsToSourceAbilityRelationship.GetDependents(ability.Entity).ToList())
            {
                RemoveComponentMessage.Enqueue(appliedEffect, EntityComponent.Effect, manager);
            }
        }

        private void ChangeState(bool active, AbilityComponent ability, GameEntity activatorEntity)
        {
            if (active)
            {
                if ((ability.Activation & ActivationType.Continuous) == 0)
                {
                    if (ability.Cooldown > 0)
                    {
                        var delay = GetDelay(ability, activatorEntity);
                        Debug.Assert(delay >= 0);

                        var game = activatorEntity.Manager.Game;
                        ability.CooldownTick = game.CurrentTick + ability.Cooldown + delay;
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
                    var game = activatorEntity.Manager.Game;
                    ability.CooldownTick = game.CurrentTick + ability.Cooldown;
                }

                if (ability.XPCooldown > 0)
                {
                    ability.CooldownXpLeft = activatorEntity.Player.NextLevelXP * ability.XPCooldown / 100;
                }
            }
        }

        public int GetAccuracy(AbilityComponent ability, GameEntity activatorEntity)
        {
            if (ability.Accuracy == null)
            {
                return activatorEntity.Being.Accuracy;
            }

            if (ability.AccuracyFunction == null)
            {
                ability.AccuracyFunction = CreateAccuracyFunction(ability.Accuracy, ability.Name);
            }

            try
            {
                return (int)ability.AccuracyFunction(ability.Entity, activatorEntity);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while evaluating the Accuracy for " + ability.Name, e);
            }
        }

        public static Func<GameEntity, GameEntity, float> CreateAccuracyFunction(string expression, string name)
        {
            try
            {
                return _accuracyTranslator.Translate<Func<GameEntity, GameEntity, float>, float>(expression);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while parsing the Accuracy for " + name, e);
            }
        }

        private static float GetAccuracyRequirementsModifierMethod(GameEntity abilityEntity, GameEntity activatorEntity)
        {
            var item = abilityEntity.Ability.OwnerEntity.Item;
            var template = Item.Loader.Get(item.TemplateName);
            var skillBonus =
                activatorEntity.Manager.SkillAbilitiesSystem.GetItemSkillBonus(template, activatorEntity.Player);
            var requiredPerception = template?.RequiredPerception ?? 0;
            var difference = activatorEntity.Being.Perception + skillBonus - requiredPerception;
            var accuracyModifier = requiredPerception * (difference + Math.Min(0, difference));

            return activatorEntity.Being.Accuracy + accuracyModifier;
        }

        private static float GetAccuracyModifierMethod(GameEntity abilityEntity, GameEntity activatorEntity)
        {
            return activatorEntity.Being.Accuracy;
        }

        public int GetDelay(AbilityComponent ability, GameEntity activatorEntity)
        {
            if (ability.Delay == null)
            {
                return 0;
            }

            if (ability.DelayFunction == null)
            {
                ability.DelayFunction = CreateDelayFunction(ability.Delay, ability.Name);
            }

            try
            {
                return (int)ability.DelayFunction(ability.Entity, activatorEntity);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while evaluating the Delay for " + ability.Name, e);
            }
        }

        public static Func<GameEntity, GameEntity, float> CreateDelayFunction(string expression, string name)
        {
            try
            {
                return _delayTranslator.Translate<Func<GameEntity, GameEntity, float>, float>(expression);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while parsing the Delay for " + name, e);
            }
        }

        private static float GetDelayRequirementModifierMethod(GameEntity abilityEntity, GameEntity activatorEntity)
        {
            var item = abilityEntity.Ability.OwnerEntity.Item;
            var template = Item.Loader.Get(item.TemplateName);
            var skillBonus = activatorEntity.Manager.SkillAbilitiesSystem.GetItemSkillBonus(template, activatorEntity.Player);
            var requiredSpeed = template?.RequiredSpeed ?? 0;
            var difference = activatorEntity.Being.Speed + skillBonus - requiredSpeed;
            var divisor = 100 + requiredSpeed * (difference + Math.Min(0, difference));

            if (divisor <= 0)
            {
                return -1;
            }

            return 100f / divisor;
        }

        private static float GetSpeedModifierMethod(GameEntity abilityEntity, GameEntity activatorEntity)
        {
            var divisor = 100 + 10 * activatorEntity.Being.Speed;

            return 100f / divisor;
        }

        private class AccuracyExpressionVisitor : UnicornExpressionVisitor
        {
            public AccuracyExpressionVisitor(IEnumerable<ParameterExpression> parameters)
                : base(parameters)
            {
            }

            protected override Expression CreateInvocation(string function, IEnumerable<Expression> children, Type childrenCommonType)
            {
                MethodInfo method;
                switch (function)
                {
                    case "RequirementsModifier":
                        method = _accuracyRequirementsModifierMethod;
                        break;
                    case "PerceptionModifier":
                        method = _accuracyModifierMethod;
                        break;
                    default:
                        return base.CreateInvocation(function, children, childrenCommonType);
                }

                if (children.Any())
                {
                    throw new InvalidOperationException($"Expected 0 parameters for function {function}");
                }

                return Expression.Call(null, method, AbilityParameter, ActivatorParameter);
            }
        }

        private class DelayExpressionVisitor : UnicornExpressionVisitor
        {
            public DelayExpressionVisitor(IEnumerable<ParameterExpression> parameters)
                : base(parameters)
            {
            }

            protected override Expression CreateInvocation(string function, IEnumerable<Expression> children, Type childrenCommonType)
            {
                MethodInfo method;
                switch (function)
                {
                    case "RequirementsModifier":
                        method = _delayRequirementModifierMethod;
                        break;
                    case "SpeedModifier":
                        method = _speedModifierMethod;
                        break;
                    default:
                        return base.CreateInvocation(function, children, childrenCommonType);
                }

                if (children.Any())
                {
                    throw new InvalidOperationException($"Expected 0 parameters for function {function}");
                }

                return Expression.Call(null, method, AbilityParameter, ActivatorParameter);
            }
        }

        private class AbilityActivation
        {
            public AbilityActivatedMessage ActivationMessage { get; set; }
            public AbilityActivatedMessage TargetMessage { get; set; }
        }
    }
}
