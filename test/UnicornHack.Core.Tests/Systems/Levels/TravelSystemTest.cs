using UnicornHack.Data.Creatures;

namespace UnicornHack.Systems.Levels;

public class TravelSystemTest
{
    [Fact]
    public void GetPossibleMovementDirections_returns_correct_directions()
    {
        var level = TestHelper.BuildRoom(@"
.#.
.>.
...");
        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 1));
        var manager = undine.Manager;
        PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new[] { Direction.North, Direction.Southeast },
            manager.TravelSystem.GetPossibleMovementDirections(undine.Position!, safe: true));
    }

    [Fact]
    public void Can_travel_to_other_levels()
    {
        var level1 = TestHelper.BuildRoom(@"
.#.
.>.
...");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level1, new Point(1, 0));
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        var level2 = TestHelper.BuildRoom(@"
...
..#
..<", game: manager.Game, depth: 1);

        var connection1Entity = level1.Connections.GetValueOrDefault(new Point(1, 1))!;
        var connection1 = connection1Entity.Connection!;

        var connection2Entity = level2.Connections.GetValueOrDefault(new Point(2, 2))!;

        connection1.TargetRoomId = level2.EntityId;
        connection1.TargetCell = connection2Entity.Position!.Cell;

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.South;
        travelMessage.TargetCell = connection1Entity.Position!.Cell;
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(level2.EntityId, player.Position!.RoomId);
        Assert.Equal(connection1.TargetCell, player.Position.Cell);
    }

    [Fact]
    public void CanMoveTo_forbids_moving_outside_room_boundary()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var travelSystem = room.Game.Manager.TravelSystem;

        // Inside the room is fine.
        Assert.True(travelSystem.CanMoveTo(new Point(0, 0), room));
        Assert.True(travelSystem.CanMoveTo(new Point(2, 2), room));

        // One past the boundary on each side is forbidden (treated as a wall).
        // X past the right edge.
        Assert.False(travelSystem.CanMoveTo(new Point(3, 0), room));
        // Y past the bottom edge.
        Assert.False(travelSystem.CanMoveTo(new Point(0, 3), room));
        // Negative would underflow to 255 -- still rejected.
        Assert.False(travelSystem.CanMoveTo(new Point(255, 0), room));
        Assert.False(travelSystem.CanMoveTo(new Point(0, 255), room));
        Assert.False(travelSystem.CanMoveTo(new Point(255, 255), room));
    }
}
