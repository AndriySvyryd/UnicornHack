using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack.Persistence;

public class RoomPersistenceTest
{
    [Fact]
    public async Task Visited_source_rooms_remain_loaded_after_multiple_room_transitions()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = persisted.Game.Manager;
        var firstRoom = player.Position!.RoomEntity.Room!;
        var secondRoom = TestHelper.BuildRoom("...", persisted.Game);
        var thirdRoom = TestHelper.BuildRoom("...", persisted.Game);
        secondRoom.Level = firstRoom.Level;
        thirdRoom.Level = firstRoom.Level;
        AddConnection(firstRoom, secondRoom, new Point(4, 0), new Point(0, 0), manager);
        AddConnection(secondRoom, thirdRoom, new Point(1, 0), new Point(0, 0), manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.TravelSystem.TravelToRoom(player, secondRoom.EntityId, manager));
        manager.Queue.ProcessQueue(manager);
        Assert.True(manager.TravelSystem.TravelToRoom(player, thirdRoom.EntityId, manager));
        manager.Queue.ProcessQueue(manager);
        persisted.Context.SaveChanges();

        var expectedRoomIds = new[]
        {
            firstRoom.EntityId,
            secondRoom.EntityId,
            thirdRoom.EntityId
        };
        Assert.All(
            firstRoom.Level.Rooms.Where(room => expectedRoomIds.Contains(room.EntityId)),
            room => Assert.True(room.IsVisited));

        await persisted.ReloadAsync();

        var reloadedLevel = persisted.PlayerEntity.Position!.RoomEntity.Room!.Level;
        Assert.Equal(
            expectedRoomIds.Order(),
            reloadedLevel.Rooms.Select(room => room.EntityId).Where(expectedRoomIds.Contains).Order());
        Assert.All(
            reloadedLevel.Rooms.Where(room => expectedRoomIds.Contains(room.EntityId)),
            room => Assert.True(room.IsVisited));
        var levelMap = GameQueryProcessor.SerializeLevelMap(persisted.PlayerEntity);
        Assert.Equal(
            expectedRoomIds.Order(),
            levelMap.Rooms.Select(room => room.Id).Where(expectedRoomIds.Contains).Order());
    }

    private static void AddConnection(
        RoomComponent source,
        RoomComponent target,
        Point sourceCell,
        Point targetCell,
        GameManager manager)
    {
        using var connectionReference = manager.CreateEntity();
        var connectionEntity = connectionReference.Referenced;
        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.RoomEntity = source.Entity;
        position.Cell = sourceCell;
        connectionEntity.Position = position;

        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomId = target.EntityId;
        connection.TargetCell = targetCell;
        connection.Directionality = Directionality.Source;
        connection.Orientation = Direction.East;
        connectionEntity.Connection = connection;
    }
}
