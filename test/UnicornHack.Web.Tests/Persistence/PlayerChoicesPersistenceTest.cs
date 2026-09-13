using UnicornHack.Generation;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using UnicornHack.TestUtilities;
using Xunit;

namespace UnicornHack.Persistence;

public class PlayerChoicesPersistenceTest
{
    [Fact]
    public void Race_pedestal_menu_cards_and_result_are_sent_as_player_deltas()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom("..", game);
        var pedestalCell = new Point(1, 0);
        TerrainSystem.SetTerrain(MapFeature.RacePedestal, pedestalCell, room);
        room.Features |= RoomFeatures.RacePedestal;
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Tester", Sex.Male, room, new Point(0, 0));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);
        var player = playerEntity.Player!;
        var listener = WebTestHelper.SetupListeners(manager);

        var menuState = WebTestHelper.PerformAndVerify(
            player,
            listener,
            game.Services,
            ActorActionType.MoveOneCell,
            (int)Direction.East);

        Assert.Equal(PlayerChoiceSystem.RaceChoice, menuState.PendingOptions!.Choice);
        Assert.Null(menuState.PendingOptions.Action);
        Assert.Equal(3, menuState.PendingOptions.Options.Count);

        var removeState = WebTestHelper.PerformAndVerify(
            player,
            listener,
            game.Services,
            ActorActionType.SelectChoice,
            1);

        var removeCards = removeState.PendingOptions!;
        Assert.Equal(PlayerChoiceSystem.RaceChoice, removeCards.Choice);
        Assert.Equal(PlayerChoiceSystem.RemoveAction, removeCards.Action);
        Assert.Equal(2, removeCards.Options.Count);

        var addState = WebTestHelper.PerformAndVerify(
            player,
            listener,
            game.Services,
            ActorActionType.SelectChoice,
            0);

        var addCards = addState.PendingOptions!;
        Assert.Equal(PlayerChoiceSystem.AddAction, addCards.Action);
        Assert.Equal(4, addCards.Options.Count);
        Assert.Empty(playerEntity.Being!.Races);
        Assert.Equal(MapFeature.RacePedestal, room.Terrain[
            room.PointToIndex![pedestalCell.X, pedestalCell.Y]]);

        var finalState = WebTestHelper.PerformAndVerify(
            player,
            listener,
            game.Services,
            ActorActionType.SelectChoice,
            0);

        Assert.Null(finalState.PendingOptions);
        Assert.Single(playerEntity.Being!.Races);
        Assert.Equal(
            MapFeature.InactiveRacePedestal,
            room.Terrain[room.PointToIndex![pedestalCell.X, pedestalCell.Y]]);
        Assert.True(room.Features.HasFlag(RoomFeatures.RacePedestal));
    }

    [Fact]
    public void Race_pedestal_cards_survive_reload_and_resolve_in_core()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var playerEntity = persisted.PlayerEntity;
        var room = playerEntity.Position!.RoomEntity.Room!;
        var pedestalCell = new Point(3, 1);
        TerrainSystem.SetTerrain(MapFeature.RacePedestal, pedestalCell, room);
        room.Features |= RoomFeatures.RacePedestal;
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;

        playerEntity.Player!.NextAction = ActorActionType.MoveOneCell;
        playerEntity.Player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, persisted.Game.Manager);
        var removeTransition = persisted.Game.Manager.PlayerChoiceSystem.Select(
            playerEntity, optionIndex: 1, persisted.Game.Manager);
        Assert.True(removeTransition.IsAccepted, removeTransition.ErrorMessage);
        var addTransition = persisted.Game.Manager.PlayerChoiceSystem.Select(
            playerEntity, optionIndex: 0, persisted.Game.Manager);
        Assert.True(addTransition.IsAccepted, addTransition.ErrorMessage);
        Assert.Empty(playerEntity.Being!.Races);
        Assert.Equal(PlayerChoiceSystem.AddAction, playerEntity.Player!.PendingChoice!.PendingChoiceAction);
        var expectedOptions = playerEntity.Player.PendingChoice!.PendingChoiceOptions;
        persisted.Context.SaveChanges();

        persisted.Reload();

        playerEntity = persisted.PlayerEntity;
        Assert.Equal(PlayerChoiceSystem.RaceChoice, playerEntity.Player!.PendingChoice!.ChoiceType);
        Assert.Equal(PlayerChoiceSystem.AddAction, playerEntity.Player.PendingChoice.PendingChoiceAction);
        Assert.Equal(expectedOptions, playerEntity.Player.PendingChoice.PendingChoiceOptions);
        var completion = persisted.Game.Manager.PlayerChoiceSystem.Select(
            playerEntity, optionIndex: 0, persisted.Game.Manager);
        Assert.True(completion.IsAccepted, completion.ErrorMessage);
        room = playerEntity.Position!.RoomEntity.Room!;
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        persisted.Context.SaveChanges();

        persisted.Reload();

        Assert.Null(persisted.PlayerEntity.Player!.PendingChoice);
        Assert.Single(persisted.PlayerEntity.Being!.Races);
        room = persisted.PlayerEntity.Position!.RoomEntity.Room!;
        Assert.Equal(
            MapFeature.InactiveRacePedestal,
            room.Terrain[room.PointToIndex![pedestalCell.X, pedestalCell.Y]]);
        Assert.True(room.Features.HasFlag(RoomFeatures.RacePedestal));
    }
}
