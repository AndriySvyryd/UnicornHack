using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

public class ItemMovingSystem : IGameSystem<MoveItemMessage>, IGameSystem<TraveledMessage>, IGameSystem<DiedMessage>
{
    public const int DefaultInventorySize = 10;

    public bool CanMoveItem(MoveItemMessage message, GameManager manager)
    {
        var moved = TryMove(message, manager, pretend: true);
        var successful = moved.Successful;
        message.ItemEntity.Manager.ReturnMessage(moved);
        return successful;
    }

    public MessageProcessingResult Process(MoveItemMessage message, GameManager manager)
    {
        var moved = TryMove(message, manager);
        // A room target accompanying a container target is its fallback when the container can't accept the item.
        if (!moved.Successful
            && message.TargetContainerEntity != null
            && message.TargetRoomEntity != null)
        {
            manager.ReturnMessage(moved);
            message.TargetContainerEntity = null;
            moved = TryMove(message, manager);
        }

        if (!moved.Successful
            && !message.SuppressLog
            && message.TargetContainerEntity is { Player: not null } playerEntity)
        {
            manager.LoggingSystem.WriteLog(
                manager.Game.Services.Language.InventoryFull(), playerEntity, manager);
        }

        manager.Enqueue(moved);
        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
    {
        var position = message.Entity.Position!;
        if (message.Entity.Physical!.Capacity == 0
            || position.Cell is not { } cell)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var roomItem = position.RoomEntity.Room!.Items.GetValueOrDefault(cell);
        if (roomItem != null)
        {
            // Pickup item on move over
            var moveMessage = MoveItemMessage.Create(manager);
            moveMessage.ItemEntity = roomItem;
            moveMessage.TargetContainerEntity = message.Entity;

            manager.Enqueue(moveMessage);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    // TODO: Drop items when Physical.Capacity decreases

    public MessageProcessingResult Process(DiedMessage message, GameManager manager)
    {
        var beingEntity = message.BeingEntity;
        var engulfer = manager.EngulferToVictimRelationship.GetEngulfer(beingEntity);
        var position = GetTopContainer(beingEntity, manager).Position;
        if (position?.Cell is not { } cell)
        {
            // The whole container chain is off the map: there is nowhere to drop onto.
            return MessageProcessingResult.ContinueProcessing;
        }

        foreach (var item in beingEntity.Being!.Items)
        {
            var moveItemMessage = MoveItemMessage.Create(manager);
            moveItemMessage.ItemEntity = item;
            moveItemMessage.TargetContainerEntity = engulfer;
            moveItemMessage.TargetCell = cell;
            moveItemMessage.TargetRoomEntity = position.RoomEntity;
            moveItemMessage.SuppressLog = true;
            moveItemMessage.Force = true;

            manager.Enqueue(moveItemMessage);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private static ItemMovedMessage TryMove(MoveItemMessage message, GameManager manager, bool pretend = false)
    {
        var itemEntity = message.ItemEntity;
        var item = itemEntity.Item!;
        var position = itemEntity.Position;
        var itemMovedMessage = ItemMovedMessage.Create(manager);
        itemMovedMessage.ItemEntity = itemEntity;
        itemMovedMessage.InitialRoomCell = position?.Cell;
        itemMovedMessage.InitialContainer = item.ContainerEntity;
        itemMovedMessage.InitialRoomEntity = position?.RoomEntity;
        itemMovedMessage.InitialCount = item.GetQuantity();
        itemMovedMessage.SuppressLog = message.SuppressLog;

        if (item.ContainerId != null
            && (item.ContainerId == message.TargetContainerEntity?.Id
                || itemEntity.Id == message.TargetContainerEntity?.Id))
        {
            throw new InvalidOperationException(
                $"Invalid target container '{message.TargetContainerEntity?.Id}' for item '{itemEntity.Id}'");
        }

        if (!message.Force)
        {
            var sourceActor = item.ContainerEntity is { } sourceContainer
                ? manager.ItemMovingSystem.GetTopContainer(sourceContainer, manager).Being?.Entity
                : null;
            var targetActor = message.TargetContainerEntity is { } targetContainer
                ? manager.ItemMovingSystem.GetTopContainer(targetContainer, manager).Being?.Entity
                : null;
            if ((sourceActor != null && !sourceActor.Being!.CanMoveItems)
                || (targetActor != null && !targetActor.Being!.CanMoveItems))
            {
                return itemMovedMessage;
            }
        }

        if (message.TargetContainerEntity == null)
        {
            Debug.Assert(message.TargetRoomEntity != null);
            if (item.ContainerId != null
                && item.EquippedSlot != EquipmentSlot.None)
            {
                var equipMessage = EquipItemMessage.Create(manager);
                equipMessage.ActorEntity = item.ContainerEntity!;
                equipMessage.ItemEntity = itemEntity;
                equipMessage.SuppressLog = true;
                equipMessage.Force = message.Force;

                if (!manager.ItemUsageSystem.CanEquipItem(equipMessage, manager))
                {
                    manager.Queue.ReturnMessage(equipMessage);
                    return itemMovedMessage;
                }

                manager.ItemUsageSystem.Process(equipMessage, manager);
                manager.Queue.ReturnMessage(equipMessage);
            }

            GameEntity? initialTopContainer = null;
            var initialPosition = itemMovedMessage.InitialRoomCell;
            if (initialPosition == null)
            {
                initialTopContainer = itemMovedMessage.InitialContainer != null
                    ? manager.ItemMovingSystem.GetTopContainer(itemMovedMessage.InitialContainer, manager)
                    : itemEntity;
                initialPosition = initialTopContainer.Position?.Cell;
            }

            Direction? spilloverDirection = null;
            if (initialPosition != null
                && initialPosition != message.TargetCell!.Value)
            {
                spilloverDirection = message.TargetCell.Value.DifferenceTo(initialPosition.Value).GetUnit()
                    .AsDirection();
            }

            // Settle the item onto the target cell, spilling over to the nearest cell along the spillover
            // direction when it is blocked or occupied and stacking with a matching item along the way.
            var placeOnEmpty = false;
            var freeCell = manager.TravelSystem.FindCell(
                message.TargetRoomEntity.Room!,
                (cell, room) =>
                {
                    if (!manager.TravelSystem.CanMoveTo(cell, room))
                    {
                        return false;
                    }

                    var existingItem = room.Items.GetValueOrDefault(cell);
                    if (existingItem == null)
                    {
                        placeOnEmpty = true;
                        return true;
                    }

                    var remaining = TryStackWith(itemMovedMessage, existingItem.Item!, pretend);
                    if (remaining == null)
                    {
                        return true;
                    }

                    itemMovedMessage.ItemEntity = remaining.Entity;
                    return false;
                },
                origin: message.TargetCell!.Value,
                direction: spilloverDirection);

            if (freeCell is not { } targetPoint)
            {
                // TODO: Create a sack
                return itemMovedMessage;
            }

            if (placeOnEmpty)
            {
                itemMovedMessage.Successful = true;
                if (pretend)
                {
                    return itemMovedMessage;
                }

                item.ContainerId = null;
                position ??= manager.CreateComponent<PositionComponent>(EntityComponent.Position);

                position.SetRoomPosition(message.TargetRoomEntity.Id, targetPoint);

                itemEntity.Position = position;
            }

            if (initialTopContainer != null
                && !pretend)
            {
                // TODO: Calculate delay
                DelayMessage.Enqueue(initialTopContainer, TimeSystem.DefaultActionDelay / 2, manager);
            }

            itemMovedMessage.Successful = true;
            return itemMovedMessage;
        }
        else
        {
            var targetContainer = message.TargetContainerEntity?.Physical;
            if ((targetContainer?.Capacity ?? 0) == 0)
            {
                return itemMovedMessage;
            }

            // TODO: If picking up from floor try to equip it directly

            var leftover = item;
            if (leftover.MaxStackSize > 1
                || leftover.Count != null)
            {
                foreach (var existingItem in targetContainer!.Items)
                {
                    leftover = TryStackWith(itemMovedMessage, existingItem.Item!, pretend);
                    if (leftover == null)
                    {
                        break;
                    }

                    itemMovedMessage.ItemEntity = leftover.Entity;
                }
            }

            if (leftover != null)
            {
                if (targetContainer!.Capacity <= targetContainer.Items.Count
                    || (targetContainer.Entity.Being != null
                        && manager.AbilitySlottingSystem.GetFirstFreeSlot(targetContainer.Entity) == null))
                {
                    return itemMovedMessage;
                }

                if (!pretend)
                {
                    MoveIntoContainer(leftover, targetContainer.Entity);
                }
            }

            itemMovedMessage.Successful = true;
            return itemMovedMessage;
        }
    }

    // Anchors an off-the-map item to a room without a cell of its own - a contained item or a natural weapon -
    // so it joins the RoomItems group and the room-load query finds it through its RoomId. Creates the Position
    // component (with the room already set, so the item never joins the group with an unset room) when it
    // doesn't have one yet.
    public static void SetItemRoom(GameEntity itemEntity, int roomId)
    {
        if (itemEntity.Position is { } position)
        {
            position.Cell = null;
            position.RoomId = roomId;
        }
        else
        {
            position = itemEntity.Manager.CreateComponent<PositionComponent>(EntityComponent.Position);
            position.RoomId = roomId;
            itemEntity.Position = position;
        }
    }

    // Puts an item into a container: it keeps its PositionComponent (so the room-load query still finds it
    // through its RoomId) but loses its cell, and its RoomId is realigned to the container's current room.
    private static void MoveIntoContainer(ItemComponent item, GameEntity container)
    {
        if (container.Position is { } containerPosition)
        {
            SetItemRoom(item.Entity, containerPosition.RoomId);
        }
        else if (item.Entity.Position is { } position)
        {
            position.Cell = null;
        }

        item.ContainerId = container.Id;
    }

    private static ItemComponent? TryStackWith(
        ItemMovedMessage itemMovedMessage, ItemComponent existingItem, bool pretend)
    {
        var item = itemMovedMessage.ItemEntity.Item!;
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

        var stack = item.Items.ToList();
        var stackSize = stack.Count + 1;

        var existingStackSize = existingItem.Items.Count + 1;

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
                    stackedItem.Item!.ContainerId = existingItem.EntityId;
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
            MoveIntoContainer(item, existingItem.Entity);
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

            var newItemEntityReference = manager.Game.Services.Data.Items.Get(item.TemplateName).Instantiate(manager);

            newItemEntityReference.Referenced.Item!.Count = quantity;
            item.Count -= quantity;

            return newItemEntityReference;
        }

        var stackedItems = item.Items.ToList();
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

        var newStack = stackedItems[^1];
        stackedItems.RemoveAt(stackedItems.Count - 1);
        stackSize--;

        var itemReference = item.Entity.AddReference(manager);
        using (newStack.AddReference(manager))
        {
            newStack.Item!.ContainerId = item.ContainerId;
            item.ContainerId = null;

            foreach (var stackedItem in stackedItems)
            {
                if (stackSize == quantity)
                {
                    break;
                }

                using (stackedItem.AddReference(manager))
                {
                    stackedItem.Item!.ContainerId = newStack.Id;
                }

                stackSize--;
            }
        }

        return itemReference;
    }

    public GameEntity GetTopContainer(GameEntity target, GameManager manager)
    {
        while (true)
        {
            if (target.Item?.ContainerEntity is { } container)
            {
                target = container;
                continue;
            }

            return target;
        }
    }
}
