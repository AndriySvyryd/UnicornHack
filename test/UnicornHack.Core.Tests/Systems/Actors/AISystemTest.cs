using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Actors;

public class AISystemTest
{
    private const string Map = @"
.....
.....
.....
.....
.....";

    /// <summary>
    ///     An NPC in a room no living player occupies must just pass its turn instead of
    ///     wandering.
    /// </summary>
    [Fact]
    public void Actor_in_room_without_a_player_waits()
    {
        var game = TestHelper.CreateGame(seed: 1u);

        // Player is in its own room.
        var playerRoom = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, playerRoom, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        // NPC is alone in a different room.
        var otherRoom = TestHelper.BuildRoom(Map, game, depth: 1);
        var npc = CreatureData.Human.Instantiate(otherRoom, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var ai = npc.AI!;
        ai.NextAction = null;
        ai.NextActionTarget = null;
        ai.NextActionTarget2 = null;

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.Wait, ai.NextAction);
    }

    /// <summary>
    ///     An NPC alone in a room (no living player) must just pass its turn — it must not
    ///     wander, even toward an adjacent item. With no player observing the room the actor
    ///     never moves, so it never walks onto (and picks up) floor items off-screen.
    /// </summary>
    [Fact]
    public void Actor_with_an_adjacent_item_and_no_player_waits_instead_of_moving()
    {
        var game = TestHelper.CreateGame(seed: 1u);

        var playerRoom = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, playerRoom, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        // NPC alone in a different room, with an item right next to it.
        var otherRoom = TestHelper.BuildRoom(Map, game, depth: 1);
        var npc = CreatureData.Human.Instantiate(otherRoom, new Point(0, 0));
        npc.Position!.Heading = Direction.East;
        ItemData.GoldCoin.Instantiate(otherRoom, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var goldEntity = otherRoom.Items[new Point(1, 0)];

        var ai = npc.AI!;
        ai.NextAction = null;
        ai.NextActionTarget = null;
        ai.NextActionTarget2 = null;

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);

        // It decided to wait rather than move, so it stayed put and never picked the item up.
        Assert.Equal(ActorActionType.Wait, ai.NextAction);
        Assert.Equal(new Point(0, 0), npc.Position!.Cell);
        Assert.Null(goldEntity.Item!.ContainerId);
        Assert.Same(goldEntity, otherRoom.Items[new Point(1, 0)]);
    }

    /// <summary>
    ///     Guard against over-restriction: an NPC moving over an item in a room a player
    ///     occupies still picks it up.
    /// </summary>
    [Fact]
    public void Actor_in_room_with_a_player_picks_up_items()
    {
        var game = TestHelper.CreateGame(seed: 1u);

        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(4, 4));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.Human.Instantiate(room, new Point(0, 0));
        npc.Position!.Heading = Direction.East;
        ItemData.GoldCoin.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var goldEntity = room.Items[new Point(1, 0)];

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = npc;
        travelMessage.TargetCell = new Point(1, 0);
        travelMessage.TargetHeading = Direction.East;
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new Point(1, 0), npc.Position!.Cell);
        Assert.Equal(npc.Id, goldEntity.Item!.ContainerId);
        Assert.False(room.Items.ContainsKey(new Point(1, 0)));
    }
}
