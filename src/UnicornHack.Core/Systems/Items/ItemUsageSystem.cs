﻿using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

public class ItemUsageSystem : IGameSystem<EquipItemMessage>
{
    public bool CanEquipItem(EquipItemMessage message, GameManager manager)
    {
        var equipped = TryEquip(message, manager, pretend: true);
        var successful = equipped.Successful;

        message.ActorEntity.Manager.ReturnMessage(equipped);
        return successful;
    }

    public MessageProcessingResult Process(EquipItemMessage message, GameManager manager)
    {
        var equipped = TryEquip(message, manager);

        // TODO: log a message on failure
        manager.Enqueue(equipped);

        return MessageProcessingResult.ContinueProcessing;
    }

    public bool TryEquip(GameEntity itemEntity, EquipmentSlot slot, GameEntity targetEntity, bool force,
        bool suppressLog = false)
    {
        var manager = itemEntity.Manager;
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = targetEntity;
        equipMessage.ItemEntity = itemEntity;
        equipMessage.Slot = slot;
        equipMessage.SuppressLog = suppressLog;
        equipMessage.Force = force;

        if (!manager.ItemUsageSystem.CanEquipItem(equipMessage, manager))
        {
            manager.Queue.ReturnMessage(equipMessage);
            return true;
        }

        manager.ItemUsageSystem.Process(equipMessage, manager);
        manager.Queue.ReturnMessage(equipMessage);
        return false;
    }

    private ItemEquippedMessage TryEquip(EquipItemMessage message, GameManager manager, bool pretend = false)
    {
        var item = message.ItemEntity.Item!;
        var equippedMessage = ItemEquippedMessage.Create(manager);
        equippedMessage.ItemEntity = message.ItemEntity;
        equippedMessage.ActorEntity = message.ActorEntity;
        equippedMessage.Slot = message.Slot;
        equippedMessage.OldSlot = item.EquippedSlot;
        equippedMessage.SuppressLog = message.SuppressLog;

        switch (message.Slot)
        {
            case EquipmentSlot.None:
            case EquipmentSlot.GraspPrimaryMelee:
            case EquipmentSlot.GraspSecondaryMelee:
            case EquipmentSlot.GraspBothMelee:
            case EquipmentSlot.GraspPrimaryRanged:
            case EquipmentSlot.GraspSecondaryRanged:
            case EquipmentSlot.GraspBothRanged:
            case EquipmentSlot.GraspMouth:
            case EquipmentSlot.Torso:
            case EquipmentSlot.Head:
            case EquipmentSlot.Feet:
            case EquipmentSlot.Hands:
            case EquipmentSlot.Back:
            case EquipmentSlot.Neck:
                break;
            default:
                throw new InvalidOperationException($"Invalid slot {message.Slot}");
        }

        if (message.Slot != EquipmentSlot.None)
        {
            var equipableSlots = GetEquipableSlots(item, message.ActorEntity.Physical!.Size);
            if ((message.Slot & equipableSlots) == 0)
            {
                return equippedMessage;
            }

            var equippedItem = GetEquippedItem(message.Slot, message.ActorEntity);
            if (equippedItem != null)
            {
                if (equippedItem == message.ItemEntity)
                {
                    return equippedMessage;
                }

                if (!message.Force
                    || !Unequip(equippedItem, message.ActorEntity, manager, pretend))
                {
                    return equippedMessage;
                }
            }

            if (message.Slot == EquipmentSlot.GraspPrimaryMelee
                || message.Slot == EquipmentSlot.GraspSecondaryMelee)
            {
                equippedItem = GetEquippedItem(EquipmentSlot.GraspBothMelee, message.ActorEntity);
                if (equippedItem != null
                    && (!message.Force
                        || !Unequip(equippedItem, message.ActorEntity, manager, pretend)))
                {
                    return equippedMessage;
                }
            }
            else if (message.Slot == EquipmentSlot.GraspBothMelee)
            {
                equippedItem = GetEquippedItem(EquipmentSlot.GraspPrimaryMelee, message.ActorEntity);
                if (equippedItem != null
                    && (!message.Force
                        || !Unequip(equippedItem, message.ActorEntity, manager, pretend)))
                {
                    return equippedMessage;
                }

                equippedItem = GetEquippedItem(EquipmentSlot.GraspSecondaryMelee, message.ActorEntity);
                if (equippedItem != null
                    && (!message.Force
                        || !Unequip(equippedItem, message.ActorEntity, manager, pretend)))
                {
                    return equippedMessage;
                }
            }

            if (message.Slot == EquipmentSlot.GraspPrimaryRanged
                || message.Slot == EquipmentSlot.GraspSecondaryRanged)
            {
                equippedItem = GetEquippedItem(EquipmentSlot.GraspBothRanged, message.ActorEntity);
                if (equippedItem != null
                    && (!message.Force
                        || !Unequip(equippedItem, message.ActorEntity, manager, pretend)))
                {
                    return equippedMessage;
                }
            }
            else if (message.Slot == EquipmentSlot.GraspBothRanged)
            {
                equippedItem = GetEquippedItem(EquipmentSlot.GraspPrimaryRanged, message.ActorEntity);
                if (equippedItem != null
                    && (!message.Force
                        || !Unequip(equippedItem, message.ActorEntity, manager, pretend)))
                {
                    return equippedMessage;
                }

                equippedItem = GetEquippedItem(EquipmentSlot.GraspSecondaryRanged, message.ActorEntity);
                if (equippedItem != null
                    && (!message.Force
                        || !Unequip(equippedItem, message.ActorEntity, manager, pretend)))
                {
                    return equippedMessage;
                }
            }

            equippedMessage.Successful = true;
            if (pretend)
            {
                return equippedMessage;
            }

            item.EquippedSlot = message.Slot;

            DelayMessage.Enqueue(message.ActorEntity, TimeSystem.DefaultActionDelay / 20, manager);
        }
        else
        {
            if (item.EquippedSlot == EquipmentSlot.None
                || (!message.Force
                    && item.Abilities.Select(a => a.Ability!)
                        .Any(a => a.CooldownTick != null
                                  || a.CooldownXpLeft != null
                                  || (a.Activation & ActivationType.Slottable) != 0 && a.IsActive)))
            {
                return equippedMessage;
            }

            equippedMessage.Successful = true;
            if (pretend)
            {
                return equippedMessage;
            }

            item.EquippedSlot = EquipmentSlot.None;

            DelayMessage.Enqueue(message.ActorEntity, TimeSystem.DefaultActionDelay / 20, manager);
        }

        return equippedMessage;
    }

    private bool Unequip(
        GameEntity equippedItem, GameEntity actorEntity, GameManager manager, bool pretend)
    {
        var unequipMessage = EquipItemMessage.Create(manager);
        unequipMessage.ItemEntity = equippedItem;
        unequipMessage.ActorEntity = actorEntity;
        unequipMessage.Slot = EquipmentSlot.None;

        var equipped = TryEquip(unequipMessage, manager, pretend);

        var success = equipped.Successful;

        if (!pretend)
        {
            manager.Enqueue(equipped);
        }
        else
        {
            manager.Queue.ReturnMessage(unequipMessage);
        }

        return success;
    }

    // TODO: Sort by requirements
    public IEnumerable<EquipmentSlot> GetEquipableSlots(ItemComponent item, GameEntity beingEntity)
        => GetEquipableSlots(item, beingEntity.Physical!.Size).GetNonRedundantFlags(removeComposites: true);

    public EquipmentSlot GetEquipableSlots(ItemComponent item, int size)
    {
        var category = GetSizeCategory(size);
        if ((item.EquipableSizes & category) != 0)
        {
            return item.EquipableSlots;
        }

        var slots = EquipmentSlot.None;
        if ((item.EquipableSlots & EquipmentSlot.GraspBothMelee) != 0
            && (item.EquipableSlots & EquipmentSlot.GraspSingleMelee) != 0)
        {
            if (category != SizeCategory.Tiny)
            {
                var smallerSize = (SizeCategory)((int)category >> 1);
                if ((item.EquipableSizes & smallerSize) != 0)
                {
                    slots |= EquipmentSlot.GraspSingleMelee;
                }
            }

            if (category != SizeCategory.Gigantic)
            {
                var biggerSize = (SizeCategory)((int)category << 1);
                if ((item.EquipableSizes & biggerSize) != 0)
                {
                    slots |= EquipmentSlot.GraspBothMelee;
                }
            }
        }

        if ((item.EquipableSlots & EquipmentSlot.GraspBothRanged) != 0
            && (item.EquipableSlots & EquipmentSlot.GraspSingleRanged) != 0)
        {
            if (category != SizeCategory.Tiny)
            {
                var smallerSize = (SizeCategory)((int)category >> 1);
                if ((item.EquipableSizes & smallerSize) != 0)
                {
                    slots |= EquipmentSlot.GraspSingleRanged;
                }
            }

            if (category != SizeCategory.Gigantic)
            {
                var biggerSize = (SizeCategory)((int)category << 1);
                if ((item.EquipableSizes & biggerSize) != 0)
                {
                    slots |= EquipmentSlot.GraspBothRanged;
                }
            }
        }

        return slots;
    }

    private static SizeCategory GetSizeCategory(int size)
    {
        if (size <= 2)
        {
            return SizeCategory.Tiny;
        }

        if (size <= 4)
        {
            return SizeCategory.Small;
        }

        if (size <= 8)
        {
            return SizeCategory.Medium;
        }

        if (size <= 13)
        {
            return SizeCategory.Large;
        }

        return size <= 25
            ? SizeCategory.Huge
            : SizeCategory.Gigantic;
    }

    // TODO: Add an index for EquipmentSlot?
    public GameEntity? GetEquippedItem(EquipmentSlot slot, GameEntity actor) =>
        actor.Being!.Items.FirstOrDefault(item => item.Item!.EquippedSlot == slot);
}
