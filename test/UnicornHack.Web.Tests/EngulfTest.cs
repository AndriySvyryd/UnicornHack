using UnicornHack.Data.Creatures;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack;

public class EngulfTest
{
    private const string TestMap = @"
.....
.....
.....
.....
.....";

    private static (GameManager Manager, GameEntity PlayerEntity, RoomChangeBuilder Listener)
        SetupGameWithListener()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        return (manager, playerEntity, listener);
    }

    // Drops a gelatinous cube next to the player and has it grapple them.
    private static GameEntity EngulfPlayer(GameManager manager, GameEntity playerEntity)
    {
        var room = playerEntity.Position!.RoomEntity.Room!;
        var playerCell = playerEntity.Position!.Cell!.Value;

        var engulfer = CreatureData.GelatinousCube.Instantiate(
            room, FindAdjacentWalkableCell(room, playerCell, manager));
        manager.Queue.ProcessQueue(manager);

        var engulfEffect = engulfer.ActiveEffects!.Abilities
            .SelectMany(a => a.Ability!.Effects)
            .Single(e => e.Effect!.EffectType == EffectType.Engulf);
        TestHelper.ApplyEffect(engulfEffect, engulfer, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        return engulfer;
    }

    private static Point FindAdjacentWalkableCell(RoomComponent room, Point excluded, GameManager manager)
        => manager.TravelSystem.FindCell(
               room, manager.TravelSystem.IsActorFreeCell, origin: excluded, maxRadius: 1, excluded: excluded)
           ?? throw new InvalidOperationException("No walkable cell found in the player's room.");

    [Fact]
    public void Full_snapshot_of_an_engulfed_player_uses_the_normal_room_state()
    {
        var (manager, playerEntity, _) = SetupGameWithListener();
        var services = manager.Game.Services;

        // Baseline: an un-engulfed player has a room.
        var baseline = GameStateManager.GetState(playerEntity, services);
        Assert.NotNull(baseline.Room);

        var engulfer = EngulfPlayer(manager, playerEntity);
        Assert.NotNull(playerEntity.Position!.Cell);
        Assert.Equal(1, playerEntity.Position.Cell.Value.DistanceTo(engulfer.Position!.Cell!.Value));
        Assert.True((EffectApplicationSystem.GetAbilityLevel(playerEntity, "engulfed") > 0));

        var state = GameStateManager.GetState(playerEntity, services);
        Assert.NotNull(state.Room);

        var engulferKnowledgeId = engulfer.Position!.Knowledge!.Id;
        Assert.True(state.Room!.Actors!.ContainsKey(engulferKnowledgeId));
        Assert.Null(state.Room!.Actors![engulferKnowledgeId].BaseName);
    }

    [Fact]
    public void Becoming_engulfed_uses_an_ordinary_room_delta()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var services = manager.Game.Services;

        var engulfer = EngulfPlayer(manager, playerEntity);

        var builder = listener.PlayerBuilders.Single();
        Assert.True(builder.HasObservableChanges(manager));

        var change = builder.BuildChangeSet(manager, services);
        Assert.NotNull(change.ChangedProperties);

        var engulferKnowledgeId = engulfer.Position!.Knowledge!.Id;
        Assert.NotNull(change.Room);
        Assert.NotNull(change.Room!.Actors);
        Assert.True(change.Room.Actors!.ContainsKey(engulferKnowledgeId));
    }

    [Fact]
    public void An_engulfed_player_remains_positioned_across_save_and_load()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var manager = persisted.Game.Manager;
        var player = persisted.PlayerEntity;
        var room = player.Position!.RoomEntity.Room!;
        var playerCell = player.Position!.Cell!.Value;

        var engulfer = CreatureData.GelatinousCube.Instantiate(
            room, FindAdjacentWalkableCell(room, playerCell, manager));
        manager.Queue.ProcessQueue(manager);

        var engulfEffect = engulfer.ActiveEffects!.Abilities
            .SelectMany(a => a.Ability!.Effects)
            .Single(e => e.Effect!.EffectType == EffectType.Engulf);
        TestHelper.ApplyEffect(engulfEffect, engulfer, player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(player.Position!.Cell);
        Assert.Equal(1, player.Position.Cell.Value.DistanceTo(engulfer.Position!.Cell!.Value));
        Assert.True((EffectApplicationSystem.GetAbilityLevel(player, "engulfed") > 0));
        var engulferId = engulfer.Id;
        var engulferCell = engulfer.Position!.Cell;
        var playerCellBeforeReload = player.Position.Cell;

        persisted.Context.SaveChanges();
        persisted.Reload();

        var reloadedManager = persisted.Game.Manager;
        var reloadedPlayer = reloadedManager.FindEntity(persisted.PlayerEntityId);
        Assert.NotNull(reloadedPlayer);
        Assert.Equal(playerCellBeforeReload, reloadedPlayer!.Position!.Cell);
        Assert.True((EffectApplicationSystem.GetAbilityLevel(reloadedPlayer, "engulfed") > 0));

        var reloadedEngulfer = reloadedManager.EngulferToVictimRelationship.GetEngulfer(reloadedPlayer);
        Assert.NotNull(reloadedEngulfer);
        Assert.Equal(engulferId, reloadedEngulfer!.Id);
        Assert.Equal(engulferCell, reloadedEngulfer.Position!.Cell);
    }
}
