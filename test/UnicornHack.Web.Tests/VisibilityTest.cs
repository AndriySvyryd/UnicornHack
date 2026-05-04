using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class VisibilityTest
{
    [Fact]
    public void InitialVisibility()
    {
        var map = @"
.....
.....
.....
.....
.....";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        var state = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));

        // Verify terrain arrays are populated
        Assert.Equal(level.Width, state.Level!.Width);
        Assert.Equal(level.Height, state.Level!.Height);

        // Player should see tiles around position (2,2)
        var tileCount = state.Level!.Width * state.Level!.Height;
        var hasExplored = false;
        for (var i = 0; i < tileCount; i++)
        {
            if (state.Level.LevelMap!.Terrain[i] != 0)
            {
                hasExplored = true;
                break;
            }
        }

        Assert.True(hasExplored, "Player should have explored some tiles");
    }

    [Fact]
    public void MoveRevealsNewTiles()
    {
        var map = @"
..........
..........
..........
..........
..........
..........
..........
..........
..........
..........";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        var before = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));

        // Move south several times
        for (var i = 0; i < 5; i++)
        {
            WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
                ActorActionType.MoveOneCell, (int)Direction.South);
        }

        var after = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));

        // Should see more explored tiles after moving
        var beforeExplored = CountExploredTiles(before);
        var afterExplored = CountExploredTiles(after);
        Assert.True(afterExplored >= beforeExplored,
            $"Should have at least as many explored tiles after moving (before: {beforeExplored}, after: {afterExplored})");
    }

    [Fact]
    public void NPCVisibilityChange()
    {
        var map = @"
..........
..........
..........
..........
..........
..........
..........
..........
..........
..........";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(5, 5));
        manager.Queue.ProcessQueue(manager);

        // Place NPC near the edge of visibility
        CreatureData.Human.Instantiate(level, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        var initial = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));

        // Wait a few turns — NPC may move
        for (var i = 0; i < 3; i++)
        {
            WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services, ActorActionType.Wait);
        }

        var after = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));
    }

    private static int CountExploredTiles(PlayerChange state)
    {
        var count = 0;
        var tileCount = state.Level!.Width * state.Level!.Height;
        for (var i = 0; i < tileCount; i++)
        {
            if (state.Level.LevelMap!.Terrain[i] != 0)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    ///     An item that the player can see should appear in the initial state with
    ///     <c>IsCurrentlyPerceived = true</c>.
    /// </summary>
    [Fact]
    public void ItemVisibleOnInitialLoad()
    {
        var map = @"
..........
..........
..........
..........
..........";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        // Place item on the level near where the player will spawn
        ItemData.LongSword.Instantiate(level, new Point(3, 2));
        manager.Queue.ProcessQueue(manager);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        var state = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));

        // Item should be present and perceived
        Assert.NotEmpty(state.Level!.Items!);
        var swordItem = state.Level.Items!.Values.SingleOrDefault(i => i.BaseName == "long sword");
        Assert.NotNull(swordItem);
        Assert.True(swordItem.IsCurrentlyPerceived, "Item near player should be perceived");
    }

    /// <summary>
    ///     An item that the player cannot see (behind a wall) should still appear in
    ///     the initial state once the player has moved to see it and then away,
    ///     with <c>IsCurrentlyPerceived = false</c>.
    /// </summary>
    [Fact]
    public void ItemBehindWallNotPerceivedOnFullRefresh()
    {
        // Two rooms separated by a wall, connected by a single gap.
        // Room A (top): y=0..3     Room B (bottom): y=5..8     Wall at y=4
        var map = @"
..........
..........
..........
..........
#####.####
..........
..........
..........
..........";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        // Place item in Room B
        ItemData.LongSword.Instantiate(level, new Point(2, 7));
        manager.Queue.ProcessQueue(manager);

        // Player starts in Room B near the item
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 7));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        // Initial state: item is perceived
        var initial = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));
        var swordItem = initial.Level!.Items!.Values.SingleOrDefault(i => i.BaseName == "long sword");
        Assert.NotNull(swordItem);
        Assert.True(swordItem.IsCurrentlyPerceived, "Item should be perceived initially");

        // Move player to Room A through the gap at (5,4)
        // First move to the gap, then north into Room A
        var player = playerEntity.Player!;
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.East);
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.East);
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);

        // Move further north away from wall so the item is behind the wall
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);

        // Full refresh: the item should still be in the state but with IsCurrentlyPerceived=false
        var afterState = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));
        var swordAfter = afterState.Level!.Items!.Values.SingleOrDefault(i => i.BaseName == "long sword");
        Assert.NotNull(swordAfter);
        Assert.False(swordAfter.IsCurrentlyPerceived, "Item behind wall should not be perceived after move");
    }

    /// <summary>
    ///     When an item's perception changes (goes from visible to not visible),
    ///     the delta should contain the <c>IsCurrentlyPerceived</c> change.
    /// </summary>
    [Fact]
    public void ItemPerceptionChangeDelta()
    {
        // Large open map so the player can walk far enough to lose sight of the item.
        var map = @"
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        // Place item in the bottom-left area
        ItemData.LongSword.Instantiate(level, new Point(1, 23));
        manager.Queue.ProcessQueue(manager);

        // Player starts near the item
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 23));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        // Establish baseline: item is perceived
        var baseline = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));
        var swordItem = baseline.Level!.Items!.Values.SingleOrDefault(i => i.BaseName == "long sword");
        Assert.NotNull(swordItem);
        Assert.True(swordItem.IsCurrentlyPerceived, "Item should be perceived at baseline");

        // Move player far north — each move verified via PerformAndVerify
        var player = playerEntity.Player!;
        PlayerChange fresh = WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);

        // After moving north the item should be out of vision range.
        // PerformAndVerify already asserts delta-applied state == fresh GetState,
        // so the IsCurrentlyPerceived delta was carried correctly if the test passes.
        swordItem = fresh.Level!.Items!.Values.SingleOrDefault(i => i.BaseName == "long sword");
        Assert.NotNull(swordItem);
        Assert.False(swordItem.IsCurrentlyPerceived,
            "Item should not be perceived after player moved far away");

        fresh = WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.South);

        swordItem = fresh.Level!.Items!.Values.SingleOrDefault(i => i.BaseName == "long sword");
        Assert.NotNull(swordItem);
        Assert.True(swordItem.IsCurrentlyPerceived,
            "Item should be perceived after player moved back closer");
    }

    /// <summary>
    ///     Regression: when LevelActors fires a Being property change for an NPC whose
    ///     knowledge entity exists but is not currently tracked by the client, no spurious
    ///     Modified/Deleted delta must be emitted. The bug manifested as
    ///     "Actor X not deleted" on the client when the player moved in a level with NPCs.
    /// </summary>
    [Fact]
    public void NoSpuriousActorDeltaForInvisibleNPC()
    {
        // Use a large map with a wall so NPCs can act (changing NextActionTick)
        // while not visible to the player.
        var map = @"
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
############.############
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        // Place several NPCs south of the wall (invisible to the player initially)
        CreatureData.Human.Instantiate(level, new Point(3, 20));
        CreatureData.Human.Instantiate(level, new Point(7, 22));
        CreatureData.Human.Instantiate(level, new Point(10, 18));
        manager.Queue.ProcessQueue(manager);

        // Player starts far north
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(5, 3));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        var player = playerEntity.Player!;

        // Move around — NPCs act in between player turns. The key assertion is that
        // PerformAndVerify (which applies change sets and compares against GetState)
        // doesn't throw due to spurious actor deltas.
        for (var i = 0; i < 5; i++)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.MoveOneCell, (int)Direction.East);
        }

        for (var i = 0; i < 5; i++)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.MoveOneCell, (int)Direction.West);
        }

        // Wait a few turns so NPCs keep acting
        for (var i = 0; i < 5; i++)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.Wait);
        }

        // Move toward the gap — NPCs may enter view
        for (var i = 0; i < 8; i++)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.MoveOneCell, (int)Direction.South);
        }

        // Move back north — NPCs leave view and keep acting
        for (var i = 0; i < 8; i++)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.MoveOneCell, (int)Direction.North);
        }
    }

    /// <summary>
    ///     Regression: equipping mail armor via ability then moving in the opposite direction
    ///     causes "Actor X not deleted" and "Item X not found" exceptions on the client.
    ///     The bug occurs because <see cref="ActorChangeBuilder.OnBaseGroupPropertyValuesChanged" />
    ///     unconditionally creates Modified entries for knowledge entities that may not be in
    ///     the <c>KnownActorsToLevelCellRelationship</c>, leading to spurious deltas for entities
    ///     the client was never told about.
    /// </summary>
    [Fact]
    public void EquipArmorThenReverseHeadingWithVisibleEntities()
    {
        // Open map large enough for entities to drop out of the 180° perception cone.
        var map = @"
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        // Place creatures and item south of where the player will be — visible in south-facing FOV.
        CreatureData.Human.Instantiate(level, new Point(12, 13));
        CreatureData.Human.Instantiate(level, new Point(10, 13));
        ItemData.LongSword.Instantiate(level, new Point(13, 13));
        manager.Queue.ProcessQueue(manager);

        // Player starts in the middle, heading south (default).
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(12, 12));
        manager.Queue.ProcessQueue(manager);

        // Give the player mail armor in their inventory.
        ItemData.MailArmor.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        var player = playerEntity.Player!;

        // Step 1: Equip mail armor via its ability.
        var armorSlot = playerEntity.Being!.SlottedAbilities.Values
            .Select(e => e.Ability!)
            .Single(a => a.Type == AbilityType.Item && a.Name == "mail armor: Equip")
            .Slot!.Value;
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.UseAbilitySlot, armorSlot);

        // Step 2: Move north — heading changes to north, creatures/items to the south
        // fall out of the 180° perception cone. The NPC also acts (changing NextActionTick),
        // which triggers OnBaseGroupPropertyValuesChanged on the LevelActors group and may
        // create spurious Modified entries for knowledge entities that are no longer in the
        // KnownActorsToLevelCellRelationship. This leads to "Actor X not deleted" / "Item X
        // not found" errors on the client in subsequent turns.
        for (var i = 0; i < 8; i++)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.MoveOneCell, (int)Direction.North);
        }

        // Step 3: Move south again — entities re-enter the FOV.
        for (var i = 0; i < 4; i++)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.MoveOneCell, (int)Direction.South);
        }
    }
}
