using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items
{
    public class ItemUsageSystem : IGameSystem<EquipItemMessage>
    {
        public const string EquipItemMessageName = "EquipItem";
        public const string ItemEquippedMessageName = "ItemEquipped";

        public EquipItemMessage CreateEquipItemMessage(GameManager manager)
            => manager.Queue.CreateMessage<EquipItemMessage>(EquipItemMessageName);

        public bool CanEquipItem(EquipItemMessage message, GameManager manager)
        {
            using (var equipped = TryEquip(message, manager, pretend: true))
            {
                return equipped.Successful;
            }
        }

        public MessageProcessingResult Process(EquipItemMessage message, GameManager manager)
        {
            var equipped = TryEquip(message, manager);

            manager.Enqueue(equipped);

            return MessageProcessingResult.ContinueProcessing;
        }

        private ItemEquippedMessage TryEquip(EquipItemMessage message, GameManager manager, bool pretend = false)
        {
            var item = message.ItemEntity.Item;
            var equippedMessage = manager.Queue.CreateMessage<ItemEquippedMessage>(ItemEquippedMessageName);
            equippedMessage.ItemEntity = message.ItemEntity;
            equippedMessage.ActorEntity = message.ActorEntity;
            equippedMessage.Slot = message.Slot;
            equippedMessage.SuppressLog = message.SuppressLog;

            if (message.Slot != EquipmentSlot.None)
            {
                var equipableSlots = GetEquipableSlots(item, message.ActorEntity.Physical.Size);
                if ((message.Slot & equipableSlots) == 0)
                {
                    return equippedMessage;
                }

                var equippedItem = GetEquippedItem(message.Slot, message.ActorEntity, manager);
                if (equippedItem != null)
                {
                    if (equippedItem == message.ItemEntity)
                    {
                        return equippedMessage;
                    }

                    if ((equippedItem.Item.EquipableSlots & message.Slot) == 0)
                    {
                        return equippedMessage;
                    }

                    if (!Unequip(equippedItem, message.ActorEntity, manager, pretend))
                    {
                        return equippedMessage;
                    }
                }

                if (message.Slot == EquipmentSlot.GraspPrimaryExtremity
                    || message.Slot == EquipmentSlot.GraspSecondaryExtremity)
                {
                    equippedItem = GetEquippedItem(EquipmentSlot.GraspBothExtremities, message.ActorEntity, manager);
                    if (equippedItem != null
                        && !Unequip(equippedItem, message.ActorEntity, manager, pretend))
                    {
                        return equippedMessage;
                    }
                }
                else if (message.Slot == EquipmentSlot.GraspBothExtremities)
                {
                    equippedItem = GetEquippedItem(EquipmentSlot.GraspPrimaryExtremity, message.ActorEntity, manager);
                    if (equippedItem != null
                        && !Unequip(equippedItem, message.ActorEntity, manager, pretend))
                    {
                        return equippedMessage;
                    }

                    equippedItem = GetEquippedItem(EquipmentSlot.GraspSecondaryExtremity, message.ActorEntity, manager);
                    if (equippedItem != null
                        && !Unequip(equippedItem, message.ActorEntity, manager, pretend))
                    {
                        return equippedMessage;
                    }
                }

                equippedMessage.Successful = true;
                if (pretend)
                {
                    return equippedMessage;
                }

                // TODO: Calculate delay
                equippedMessage.Delay += TimeSystem.DefaultActionDelay;

                item.EquippedSlot = message.Slot;
            }
            else
            {
                if (item.EquippedSlot == EquipmentSlot.None)
                {
                    return equippedMessage;
                }

                equippedMessage.Successful = true;
                if (pretend)
                {
                    return equippedMessage;
                }

                item.EquippedSlot = EquipmentSlot.None;

                // TODO: Calculate delay
                equippedMessage.Delay += TimeSystem.DefaultActionDelay;
            }

            return equippedMessage;
        }

        private bool Unequip(
            GameEntity equippedItem, GameEntity actorEntity, GameManager manager, bool pretend)
        {
            var unequipMessage = CreateEquipItemMessage(manager);
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

        public EquipmentSlot GetEquipableSlots(ItemComponent item, int size)
        {
            var category = GetSizeCategory(size);
            if ((item.EquipableSizes & category) != 0)
            {
                return item.EquipableSlots;
            }

            var slots = EquipmentSlot.None;
            if ((item.EquipableSlots & EquipmentSlot.GraspBothExtremities) != 0
                && (item.EquipableSlots & EquipmentSlot.GraspSingleExtremity) != 0)
            {
                if (category != SizeCategory.Tiny)
                {
                    var smallerSize = (SizeCategory)((int)category >> 1);
                    if ((item.EquipableSizes & smallerSize) != 0)
                    {
                        slots |= EquipmentSlot.GraspSingleExtremity;
                    }
                }

                if (category != SizeCategory.Gigantic)
                {
                    var biggerSize = (SizeCategory)((int)category << 1);
                    if ((item.EquipableSizes & biggerSize) != 0)
                    {
                        slots |= EquipmentSlot.GraspBothExtremities;
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
        public GameEntity GetEquippedItem(EquipmentSlot slot, GameEntity actor, GameManager manager) =>
            manager.EntityItemsToContainerRelationship[actor.Id].FirstOrDefault(item => item.Item.EquippedSlot == slot);
    }
}
