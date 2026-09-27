using UnicornHack.Systems.Items;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class AbilitySlottingSystem :
    IGameSystem<SetAbilitySlotMessage>,
    IGameSystem<ItemMovedMessage>,
    IGameSystem<ItemEquippedMessage>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, bool>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
{
    public static readonly int DefaultMeleeAttackSlot = 0;
    public static readonly int DefaultRangedAttackSlot = 1;
    public static readonly int DefaultSlotCapacity = 8;

    public MessageProcessingResult Process(SetAbilitySlotMessage message, GameManager manager)
    {
        var actorEntity = message.ActorEntity;
        if (actorEntity.Being == null)
        {
            throw new InvalidOperationException("No actor found for the ability slot.");
        }

        var abilityEntity = message.AbilityEntity;
        var ability = message.AbilityEntity?.Ability;
        if (abilityEntity != null
            && !actorEntity.ActiveEffects!.AvailableAbilities.Contains(abilityEntity))
        {
            throw new InvalidOperationException(
                $"Ability {abilityEntity.Id} is not available to actor {actorEntity.Id}.");
        }

        if (message.Slot != null)
        {
            var conflictingAbility = actorEntity.Being.SlottedAbilities
                .GetValueOrDefault(message.Slot.Value);
            if (conflictingAbility != null)
            {
                if (conflictingAbility == message.AbilityEntity)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                ResetSlot(conflictingAbility, actorEntity, manager);

                if (conflictingAbility.Ability!.Slot != null)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }
            }

            if (ability == null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            Debug.Assert(ability.CooldownTick == null && ability.CooldownXpLeft == null);

            if (message.Slot.Value < 0
                || message.Slot.Value >= actorEntity.Physical!.Capacity)
            {
                throw new InvalidOperationException("Invalid slot " + message.Slot.Value);
            }

            if ((ability.Activation & ActivationType.Slottable) == 0)
            {
                throw new InvalidOperationException($"Ability {ability.EntityId} is not slottable.");
            }

            if (!ability.IsUsable)
            {
                throw new InvalidOperationException(
                    $"Ability {ability.EntityId} ({ability.Name}, level {ability.Level}) is not usable.");
            }

            if ((message.Slot.Value == DefaultMeleeAttackSlot
                && ability.Type != AbilityType.DefaultMeleeAttack)
                || (message.Slot.Value == DefaultRangedAttackSlot
                && ability.Type != AbilityType.DefaultRangedAttack))
            {
                throw new InvalidOperationException(
                    "Ability " + ability.EntityId + " cannot be the default attack for " + ability.OwnerId);
            }

            if (message.Slot.Value != DefaultMeleeAttackSlot
                && message.Slot.Value != DefaultRangedAttackSlot
                && ability.Type.IsDefaultAttack())
            {
                throw new InvalidOperationException(
                    "Ability " + ability.EntityId + " can only be the default attack for " + ability.OwnerId);
            }

            ability.Slot = message.Slot;
        }
        else if (ability != null)
        {
            if (ability.IsActive
                && ability.Slot != null)
            {
                var deactivateMessage = DeactivateAbilityMessage.Create(ability, actorEntity, manager);

                manager.Process(deactivateMessage);
            }

            if (ability.IsActive
                || ability.CooldownTick != null
                || ability.CooldownXpLeft != null)
            {
                return MessageProcessingResult.StopProcessing;
            }

            ability.Slot = null;

            if (ability.OwnerEntity is { Item: { EquippedSlot: EquipmentSlot.None } } itemEntity)
            {
                DropItem(itemEntity, actorEntity, manager, suppressLog: false);
            }
        }
        else if (message.Slot is null)
        {
            throw new InvalidOperationException(
                "An ability entity or a slot are required to change an ability slot.");
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        if (message.Group.Name == nameof(GameManager.EntityItemsToContainerRelationship))
        {
            if (message.PrincipalEntity?.Being != null)
            {
                AssignAvailableItemAbilities(message.Entity, message.PrincipalEntity, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(ItemMovedMessage message, GameManager manager)
    {
        if (!message.Successful
            || message.InitialContainer != null
            || message.InitialRoomCell == null
            || message.ItemEntity.Item?.ContainerEntity is not { Being: not null } ownerEntity)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        if (AssignAvailableItemAbilities(message.ItemEntity, ownerEntity, manager))
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        message.Successful = false;
        DropItem(message.ItemEntity, ownerEntity, manager, suppressLog: true);
        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(ItemEquippedMessage message, GameManager manager)
    {
        if (!message.Successful
            || manager.AvailableAbilitiesToRoomActorsRelationship
                .GetSlottableItemAbility(message.ItemEntity, message.ActorEntity) is not { } abilityEntity
            || abilityEntity.Ability!.Slot != null)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        if (!TryAssignItemAbility(abilityEntity, message.ActorEntity, manager))
        {
            if (message.Slot == EquipmentSlot.None)
            {
                DropItem(message.ItemEntity, message.ActorEntity, manager, suppressLog: true);
                return MessageProcessingResult.ContinueProcessing;
            }

            throw new InvalidOperationException(
                $"Unable to slot ability {abilityEntity.Id} after changing equipment state.");
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private bool AssignAvailableItemAbilities(
        GameEntity itemEntity, GameEntity ownerEntity, GameManager manager)
    {
        var abilityEntity = manager.AvailableAbilitiesToRoomActorsRelationship
            .GetSlottableItemAbility(itemEntity, ownerEntity);
        Debug.Assert(abilityEntity != null
                     || manager.Game.Services.Data.Items.Find(itemEntity.Item!.TemplateName) == null,
            $"Item {itemEntity.Id} must expose exactly one available slottable ability.");
        return abilityEntity != null
               && (abilityEntity.Ability!.Slot != null
                   || TryAssignItemAbility(abilityEntity, ownerEntity, manager));
    }

    private bool TryAssignItemAbility(
        GameEntity abilityEntity, GameEntity ownerEntity, GameManager manager)
    {
        var ability = abilityEntity.Ability;
        var slot = GetFirstFreeSlot(ownerEntity);
        if (ability == null
            || slot == null
            || !ability.IsUsable
            || ability.CooldownTick != null
            || ability.CooldownXpLeft != null)
        {
            return false;
        }

        var setSlotMessage = SetAbilitySlotMessage.Create(ownerEntity, manager);
        setSlotMessage.AbilityEntity = abilityEntity;
        setSlotMessage.Slot = slot;

        Process(setSlotMessage, manager);

        manager.Queue.ReturnMessage(setSlotMessage);
        return ability.Slot == slot;
    }

    private static void DropItem(
        GameEntity itemEntity, GameEntity ownerEntity, GameManager manager, bool suppressLog)
    {
        var position = ownerEntity.Position!;
        var dropMessage = MoveItemMessage.Create(manager);
        dropMessage.ItemEntity = itemEntity;
        dropMessage.TargetRoomEntity = position.RoomEntity;
        dropMessage.TargetCell = position.Cell;
        dropMessage.SuppressLog = suppressLog;
        dropMessage.Force = true;

        manager.Enqueue(dropMessage);
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, bool> message, GameManager manager)
    {
        var ability = message.Entity.Ability!;
        Debug.Assert(ability.CooldownXpLeft == null && ability.CooldownTick == null);
        if (!ability.IsUsable
            && ability.Slot != null
            && manager.SlottedAbilitiesToRoomActorsRelationship.GetPrincipal(message.Entity) is { } actorEntity)
        {
            ResetSlot(message.Entity, actorEntity, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
    {
        // Physical.Capacity
        if (message.NewValue < message.OldValue)
        {
            foreach (var (slot, abilityEntity) in message.Entity.Being!.SlottedAbilities.Reverse().ToList())
            {
                if (slot < message.NewValue)
                {
                    break;
                }

                if (slot != DefaultMeleeAttackSlot
                    && slot != DefaultRangedAttackSlot)
                {
                    ResetSlot(abilityEntity, message.Entity, manager);
                }
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private void ResetSlot(GameEntity abilityEntity, GameEntity actorEntity, GameManager manager)
    {
        var resetMessage = SetAbilitySlotMessage.Create(actorEntity, manager);
        resetMessage.AbilityEntity = abilityEntity;

        Process(resetMessage, manager);

        manager.Queue.ReturnMessage(resetMessage);
    }

    public GameEntity? GetAbility(GameEntity ownerEntity, int slot)
        => ownerEntity.Being!.SlottedAbilities.GetValueOrDefault(slot);

    private readonly int FirstPotentialSlot = Math.Max(DefaultMeleeAttackSlot, DefaultRangedAttackSlot) + 1;

    public int? GetFirstFreeSlot(GameEntity owner)
    {
        var i = FirstPotentialSlot;
        for (; i < owner.Physical!.Capacity; i++)
        {
            if (!owner.Being!.SlottedAbilities.ContainsKey(i))
            {
                break;
            }
        }

        return i < owner.Physical.Capacity ? i : null;
    }
}
