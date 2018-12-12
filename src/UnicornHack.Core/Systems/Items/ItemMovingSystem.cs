using System;
using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items
{
    public class ItemMovingSystem : IGameSystem<MoveItemMessage>, IGameSystem<TraveledMessage>, IGameSystem<DiedMessage>
    {
        public const string MoveItemMessageName = "MoveItem";
        public const string ItemMovedMessageName = "ItemMoved";
        public static readonly int DefaultInventorySize = 26;

        public MoveItemMessage CreateMoveItemMessage(GameManager manager)
            => manager.Queue.CreateMessage<MoveItemMessage>(MoveItemMessageName);

        public bool CanMoveItem(MoveItemMessage message, GameManager manager)
        {
            using (var moved = TryMove(message, manager, pretend: true))
            {
                return moved.Successful;
            }
        }

        public MessageProcessingResult Process(MoveItemMessage message, GameManager manager)
        {
            var moved = TryMove(message, manager);

            manager.Enqueue(moved);
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
        {
            var position = message.Entity.Position;
            if (message.Entity.Physical.Capacity == null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var levelItem = manager.LevelItemsToLevelCellIndex[(position.LevelId, position.LevelX, position.LevelY)];
            if (levelItem != null)
            {
                // Pickup item on move over
                var moveMessage = CreateMoveItemMessage(manager);
                moveMessage.ItemEntity = levelItem;
                moveMessage.TargetContainerEntity = message.Entity;

                manager.Enqueue(moveMessage);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            var position = message.BeingEntity.Position;
            foreach (var item in manager.EntityItemsToContainerRelationship[message.BeingEntity.Id])
            {
                var moveMessage = CreateMoveItemMessage(manager);
                moveMessage.ItemEntity = item;
                moveMessage.TargetCell = position.LevelCell;
                moveMessage.TargetLevelEntity = position.LevelEntity;
                moveMessage.SuppressLog = true;

                manager.Enqueue(moveMessage);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private static ItemMovedMessage TryMove(MoveItemMessage message, GameManager manager, bool pretend = false)
        {
            var itemEntity = message.ItemEntity;
            var item = itemEntity.Item;
            var position = itemEntity.Position;
            var itemMovedMessage = manager.Queue.CreateMessage<ItemMovedMessage>(ItemMovedMessageName);
            itemMovedMessage.ItemEntity = itemEntity;
            itemMovedMessage.InitialLevelCell = position?.LevelCell;
            itemMovedMessage.InitialContainer = item.ContainerId != null
                ? manager.FindEntity(item.ContainerId.Value)
                : null;
            itemMovedMessage.InitialCount = item.GetQuantity(manager);
            itemMovedMessage.SuppressLog = message.SuppressLog;

            if (item.ContainerId != null
                && (item.ContainerId == message.TargetContainerEntity?.Id
                    || itemEntity.Id == message.TargetContainerEntity?.Id))
            {
                throw new InvalidOperationException(
                    $"Invalid target container '{message.TargetContainerEntity?.Id}' for item '{itemEntity.Id}'");
            }

            if (message.TargetLevelEntity != null)
            {
                if (item.ContainerId != null
                    && item.EquippedSlot != EquipmentSlot.None)
                {
                    var equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
                    equipMessage.ActorEntity = manager.FindEntity(item.ContainerId.Value);
                    equipMessage.ItemEntity = itemEntity;
                    equipMessage.Slot = EquipmentSlot.None;
                    equipMessage.SuppressLog = true;

                    if (!manager.ItemUsageSystem.CanEquipItem(equipMessage, manager))
                    {
                        manager.Queue.ReturnMessage(equipMessage);
                        return itemMovedMessage;
                    }

                    manager.ItemUsageSystem.Process(equipMessage, manager);
                }

                itemMovedMessage.Delay += TimeSystem.DefaultActionDelay;

                var initialPosition = itemMovedMessage.InitialLevelCell;
                if (initialPosition == null)
                {
                    var initialTopContainer = itemMovedMessage.InitialContainer != null
                        ? manager.ItemMovingSystem.GetTopContainer(itemMovedMessage.InitialContainer, manager)
                        : itemEntity;
                    initialPosition = initialTopContainer.Position?.LevelCell;
                }

                var spilloverDirection = Direction.South;
                if (initialPosition != null
                    && initialPosition != message.TargetCell.Value)
                {
                    spilloverDirection = message.TargetCell.Value.DifferenceTo(initialPosition.Value).GetUnit().AsDirection();
                }

                ItemComponent leftover = null;
                foreach (var targetPoint in TerrainSystem.GetAdjacentPoints(
                    message.TargetLevelEntity.Level, message.TargetCell.Value, spilloverDirection, includeInitial: true))
                {
                    var (levelX, levelY) = targetPoint;
                    var existingItem = manager.LevelItemsToLevelCellIndex[(message.TargetLevelEntity.Id, levelX, levelY)];
                    if (existingItem == null)
                    {
                        itemMovedMessage.Successful = true;
                        if (pretend)
                        {
                            return itemMovedMessage;
                        }

                        item.ContainerId = null;
                        position = position ?? manager.CreateComponent<PositionComponent>(EntityComponent.Position);

                        position.SetLevelPosition(message.TargetLevelEntity.Id, targetPoint);

                        itemEntity.Position = position;

                        return itemMovedMessage;
                    }

                    leftover = TryStackWith(itemMovedMessage, existingItem.Item, pretend);
                    if (leftover == null)
                    {
                        break;
                    }

                    itemMovedMessage.ItemEntity = leftover.Entity;
                }

                if (leftover != null)
                {
                    // TODO: Create a sack
                    return itemMovedMessage;
                }
            }
            else
            {
                var targetContainer = message.TargetContainerEntity.Physical;
                if (targetContainer.Capacity == null)
                {
                    return itemMovedMessage;
                }

                var leftover = item;
                if (leftover.MaxStackSize > 1
                    || leftover.Count != null)
                {
                    foreach (var existingItem in manager.EntityItemsToContainerRelationship[targetContainer.EntityId])
                    {
                        leftover = TryStackWith(itemMovedMessage, existingItem.Item, pretend);
                        if (leftover == null)
                        {
                            break;
                        }

                        itemMovedMessage.ItemEntity = leftover.Entity;
                    }
                }

                if (leftover != null)
                {
                    if (targetContainer.Capacity
                        <= manager.EntityItemsToContainerRelationship[targetContainer.EntityId].Count())
                    {
                        return itemMovedMessage;
                    }

                    if (!pretend)
                    {
                        leftover.Entity.RemoveComponent(EntityComponent.Position);
                        leftover.ContainerId = targetContainer.EntityId;

                        // TODO: Update container weight
                    }
                }
            }

            itemMovedMessage.Successful = true;
            return itemMovedMessage;
        }

        private static ItemComponent TryStackWith(
            ItemMovedMessage itemMovedMessage, ItemComponent existingItem, bool pretend)
        {
            var item = itemMovedMessage.ItemEntity.Item;
            if (item.TemplateName != existingItem.TemplateName
                || (item.MaxStackSize <= 1
                    && item.Count == null))
            {
                return item;
            }

            if (item.Count != null)
            {
                if (!pretend)
                {
                    existingItem.Count += item.Count.Value;
                    item.Entity.RemoveComponent(EntityComponent.Position);
                    item.ContainerId = null;
                    itemMovedMessage.ItemEntity = existingItem.Entity;
                }

                return null;
            }

            var manager = item.Entity.Manager;
            var stack = manager.EntityItemsToContainerRelationship[item.EntityId].ToList();
            var stackSize = stack.Count + 1;

            var existingStackSize = manager.EntityItemsToContainerRelationship[existingItem.EntityId].Count() + 1;

            if (stackSize > 1 && existingStackSize < existingItem.MaxStackSize)
            {
                foreach (var stackedItem in stack)
                {
                    if (stackSize == 1
                        || existingStackSize == existingItem.MaxStackSize)
                    {
                        break;
                    }

                    if (!pretend)
                    {
                        stackedItem.Item.ContainerId = existingItem.EntityId;
                    }

                    existingStackSize++;
                    stackSize--;
                }
            }

            if (existingStackSize == existingItem.MaxStackSize)
            {
                return item;
            }

            if (!pretend)
            {
                item.Entity.RemoveComponent(EntityComponent.Position);
                item.ContainerId = existingItem.EntityId;
            }

            return null;
        }

        public ITransientReference<GameEntity> Split(ItemComponent item, int quantity)
        {
            var manager = item.Entity.Manager;
            if (item.Count != null)
            {
                if (item.Count.Value <= quantity)
                {
                    var reference = item.Entity.AddReference(manager);
                    item.ContainerId = null;
                    return reference;
                }

                var newItemEntityReference = Item.Loader.Get(item.TemplateName).Instantiate(manager);

                newItemEntityReference.Referenced.Item.Count = quantity;
                item.Count -= quantity;

                return newItemEntityReference;
            }

            var stackedItems = manager.EntityItemsToContainerRelationship[item.EntityId].ToList();
            var stackSize = stackedItems.Count + 1;
            if (quantity > stackSize || quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            if (quantity == stackSize)
            {
                var reference = item.Entity.AddReference(manager);
                item.ContainerId = null;
                return reference;
            }

            var newStack = stackedItems[stackedItems.Count - 1];
            stackedItems.RemoveAt(stackedItems.Count - 1);
            var stackReference = newStack.AddReference(manager);
            newStack.Item.ContainerId = null;
            var newStackSize = 1;

            if (quantity > 1)
            {
                foreach (var stackedItem in stackedItems)
                {
                    if (newStackSize == quantity)
                    {
                        break;
                    }

                    stackedItem.Item.ContainerId = newStack.Id;
                    newStackSize++;
                }
            }

            return stackReference;
        }

        public GameEntity GetTopContainer(GameEntity target, GameManager manager)
        {
            var containerId = target.Item?.ContainerId;
            while (containerId != null)
            {
                target = manager.FindEntity(containerId.Value);
                containerId = target.Item?.ContainerId;
            }

            return target;
        }
    }
}
