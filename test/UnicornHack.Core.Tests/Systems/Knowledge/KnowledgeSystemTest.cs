using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Knowledge;

public class KnowledgeSystemTest
{
    private static readonly string TestMap = @"
...
>..
..#";

    [Fact]
    public void Knowledge_updated_on_travel()
    {
        var level = TestHelper.BuildLevel(TestMap);
        ItemData.Dagger.Instantiate(level, new Point(0, 0));
        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
        player.Position!.Heading = Direction.West;
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        var nymphKnowledge = undine.Position!.Knowledge!;
        var playerKnowledge = player.Position.Knowledge!;
        var dagger = level.Items[new Point(0, 0)];
        var daggerKnowledge = dagger.Position!.Knowledge!;
        var connection = level.Connections.Single().Value;
        var connectionKnowledge = connection.Position!.Knowledge!;

        Assert.Equal(4, manager.KnownPositions.Count);
        Assert.Single(level.KnownItems);
        Assert.Equal(2, level.KnownActors.Count);
        Assert.Single(level.KnownConnections);

        Assert.Equal(undine.Position.LevelCell, nymphKnowledge.Position!.LevelCell);
        Assert.Equal(undine.Position.Heading, nymphKnowledge.Position.Heading);
        Assert.Same(undine, nymphKnowledge.Knowledge!.KnownEntity);
        Assert.Equal(SenseType.Sight, nymphKnowledge.Knowledge.SensedType);
        Assert.Same(level.KnownActors.GetValueOrDefault(new Point(0, 0)), nymphKnowledge);

        Assert.Equal(player.Position.LevelCell, playerKnowledge.Position!.LevelCell);
        Assert.Equal(player.Position.Heading, playerKnowledge.Position.Heading);
        Assert.Same(player, playerKnowledge.Knowledge!.KnownEntity);
        Assert.Equal(SenseType.Sight | SenseType.Touch | SenseType.Telepathy, playerKnowledge.Knowledge.SensedType);
        Assert.Same(level.KnownActors.GetValueOrDefault(new Point(0, 2)), playerKnowledge);

        Assert.Equal(dagger.Position.LevelCell, daggerKnowledge.Position!.LevelCell);
        Assert.Equal(dagger.Position.Heading, daggerKnowledge.Position.Heading);
        Assert.Same(dagger, daggerKnowledge.Knowledge!.KnownEntity);
        Assert.Equal(SenseType.Sight, daggerKnowledge.Knowledge.SensedType);
        Assert.Same(level.KnownItems.GetValueOrDefault(new Point(0, 0)), daggerKnowledge);

        Assert.Equal(connection.Position.LevelCell, connectionKnowledge.Position!.LevelCell);
        Assert.Equal(connection.Position.Heading, connectionKnowledge.Position.Heading);
        Assert.Same(connection, connectionKnowledge.Knowledge!.KnownEntity);
        Assert.Equal(SenseType.Sight, connectionKnowledge.Knowledge.SensedType);
        Assert.Same(level.KnownConnections.GetValueOrDefault(new Point(0, 1)), connectionKnowledge);

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = undine;
        travelMessage.TargetHeading = Direction.East;
        travelMessage.TargetCell = new Point(1, 1);
        manager.Enqueue(travelMessage);

        var moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = dagger;
        moveMessage.TargetLevelEntity = level.Entity;
        moveMessage.TargetCell = new Point(1, 0);
        manager.Enqueue(moveMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(4, manager.KnownPositions.Count);
        Assert.Single(level.KnownItems);
        Assert.Equal(2, level.KnownActors.Count);
        Assert.Single(level.KnownConnections);
        Assert.Same(nymphKnowledge, undine.Position.Knowledge);
        Assert.Same(daggerKnowledge, dagger.Position.Knowledge);

        Assert.Equal(undine.Position.LevelCell, nymphKnowledge.Position.LevelCell);
        Assert.Equal(undine.Position.Heading, nymphKnowledge.Position.Heading);
        Assert.Same(undine, nymphKnowledge.Knowledge.KnownEntity);
        Assert.Equal(SenseType.Sight, nymphKnowledge.Knowledge.SensedType);
        Assert.Same(level.KnownActors.GetValueOrDefault(new Point(1, 1)), nymphKnowledge);

        Assert.Equal(dagger.Position.LevelCell, daggerKnowledge.Position.LevelCell);
        Assert.Equal(dagger.Position.Heading, daggerKnowledge.Position.Heading);
        Assert.Same(dagger, daggerKnowledge.Knowledge.KnownEntity);
        Assert.Equal(SenseType.Sight, daggerKnowledge.Knowledge.SensedType);
        Assert.Same(level.KnownItems.GetValueOrDefault(new Point(1, 0)), daggerKnowledge);
    }
}
