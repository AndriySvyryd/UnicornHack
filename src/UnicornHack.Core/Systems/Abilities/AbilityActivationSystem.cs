using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class AbilityActivationSystem :
    IGameSystem<ActivateAbilityMessage>,
    IGameSystem<DeactivateAbilityMessage>,
    IGameSystem<ItemEquippedMessage>,
    IGameSystem<DiedMessage>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<EntityRemovedMessage<GameEntity>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
{
    private static readonly ParameterExpression AbilityParameter =
        Expression.Parameter(typeof(GameEntity), name: "ability");

    private static readonly ParameterExpression ActivatorParameter =
        Expression.Parameter(typeof(GameEntity), name: "activator");

    private static readonly AccuracyExpressionVisitor _accuracyTranslator =
        new([AbilityParameter, ActivatorParameter]);

    private static readonly DelayExpressionVisitor _delayTranslator =
        new([AbilityParameter, ActivatorParameter]);

    private static readonly MethodInfo _accuracyRequirementsModifierMethod = typeof(AbilityActivationSystem)
        .GetMethod(nameof(GetAccuracyRequirementsModifierMethod), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _accuracyModifierMethod = typeof(AbilityActivationSystem)
        .GetMethod(nameof(GetAccuracyModifierMethod), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _delayRequirementModifierMethod = typeof(AbilityActivationSystem)
        .GetMethod(nameof(GetDelayRequirementModifierMethod), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _speedModifierMethod = typeof(AbilityActivationSystem)
        .GetMethod(nameof(GetSpeedModifierMethod), BindingFlags.NonPublic | BindingFlags.Static)!;

    public string? CanActivateAbility(ActivateAbilityMessage activateAbilityMessage)
    {
        var message = Activate(activateAbilityMessage, pretend: true, stats: null)!;
        var error = message.ActivationError;
        activateAbilityMessage.AbilityEntity.Manager.ReturnMessage(message);
        return error;
    }

    public static int GetEnergyCost(AbilityComponent ability, GameEntity? activatorEntity = null)
    {
        var actor = activatorEntity?.Being != null
            ? activatorEntity
            : ability.OwnerEntity;
        if (actor?.Being == null)
        {
            return ability.EnergyCost;
        }

        var mutedLevel = EffectApplicationSystem.GetAbilityLevel(actor, AbilityData.Muted.Name);
        return mutedLevel >= 1 && ability.Type.IsSpell()
               || mutedLevel >= 2 && IsVerbalAbility(ability)
             || mutedLevel >= 2 && ability.Type == AbilityType.Miracle
            ? ability.EnergyCost * 2
            : ability.EnergyCost;
    }

    private static bool IsVerbalAbility(AbilityComponent ability)
        => ability.Action is AbilityAction.Scream or AbilityAction.Screech or AbilityAction.Spell;

    public static string? GetBlockedCapabilityProperty(
        AbilityComponent ability, BeingComponent? activatorBeing)
    {
        if (ability.Type.IsSpell()
            && activatorBeing is { CanCastSpells: false })
        {
            return nameof(BeingComponent.CanCastSpells);
        }

        if (IsVerbalAbility(ability)
            && activatorBeing is { CanUseVerbalAbilities: false })
        {
            return nameof(BeingComponent.CanUseVerbalAbilities);
        }

        if (ability.Type == AbilityType.Miracle
            && activatorBeing is { CanUseDivineAbilities: false })
        {
            return nameof(BeingComponent.CanUseDivineAbilities);
        }

        if (ability.Type.IsDefaultAttack()
            && activatorBeing is { CanUseDefaultAttacks: false })
        {
            return nameof(BeingComponent.CanUseDefaultAttacks);
        }

        if ((ability.Activation & ActivationType.Always) == 0
            && !ability.Type.IsDefaultAttack()
            && activatorBeing is { CanUseNonDefaultAbilities: false })
        {
            return nameof(BeingComponent.CanUseNonDefaultAbilities);
        }

        if ((ability.Activation & ActivationType.Always) == 0
            && activatorBeing is { CanUseComplexAbilities: false }
            && ability.Action is AbilityAction.Breath or AbilityAction.Scream
                or AbilityAction.Screech or AbilityAction.Spell)
        {
            return nameof(BeingComponent.CanUseComplexAbilities);
        }

        if (!IsAttack(ability)
            && activatorBeing is { CanUseNonAttackAbilities: false })
        {
            return nameof(BeingComponent.CanUseNonAttackAbilities);
        }

        return null;
    }

    public static int GetEvasion(GameEntity targetEntity)
    {
        if (targetEntity.Being!.UnableToActPercent >= 100)
        {
            return 0;
        }

        return targetEntity.Being!.Evasion;
    }

    public AttackStats GetAttackStats(ActivateAbilityMessage activateAbilityMessage)
    {
        var stats = new AttackStats();
        PopulateAttackStats(activateAbilityMessage, stats);
        return stats;
    }

    public void PopulateAttackStats(ActivateAbilityMessage activateAbilityMessage, AttackStats stats)
    {
        stats.Reset();
        var activated = Activate(activateAbilityMessage, pretend: true, stats);
        if (!string.IsNullOrEmpty(activated?.ActivationError))
        {
            throw new InvalidOperationException(activated.ActivationError);
        }
    }

    MessageProcessingResult IMessageConsumer<ActivateAbilityMessage, GameManager>.Process(
        ActivateAbilityMessage message, GameManager manager)
    {
        var activationError = CanActivateAbility(message);
        if (activationError != null)
        {
            throw new InvalidOperationException(activationError);
        }

        var abilityActivatedMessage = Activate(message, pretend: false, stats: null);
        if (abilityActivatedMessage != null)
        {
            Debug.Assert(abilityActivatedMessage.ActivationError == null);
            manager.Enqueue(abilityActivatedMessage);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private AbilityActivatedMessage? Activate(
        ActivateAbilityMessage activateMessage,
        bool pretend,
        AttackStats? stats)
    {
        var manager = activateMessage.AbilityEntity.Manager;
        var targetEffectsMessage = AbilityActivatedMessage.Create(manager);
        targetEffectsMessage.AbilityEntity = activateMessage.AbilityEntity;
        targetEffectsMessage.ActivatorEntity = activateMessage.ActivatorEntity;
        targetEffectsMessage.TargetCell = activateMessage.TargetCell;
        targetEffectsMessage.TargetEntity = activateMessage.TargetEntity;

        var ability = activateMessage.AbilityEntity.Ability!;
        var language = manager.Game.Services.Language;
        AbilityActivatedMessage Fail(string error)
        {
            targetEffectsMessage.ActivationError = error;
            return targetEffectsMessage;
        }

        if (stats == null)
        {
            if (!ability.IsUsable)
            {
                return Fail(language.AbilityNotUsable(ability));
            }

            if ((ability.Activation & ActivationType.Slottable) != 0
                && !activateMessage.ActivatorEntity.ActiveEffects!.AvailableAbilities.Contains(
                    activateMessage.AbilityEntity))
            {
                return Fail(language.AbilityNotUsable(ability));
            }

            if (ability.Slot == null
                && (ability.Activation & ActivationType.Slottable) != 0
                && activateMessage.ActivatorEntity.Physical?.Capacity != 0)
            {
                return Fail(language.AbilityNotSlotted(ability));
            }

            if (ability.IsActive)
            {
                return Fail(language.AbilityAlreadyActive(ability));
            }

            if (ability.CooldownTick != null
                || ability.CooldownXpLeft != null)
            {
                return Fail(language.AbilityOnCooldown(ability));
            }

            var activator = activateMessage.ActivatorEntity;
            if (GetBlockedCapabilityProperty(ability, activator.Being) is { } blockedCapability)
            {
                var preventingStatus = EffectApplicationSystem.GetAbilitiesAffectingProperty(
                    activator, blockedCapability, AbilityType.Debuff).FirstOrDefault();
                return Fail(
                    preventingStatus != null
                        ? language.StatusPreventsAbility(preventingStatus, ability)
                        : blockedCapability switch
                        {
                            nameof(BeingComponent.CanCastSpells) => language.CannotCastAbility(ability),
                            nameof(BeingComponent.CanUseVerbalAbilities) => language.CannotUseVerbalAbility(ability),
                            nameof(BeingComponent.CanUseDivineAbilities) => language.CannotUseDivineAbility(ability),
                            nameof(BeingComponent.CanUseDefaultAttacks) => language.CannotUseDefaultAttack(ability),
                            nameof(BeingComponent.CanUseNonDefaultAbilities) => language.CannotUseNonDefaultAbility(ability),
                            nameof(BeingComponent.CanUseComplexAbilities) => language.CannotUseComplexAbility(ability),
                            nameof(BeingComponent.CanUseNonAttackAbilities) => language.CannotUseNonAttackAbility(ability),
                            _ => throw new InvalidOperationException(
                                $"Unsupported blocked ability capability '{blockedCapability}'.")
                        });
            }

            if (IsAttack(ability)
                && EffectApplicationSystem.GetAbilityLevel(activator, AbilityData.Engulfed.Name) >= 2
                && manager.EngulferToVictimRelationship.GetEngulfer(activator) is { } engulfer)
            {
                // Engulfed: any attack strikes the engulfer, whatever it was aimed at.
                targetEffectsMessage.TargetEntity = engulfer;
                targetEffectsMessage.TargetCell = null;
            }

            var tauntedLevel = EffectApplicationSystem.GetAbilityLevel(activator, AbilityData.Taunted.Name);
            if (IsAttack(ability)
                && tauntedLevel >= 2
                && FindStatusSource(activator, EffectType.Taunt, manager)
                    is { Being.IsAlive: true, Position.Cell: { } sourceCell } tauntSource)
            {
                targetEffectsMessage.TargetEntity = tauntSource;
                targetEffectsMessage.TargetCell = sourceCell;
            }
        }

        if (ability.Action == AbilityAction.Digestion
            && (targetEffectsMessage.TargetEntity is not { } digestionTarget
                || !ReferenceEquals(
                    manager.EngulferToVictimRelationship.GetVictim(activateMessage.ActivatorEntity),
                    digestionTarget)))
        {
            return Fail(language.InvalidDigestionTarget(ability));
        }

        if (ability.Effects.Count == 0)
        {
            return Fail(language.AbilityHasNoEffects(ability));
        }

        var being = targetEffectsMessage.ActivatorEntity.Being;
        var energyCost = GetEnergyCost(ability, targetEffectsMessage.ActivatorEntity);
        if (energyCost > 0
            && being != null)
        {
            if (being.EnergyPoints < energyCost
                && stats == null)
            {
                return Fail(language.NotEnoughEnergy(ability));
            }
        }

        var delay = 0;
        if ((ability.Activation & ActivationType.Slottable) != 0)
        {
            delay = GetDelay(ability, activateMessage.ActivatorEntity);
            if (delay < 0
                && stats == null)
            {
                return Fail(language.TooSlowToUseAbility(ability));
            }

            if (stats != null)
            {
                stats.Delay = delay;
            }

            var activatorPosition = activateMessage.ActivatorEntity.Position;
            if (activatorPosition is { Cell: { } activatorCell }
                && stats == null)
            {
                var targetCell = activateMessage.TargetCell ?? activateMessage.TargetEntity?.Position?.Cell;
                if (targetCell == null)
                {
                    return Fail(language.AbilityRequiresTarget(ability));
                }

                var requiredHeading = GetRequiredHeading(ability, activatorPosition, targetCell.Value);
                if (requiredHeading != activatorPosition.Heading)
                {
                    var travelMessage = TravelMessage.Create(manager);
                    travelMessage.ActorEntity = activateMessage.ActivatorEntity;
                    travelMessage.TargetHeading = requiredHeading;
                    travelMessage.Distance = 0;

                    if (!manager.TravelSystem.CanTravel(travelMessage, manager))
                    {
                        manager.ReturnMessage(travelMessage);
                        return Fail(language.CannotTurnToUseAbility(ability));
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

        if (!pretend
            && energyCost > 0
            && being != null)
        {
            if ((ability.Activation & ActivationType.Continuous) != 0)
            {
                manager.EffectApplicationSystem.AddTimedPropertyModifierInstance(
                    being.Entity.ActiveEffects!, being.Entity, EffectType.ChangeProperty,
                    energyCost, nameof(BeingComponent.ReservedEnergyPoints),
                    ValueCombinationFunction.Sum, ability.EntityId,
                    ability.Activation, expirationTarget: null, manager);
            }
            else
            {
                being.EnergyPoints -= energyCost;
            }
        }

        var selfEffectsMessage = AbilityActivatedMessage.Create(manager);
        selfEffectsMessage.AbilityEntity = activateMessage.AbilityEntity;
        selfEffectsMessage.ActivatorEntity = activateMessage.ActivatorEntity;
        selfEffectsMessage.TargetEntity = activateMessage.ActivatorEntity;
        selfEffectsMessage.Outcome = ApplicationOutcome.Success;
        selfEffectsMessage.EffectsToApply = [];

        targetEffectsMessage.Trigger = ability.Trigger;
        targetEffectsMessage.EffectsToApply = [];

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
            stats!.SelfEffects = selfEffectsMessage.EffectsToApply;
            manager.ReturnMessage(selfEffectsMessage);
        }

        foreach (var activation in activations)
        {
            var targets = GetTargets(activation.ActivationMessage);
            var targetCount = targets.Count;
            if (!pretend
                && activation.ActivationMessage.TargetEntity == null
                && targetCount == 0)
            {
                manager.Enqueue(activation.ActivationMessage);
            }

            foreach (var (target, exposure) in targets)
            {
                var targetMessage = (activation.TargetMessage ?? activation.ActivationMessage).Clone(manager);
                targetMessage.TargetEntity = target;

                if (pretend)
                {
                    if (target == activateMessage.TargetEntity)
                    {
                        var subStats = stats!.RentSubAttack();
                        subStats.AbilityId = targetMessage.AbilityEntity.Ability!.EntityId;
                        subStats.SuccessCondition = targetMessage.AbilityEntity.Ability!.SuccessCondition;

                        var accuracy = manager.AbilityActivationSystem.GetAccuracy(
                            ability, targetMessage.ActivatorEntity);
                        subStats.Accuracy = accuracy;

                        var melee = activation.ActivationMessage.AbilityEntity.Ability!.Type.IsMeleeAttack();
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

            if (activation.ActivationMessage.TargetEntity != null
                || !pretend && targetCount != 0)
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

    public static bool IsAttack(AbilityComponent ability)
        => ability.Type.IsAttack()
           || ability.SuccessCondition is AbilitySuccessCondition.NormalAttack
            or AbilitySuccessCondition.UnblockableAttack
            or AbilitySuccessCondition.UnavoidableAttack;

    private Direction GetRequiredHeading(
        AbilityComponent ability,
        PositionComponent activatorPosition,
        Point targetCell)
    {
        switch (ability.Activation)
        {
            case ActivationType.Manual:
            case ActivationType.WhileToggled:
                return activatorPosition.Heading!.Value;
            case ActivationType.Targeted:
                var targetDirection = activatorPosition.Cell!.Value.DifferenceTo(targetCell);
                if (targetDirection.Length() == 0)
                {
                    return activatorPosition.Heading!.Value;
                }

                var octantsToTurn = 0;
                var targetOctantDifference = targetDirection.OctantsTo(activatorPosition.Heading!.Value);
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
                else if (newDirection >= 8)
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

        activatedAbilities.Add(activationMessage.AbilityEntity.Ability!);
        GetActivations(activationMessage, null, selfEffectsMessage, null, activatedAbilities, activations);

        return activations;
    }

    private void GetActivations(
        AbilityActivatedMessage activationMessage,
        AbilityActivatedMessage? targetMessage,
        AbilityActivatedMessage selfEffectsMessage,
        GameEntity? activatingEffect,
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
                activatedAbilities.Add(subActivation.ActivatedAbility.Ability!);

                var subAbilityMessage = messageToProcess.Clone(manager);
                subAbilityMessage.AbilityEntity = subActivation.ActivatedAbility;

                AbilityActivatedMessage nextActivationMessage;
                var nextTargetMessage = targetMessage;
                if (subActivation.ActivatedAbility.Ability!.Range != 0
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
            Debug.Assert(activationMessage.AbilityEntity.Ability!.Action != AbilityAction.Modifier);
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

    private List<(GameEntity ActivatingEffect, GameEntity ActivatedAbility)>? GetActivatedAbilities(
        AbilityActivatedMessage targetEffectsMessage,
        AbilityActivatedMessage selfEffectsMessage)
    {
        var manager = targetEffectsMessage.ActivatorEntity.Manager;
        var abilityTrigger = targetEffectsMessage.Trigger;
        List<(GameEntity Effect, GameEntity Ability)>? activatedAbilities = null;

        foreach (var effectEntity in targetEffectsMessage.AbilityEntity.Ability!.Effects)
        {
            var effect = effectEntity.Effect!;
            if (effect.EffectType == EffectType.Activate)
            {
                var activatableId = effect.TargetEntityId!.Value;
                var activatable = ResolveActivationTarget(activatableId, targetEffectsMessage.ActivatorEntity, manager);
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

    // Resolves an Activate effect's TargetEntityId to the entity to activate: a non-negative id is looked up
    // directly; a negative id is a weapon slot, resolved to the weapon the owner currently wields there.
    public static GameEntity? ResolveActivationTarget(int targetEntityId, GameEntity ownerEntity, GameManager manager)
        => targetEntityId >= 0
            ? manager.FindEntity(targetEntityId)
            : FindWeaponInSlot(ownerEntity, (EquipmentSlot)(-targetEntityId), manager);

    // The weapon the owner currently wields in the given equipment slot: an equipped item in that slot takes
    // precedence over the being's natural weapon (e.g. a fist) for that slot. Null if neither is present.
    public static GameEntity? FindWeaponInSlot(GameEntity ownerEntity, EquipmentSlot slot, GameManager manager)
    {
        if (ownerEntity.Being is not { } being)
        {
            return null;
        }

        // A default attack may target a combined slot (e.g. either grasping hand), so match any weapon whose
        // equipped slot intersects the requested one rather than requiring an exact match.
        var trigger = (slot & EquipmentSlot.GraspMelee) != 0
            ? ActivationType.OnMeleeAttack
            : ActivationType.OnRangedAttack;
        var occupiedSlots = EquipmentSlot.None;
        foreach (var itemEntity in being.Items)
        {
            var equippedSlot = itemEntity.Item!.EquippedSlot;
            occupiedSlots |= equippedSlot;
            if ((equippedSlot & slot) != 0
                && GetTriggeredAbilities(itemEntity, trigger).Any())
            {
                return itemEntity;
            }
        }

        if ((slot & EquipmentSlot.GraspPrimaryMelee) != 0
            && (occupiedSlots & EquipmentSlot.GraspPrimaryMelee) == 0
            && being.PrimaryNaturalWeaponId is { } primaryId)
        {
            return manager.FindEntity(primaryId);
        }

        if ((slot & EquipmentSlot.GraspSecondaryMelee) != 0
            && (occupiedSlots & EquipmentSlot.GraspSecondaryMelee) == 0
            && being.SecondaryNaturalWeaponId is { } secondaryId)
        {
            return manager.FindEntity(secondaryId);
        }

        return null;
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

            if (message.ItemEntity.Item!.ContainerEntity == message.ActorEntity)
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

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        switch (message.Group.Name)
        {
            case nameof(GameManager.AbilitiesToAffectableRelationship):
                // The ability can already be gone if it was torn down in the same tick it was added.
                if (message.Entity.Ability is { } ability
                    && (ability.Activation & ActivationType.Always) != 0
                    && !ability.IsActive
                    && ability.Effects.Count > 0)
                {
                    SelfActivate(ability, manager);
                }

                break;
            case nameof(GameManager.EntityItemsToContainerRelationship):
                if (message.PrincipalEntity != null
                    && message.PrincipalEntity.HasComponent(EntityComponent.Being)
                    && message.Entity.Item!.EquippedSlot == EquipmentSlot.None)
                {
                    var activationTemplate = ActivateAbilityMessage.Create(manager);
                    activationTemplate.ActivatorEntity = message.PrincipalEntity;
                    activationTemplate.TargetEntity = message.PrincipalEntity;

                    ActivateAbilities(message.Entity, ActivationType.WhilePossessed, activationTemplate, manager);
                    manager.ReturnMessage(activationTemplate);
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
                if (message.RemovedComponent is AbilityComponent removedAbility)
                {
                    if ((removedAbility.Activation & ActivationType.Continuous) != 0
                        && removedAbility.IsActive
                        && message.PrincipalEntity is { } removedOwner)
                    {
                        var effectTarget = removedOwner.Being != null
                            ? removedOwner
                            : removedOwner.Item?.ContainerEntity;
                        if (effectTarget != null)
                        {
                            DeactivateInnerAbilities(removedAbility.Effects, effectTarget, manager);
                            manager.EffectApplicationSystem.RemoveMatchingPropertyEffects(
                                effectTarget.ActiveEffects!, effectTarget,
                                instance => instance.SourceAbilityEntityId == message.Entity.Id,
                                manager);
                            manager.EffectApplicationSystem.RemoveAbilityComponentEffects(
                                effectTarget, message.Entity.Id, manager);
                        }
                    }

                    break;
                }

                var ability = message.Entity.Ability;
                var wasActive = ability?.IsActive == true;
                if (ability != null
                    && (ability.Activation & ActivationType.Continuous) != 0
                    && wasActive)
                {
                    var deactivateMessage = DeactivateAbilityMessage.Create(
                        ability, message.PrincipalEntity!, manager);
                    manager.Enqueue(deactivateMessage);
                }

                // Source-linked instances the ability contributed (including on other entities) are cleaned
                // up by AppliedEffectsToSourceAbilityRelationship when the ability entity leaves the Abilities
                // group: source-bound effects such as Engulf and continuously sustained modifiers are removed;
                // fire-and-forget effects persist unlinked.

                break;
            case nameof(GameManager.EntityItemsToContainerRelationship):
                if (message.PrincipalEntity?.HasComponent(EntityComponent.Being) == true)
                {
                    DeactivateAbilities(
                        message.Entity, ActivationType.Continuous, message.PrincipalEntity, manager);
                }

                break;
            default:
                throw new InvalidOperationException(message.Group.Name);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private void SelfActivate(AbilityComponent ability, GameManager manager)
        => manager.Process(ActivateAbilityMessage.Create(ability.Entity, ability.OwnerEntity!, ability.OwnerEntity!));

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
            else
            {
                var activationError = CanActivateAbility(newActivation);
                manager.ReturnMessage(newActivation);
                if (activationError != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static IEnumerable<GameEntity> GetTriggeredAbilities(GameEntity activatableEntity,
        ActivationType trigger)
        => activatableEntity.ActiveEffects?.Abilities
               .Select(a => a.Ability!)
               .Where(a => (a.Activation & trigger) != ActivationType.Default)
               .Select(a => a.Entity)
           ?? Enumerable.Empty<GameEntity>();

    private GameEntity GetSingleTriggeredAbility(ActivationType trigger, GameEntity activatableEntity)
    {
        var triggeredAbilities = GetTriggeredAbilities(activatableEntity, trigger);

        using var abilityEnumerator = triggeredAbilities.GetEnumerator();
        if (!abilityEnumerator.MoveNext())
        {
            throw new InvalidOperationException(
                $"Item {activatableEntity.Id} has no abilities matching {trigger}");
        }

        var triggeredAbility = abilityEnumerator.Current;

        if (abilityEnumerator.MoveNext())
        {
            throw new InvalidOperationException(
                $"Item {activatableEntity.Id} has multiple abilities matching {trigger}");
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
            Debug.Assert(abilityEntity.Ability!.Action != AbilityAction.Modifier);

            foreach (var effectEntity in abilityEntity.Ability.Effects)
            {
                if (effectEntity.Effect!.ShouldTargetActivator)
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
        var ability = activationMessage.AbilityEntity.Ability!;
        var activatorPosition = activator.Position;
        if (activationMessage.TargetEntity != null
            && (ability.Activation != ActivationType.Targeted
                || activatorPosition?.Cell == null))
        {
            targets.Add((activationMessage.TargetEntity, BeveledVisibilityCalculator.MaxVisibility));
            return targets;
        }

        var room = activatorPosition!.RoomEntity.Room!;
        var targetCell = activationMessage.TargetCell ?? activationMessage.TargetEntity!.Position!.Cell!.Value;
        var cells = GetTargetedCells(
            activatorPosition.Cell!.Value,
            targetCell,
            activatorPosition.Heading!.Value,
            ability.Range,
            ability.TargetingShape,
            ability.TargetingShapeSize,
            room);

        foreach (var (visibleTargetCell, exposure) in cells)
        {
            var target = room.Actors.GetValueOrDefault(visibleTargetCell);
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
        RoomComponent room)
    {
        var manager = room.Entity.Manager;
        var targetVector = originCell.DifferenceTo(targetCell);
        var targetDistance = targetVector.Length();
        if (targetDistance == 0)
        {
            targetVector = heading.AsVector();
        }

        switch (targetingShape)
        {
            case TargetingShape.Beam:
            {
                // TODO: Implement length equal to targetShapeSize
                if (targetShapeSize != 1)
                {
                    throw new NotSupportedException(
                        $"TargetingShape {targetingShape} with size {targetShapeSize} not supported");
                }

                return manager.SensorySystem.GetLOS(originCell, targetVector, range, room);
            }
            case TargetingShape.Line:
            {
                // TODO: Implement width equal to targetShapeSize
                if (targetShapeSize != 1)
                {
                    throw new NotSupportedException(
                        $"TargetingShape {targetingShape} with size {targetShapeSize} not supported");
                }

                var targets = (List<(Point, byte)>)manager.SensorySystem.GetLOS(
                    originCell, targetVector, range, room);
                var line = manager.PointByteListArrayPool.Rent();
                var previousCell = originCell;
                var previousGroupStart = -1;
                var previousGroupEnd = -1;
                var expectedDistance = 1;
                for (var groupStart = 0; groupStart < targets.Count;)
                {
                    var distance = originCell.DistanceTo(targets[groupStart].Item1);
                    if (distance != expectedDistance)
                    {
                        break;
                    }

                    var groupEnd = groupStart + 1;
                    while (groupEnd < targets.Count
                           && originCell.DistanceTo(targets[groupEnd].Item1) == distance)
                    {
                        groupEnd++;
                    }

                    var selected = GetNextAdjacent(targets, groupStart, groupEnd);
                    if (previousCell.DistanceTo(selected.Item1) != 1)
                    {
                        var bridge = GetNextAdjacent(
                            targets, previousGroupStart, previousGroupEnd, previousCell, selected.Item1);
                        if (bridge.Item2 == 0)
                        {
                            selected = GetNextAdjacent(targets, groupStart, groupEnd, previousCell);
                            if (selected.Item2 == 0)
                            {
                                break;
                            }
                        }
                        else
                        {
                            line.Add(bridge);
                        }
                    }

                    line.Add(selected);
                    previousCell = selected.Item1;
                    previousGroupStart = groupStart;
                    previousGroupEnd = groupEnd;
                    groupStart = groupEnd;
                    expectedDistance++;
                }

                targets.Clear();
                manager.PointByteListArrayPool.Return(targets);
                return line;
            }
            case TargetingShape.Cone:
            {
                var targets = manager.PointByteListArrayPool.Rent();
                if (targetShapeSize != 1)
                {
                    throw new NotSupportedException(
                        $"TargetingShape {targetingShape} with size {targetShapeSize} not supported");
                }

                if (range != 1)
                {
                    throw new NotSupportedException(
                        $"TargetingShape {targetingShape} with range {range} not supported");
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

                    var losCells = manager.SensorySystem.GetLOS(originCell, targetCell, room);
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

        static (Point, byte) GetNextAdjacent(
            List<(Point, byte)> cells, int start, int end,
            Point? adjacentTo = null, Point? alsoAdjacentTo = null)
        {
            var selected = default((Point, byte));
            for (var i = start; i < end; i++)
            {
                var candidate = cells[i];
                if (candidate.Item2 <= selected.Item2
                    || adjacentTo != null && adjacentTo.Value.DistanceTo(candidate.Item1) != 1
                    || alsoAdjacentTo != null && alsoAdjacentTo.Value.DistanceTo(candidate.Item1) != 1)
                {
                    continue;
                }

                selected = candidate;
            }

            return selected;
        }
    }

    private int DetermineOutcome(AbilityActivatedMessage message, byte exposure, bool pretend, int? accuracy = null)
    {
        if (message.TargetEntity == null)
        {
            message.Outcome = ApplicationOutcome.Miss;
            return 0;
        }

        message.Outcome = ApplicationOutcome.Success;
        if (message.AbilityEntity.Ability!.SuccessCondition == AbilitySuccessCondition.Always
            || !message.ActivatorEntity.HasComponent(EntityComponent.Being)
            || !message.TargetEntity.HasComponent(EntityComponent.Being))
        {
            return 100;
        }

        accuracy ??= GetAccuracy(message.AbilityEntity.Ability, message.ActivatorEntity);
        var deflectionProbability = GetDeflectionProbability(
            message.AbilityEntity, message.ActivatorEntity, message.TargetEntity, exposure, accuracy);

        var evasionProbability = GetEvasionProbability(
            message.AbilityEntity, message.ActivatorEntity, message.TargetEntity, exposure, accuracy);

        var missOutcome = ApplicationOutcome.Miss;
        var defenseProbability = evasionProbability;
        if (evasionProbability < deflectionProbability)
        {
            defenseProbability = deflectionProbability;
            missOutcome = ApplicationOutcome.Deflection;
        }

        var useAttackerFortune = false;
        var fortuneEntity = message.TargetEntity;
        if (fortuneEntity.Player == null
            && message.ActivatorEntity.Player != null)
        {
            useAttackerFortune = true;
            fortuneEntity = message.ActivatorEntity;
        }

        var entropyState = fortuneEntity.Being!.EntropyState;
        var defenseSuccess = useAttackerFortune
            ? !fortuneEntity.Game.Random.NextStreaklessBool(100 - defenseProbability, ref entropyState)
            : fortuneEntity.Game.Random.NextStreaklessBool(defenseProbability, ref entropyState);

        if (!pretend)
        {
            fortuneEntity.Being.EntropyState = entropyState;
        }

        if (defenseSuccess)
        {
            message.Outcome = missOutcome;
        }

        return 100 - defenseProbability;
    }

    private int GetAttackRating(
        GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure,
        int? accuracy)
    {
        var ability = abilityEntity.Ability!;
        var attackRating = accuracy ?? GetAccuracy(ability, activatorEntity);

        if (exposure == null)
        {
            var originPosition = activatorEntity.Position!;
            var targetPosition = targetEntity.Position!;
            var targetCell = targetPosition.Cell!.Value;
            var room = originPosition.RoomEntity.Room!;
            var cells = GetTargetedCells(
                originPosition.Cell!.Value,
                targetCell,
                originPosition.Heading!.Value,
                ability.Range,
                TargetingShape.Square,
                1,
                room);

            exposure = cells.LastOrDefault().Item2;

            ((List<(Point, byte)>)cells).Clear();
            abilityEntity.Manager.PointByteListArrayPool.Return((List<(Point, byte)>)cells);
        }

        if (exposure.Value < BeveledVisibilityCalculator.MaxVisibility)
        {
            attackRating = attackRating * exposure.Value / BeveledVisibilityCalculator.MaxVisibility;
        }

        if (EffectApplicationSystem.GetAbilityLevel(activatorEntity, AbilityData.Frightened.Name) > 0
            && IsFearSource(activatorEntity, targetEntity))
        {
            // Fear makes it hard to land a solid blow on the very thing you dread.
            attackRating /= 2;
        }

        // TODO: Add perception penalty using best sense
        return attackRating;
    }

    /// <summary>
    ///     Returns whether <paramref name="targetEntity" /> is the source of any fear currently
    ///     gripping <paramref name="frightenedEntity" /> (i.e. the creature that frightened it).
    /// </summary>
    private static bool IsFearSource(GameEntity frightenedEntity, GameEntity targetEntity)
        => ReferenceEquals(
            FindStatusSource(frightenedEntity, EffectType.Frighten, frightenedEntity.Manager),
            targetEntity);

    public static GameEntity? FindStatusSource(
        GameEntity affectedEntity, EffectType effectType, GameManager manager)
    {
        if (affectedEntity.ActiveEffects == null)
        {
            return null;
        }

        return EffectApplicationSystem.FindPerSourceBuildUpSource(affectedEntity, effectType)
                is { SourceAbilityEntityId: { } abilityId }
            ? manager.FindEntity(abilityId)?.Ability?.OwnerEntity
            : null;
    }

    public int GetDeflectionProbability(
        GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure,
        int? accuracy)
    {
        switch (abilityEntity.Ability!.SuccessCondition)
        {
            case AbilitySuccessCondition.NormalAttack:
            case AbilitySuccessCondition.UnavoidableAttack:
                var attackRating =
                    GetAttackRating(abilityEntity, activatorEntity, targetEntity, exposure, accuracy);
                return GetMissProbability(attackRating, targetEntity.Being!.Deflection);
            case AbilitySuccessCondition.UnblockableAttack:
            case AbilitySuccessCondition.Always:
                return 0;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public int GetEvasionProbability(
        GameEntity abilityEntity, GameEntity activatorEntity, GameEntity targetEntity, byte? exposure,
        int? accuracy)
    {
        switch (abilityEntity.Ability!.SuccessCondition)
        {
            case AbilitySuccessCondition.NormalAttack:
            case AbilitySuccessCondition.UnblockableAttack:
                var attackRating =
                    GetAttackRating(abilityEntity, activatorEntity, targetEntity, exposure, accuracy);
                var evasion = GetEvasion(targetEntity);
                return GetMissProbability(attackRating, evasion);
            case AbilitySuccessCondition.UnavoidableAttack:
            case AbilitySuccessCondition.Always:
                return 0;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
        var abilities = activatableEntity.ActiveEffects!.Abilities;
        var index = abilities.Count;
        while (true)
        {
            index = Math.Min(index, abilities.Count);
            if (index == 0)
            {
                return;
            }

            if (abilities[--index].Ability is not { IsActive: true } ability
                || (ability.Activation & activation) == 0)
            {
                continue;
            }

            Deactivate(ability, activatorEntity, ability.Effects, ability.EntityId, manager);
        }
    }

    MessageProcessingResult IMessageConsumer<DeactivateAbilityMessage, GameManager>.Process(
        DeactivateAbilityMessage message, GameManager manager)
    {
        var capturedAbility = message.Ability
                              ?? throw new InvalidOperationException(
                                  "DeactivateAbilityMessage has no captured ability.");
        var capturedActivator = message.ActivatorEntity
                                ?? throw new InvalidOperationException(
                                    "DeactivateAbilityMessage has no captured activator.");
        if (!ReferenceEquals(message.AbilityEntity.Ability, capturedAbility)
            || capturedActivator.ActiveEffects == null)
        {
            DeactivateInnerAbilities(message.Effects, capturedActivator, manager);
            manager.EffectApplicationSystem.RemoveAbilityComponentEffects(
                capturedActivator, message.AbilityEntity.Id, manager);
            return MessageProcessingResult.ContinueProcessing;
        }

        Deactivate(
            capturedAbility, capturedActivator, message.Effects, message.AbilityEntity.Id, manager);

        return MessageProcessingResult.ContinueProcessing;
    }

    private void Deactivate(
        AbilityComponent ability, GameEntity activatorEntity, IEnumerable<GameEntity> effects,
        int sourceAbilityId, GameManager manager)
    {
        var owner = ability.OwnerEntity ?? activatorEntity;
        ChangeState(active: false, ability, activatorEntity);

        // The continuous modifiers this ability contributed (equip/toggle/possession stat bonuses) are
        // stored as ActivePropertyEffect instances, so drop every instance it sourced when it turns off.
        var modifierTarget = owner?.Being != null ? owner : owner?.Item?.ContainerEntity;
        if (modifierTarget != null)
        {
            DeactivateInnerAbilities(effects, modifierTarget, manager);
        }

        var propertyTarget = modifierTarget
            ?? (ability.EnergyCost > 0 && (ability.Activation & ActivationType.Continuous) != 0
                ? activatorEntity
                : null);
        if (propertyTarget != null)
        {
            // Drop every property modifier this ability sourced, including its calculated energy reservation.
            manager.EffectApplicationSystem.RemoveMatchingPropertyEffects(
                propertyTarget.ActiveEffects!, propertyTarget,
                instance => instance.SourceAbilityEntityId == sourceAbilityId,
                manager);
        }

        // Abilities this ability granted are recorded as ActiveComponentEffect directives on the target
        // (the activator, who is the recipient for a continuous self-grant), so tear them down here by their source ability
        manager.EffectApplicationSystem.RemoveAbilityComponentEffects(
            activatorEntity, sourceAbilityId, manager);
    }

    private static void DeactivateInnerAbilities(
        IEnumerable<GameEntity> effects, GameEntity activatorEntity, GameManager manager)
    {
        foreach (var effectEntity in effects)
        {
            if (effectEntity.Effect is { EffectType: EffectType.Activate } activateEffect
                && manager.FindEntity(activateEffect.TargetEntityId) is
                    { Ability: { IsActive: true } innerAbility } )
            {
                manager.Enqueue(DeactivateAbilityMessage.Create(innerAbility, activatorEntity, manager));
            }
        }
    }

    private void ChangeState(bool active, AbilityComponent ability, GameEntity activatorEntity)
    {
        var cooldownFactor = activatorEntity.Being?.CooldownFactor ?? 100;
        if (active)
        {
            if ((ability.Activation & ActivationType.Continuous) == 0)
            {
                if (ability.Cooldown > 0)
                {
                    var delay = GetDelay(ability, activatorEntity);
                    Debug.Assert(delay >= 0);

                    var game = activatorEntity.Manager.Game;
                    ability.CooldownTick = game.CurrentTick
                        + ScaleCooldown(ability.Cooldown, cooldownFactor, 100)
                        + delay;
                }

                if (ability.XPCooldown > 0)
                {
                    ability.CooldownXpLeft = ScaleCooldown(ability.XPCooldown, cooldownFactor, 100);
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
                ability.CooldownTick = game.CurrentTick
                                       + ScaleCooldown(ability.Cooldown, cooldownFactor, 100);
            }

            if (ability.XPCooldown > 0)
            {
                ability.CooldownXpLeft = ScaleCooldown(ability.XPCooldown, cooldownFactor, 100);
            }
        }
    }

    private static int ScaleCooldown(int cooldown, int numerator, int denominator)
        => (int)Math.Min(
            int.MaxValue,
            ((long)cooldown * numerator + denominator - 1) / denominator);

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
    {
        if (message.OldValue <= 0
            || message.NewValue <= 0)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var currentTick = manager.Game.CurrentTick;
        foreach (var abilityEntity in message.Entity.Being!.Abilities)
        {
            if (abilityEntity.Ability is { } ability)
            {
                RescaleCooldown(ability, currentTick, message.NewValue, message.OldValue);
            }
        }

        foreach (var itemEntity in message.Entity.Being.Items)
        {
            foreach (var abilityEntity in itemEntity.ActiveEffects!.Abilities)
            {
                RescaleCooldown(abilityEntity.Ability!, currentTick, message.NewValue, message.OldValue);
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private static void RescaleCooldown(
        AbilityComponent ability, int currentTick, int newFactor, int oldFactor)
    {
        if (ability.CooldownTick is { } cooldownTick
            && cooldownTick > currentTick)
        {
            ability.CooldownTick = currentTick
                                   + ScaleCooldown(cooldownTick - currentTick, newFactor, oldFactor);
        }

        if (ability.CooldownXpLeft is { } cooldownXpLeft
            && cooldownXpLeft > 0)
        {
            ability.CooldownXpLeft = ScaleCooldown(cooldownXpLeft, newFactor, oldFactor);
        }
    }

    public int GetAccuracy(AbilityComponent ability, GameEntity activatorEntity)
    {
        if (ability.Accuracy == null)
        {
            return activatorEntity.Being!.Accuracy;
        }

        ability.AccuracyFunction ??= CreateAccuracyFunction(ability.Accuracy, ability.Name!);

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
        var item = abilityEntity.Ability!.OwnerEntity!.Item!;
        var template = activatorEntity.Manager.Game.Services.Data.Items.Get(item.TemplateName);
        var skillBonus =
            activatorEntity.Manager.SkillAbilitiesSystem.GetItemSkillBonus(template, activatorEntity);
        var requiredPerception = template.RequiredPerception ?? 0;
        var difference = activatorEntity.Being!.Perception + skillBonus - requiredPerception;
        var accuracyModifier = requiredPerception * (difference + Math.Min(0, difference));

        return activatorEntity.Being.Accuracy + accuracyModifier;
    }

    private static float GetAccuracyModifierMethod(GameEntity abilityEntity, GameEntity activatorEntity)
        => activatorEntity.Being!.Accuracy;

    public int GetDelay(AbilityComponent ability, GameEntity activatorEntity)
    {
        if (ability.Delay == null)
        {
            return 0;
        }

        if (ability.DelayFunction == null)
        {
            ability.DelayFunction = CreateDelayFunction(ability.Delay, ability.Name!);
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
        var item = abilityEntity.Ability!.OwnerEntity!.Item!;
        var template = activatorEntity.Manager.Game.Services.Data.Items.Get(item.TemplateName);
        var skillBonus =
            activatorEntity.Manager.SkillAbilitiesSystem.GetItemSkillBonus(template, activatorEntity);
        var requiredSpeed = template.RequiredSpeed ?? 0;
        var difference = activatorEntity.Being!.Speed + skillBonus - requiredSpeed;
        var divisor = 100 + requiredSpeed * (difference + Math.Min(0, difference));

        if (divisor <= 0)
        {
            return -1;
        }

        return 100f / divisor;
    }

    private static float GetSpeedModifierMethod(GameEntity abilityEntity, GameEntity activatorEntity)
    {
        var divisor = 100 + 10 * activatorEntity.Being!.Speed;

        return 100f / divisor;
    }

    private class AccuracyExpressionVisitor : UnicornExpressionVisitor
    {
        public AccuracyExpressionVisitor(IEnumerable<ParameterExpression> parameters)
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

        protected override Expression CreateInvocation(string function, IEnumerable<Expression> children,
            Type childrenCommonType)
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
        public AbilityActivatedMessage ActivationMessage
        {
            get;
            init;
        } = null!;

        public AbilityActivatedMessage? TargetMessage
        {
            get;
            init;
        }
    }
}
