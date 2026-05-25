using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Senses;

public class SensorySystemTest
{
    private static readonly string TestMap = @"
...
...
..#";

    [Fact]
    public void Visibility_updated_on_travel()
    {
        var level = TestHelper.BuildRoom(TestMap);
        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 1));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
        player.Position!.Heading = Direction.West;
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.Dagger.Instantiate(undine);

        manager.Queue.ProcessQueue(manager);

        var dagger = undine.Being!.Items.Single();
        Assert.Equal(SenseType.Sight, manager.SensorySystem.CanSense(player, undine));
        Assert.Equal(SenseType.Sight, manager.SensorySystem.SensedByPlayer(undine, undine.Position!.Cell));
        Assert.Equal(SenseType.Sight | SenseType.Touch | SenseType.Telepathy,
            manager.SensorySystem.CanSense(player, player));
        Assert.Equal(SenseType.Sight | SenseType.Touch | SenseType.Telepathy,
            manager.SensorySystem.SensedByPlayer(player, player.Position.Cell));
        Assert.Equal(SenseType.Sight, manager.SensorySystem.CanSense(player, dagger));
        Assert.Equal(SenseType.Sight, manager.SensorySystem.SensedByPlayer(dagger, undine.Position.Cell));
        var expectedVisibleMap = @"
.
.
.";
        TestHelper.AssertVisibility(level, expectedVisibleMap, level.VisibleTerrain);
        var expectedKnownMap = @"
.
.
.";
        TestHelper.AssertTerrain(level, expectedKnownMap, level.KnownTerrain);

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.South;
        travelMessage.TargetCell = player.Position.Cell;
        manager.Enqueue(travelMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, undine));
        Assert.Equal(SenseType.None, manager.SensorySystem.SensedByPlayer(undine, undine.Position.Cell));
        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, dagger));
        Assert.Equal(SenseType.None, manager.SensorySystem.SensedByPlayer(dagger, undine.Position.Cell));
        expectedVisibleMap = @"
 
 
..#";
        TestHelper.AssertVisibility(level, expectedVisibleMap, level.VisibleTerrain);
        expectedKnownMap = @"
.
.
..#";
        TestHelper.AssertTerrain(level, expectedKnownMap, level.KnownTerrain);
    }
}
