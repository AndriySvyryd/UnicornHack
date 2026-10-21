using UnicornHack.Systems.Abilities;
using UnicornHack.Generation;
using UnicornHack.Data.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack.Persistence;

public class PlayerChoicesPersistenceTest
{
    [Fact]
    public void Reward_preview_confirmation_and_claim_are_sent_as_player_deltas()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom("..", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        game.Manager.Queue.ProcessQueue(game.Manager);
        PlayerChoiceSystem.CreateRewardFeature(room, new Point(1, 0), "Barley", [AbilityData.PatientVigor.Name]);
        var listener = WebTestHelper.SetupListeners(game.Manager);
        var initialEnergy = playerEntity.Being!.EnergyPointMaximum;

        var menu = WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.East);
        Assert.Equal(RewardType.Boon, menu.PendingOptions!.Choice);
        Assert.Equal("Barley", menu.PendingOptions.Title);

        var confirmation = WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.SelectChoice, 0);
        Assert.Equal("Confirm", confirmation.PendingOptions!.Action);

        var claimed = WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.SelectChoice, 0);
        Assert.Null(claimed.PendingOptions);
        Assert.Equal(initialEnergy + 10, playerEntity.Being.EnergyPointMaximum);
        Assert.Empty(room.Features[new Point(1, 0)].Feature!.Rewards);
    }

    [Fact]
    public void Reward_source_confirmation_and_global_claim_survive_reload()
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var room = player.Position!.RoomEntity.Room!;
        var cell = new Point(3, 1);
        var reward = PlayerChoiceSystem.CreateRewardFeature(room, cell, "Wheat", [AbilityData.FurrowsReserve.Name]);
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        Assert.True(persisted.Game.Manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
        Assert.True(persisted.Game.Manager.PlayerChoiceSystem.Select(player, 0, persisted.Game.Manager).IsAccepted);
        var rewardId = reward.Id;
        var roomId = room.EntityId;
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        Assert.Equal(roomId, player.Position!.RoomId);
        Assert.Equal(rewardId, player.Player!.PendingChoice!.SourceFeatureId);
        var feature = player.Position!.RoomEntity.Room!.Features[cell];
        Assert.Equal(rewardId, feature.Id);
        Assert.Null(feature.Item);
        Assert.Null(feature.Being);
        Assert.Equal(feature.Id, Assert.Single(feature.Feature!.Rewards).FeatureId);
        Assert.Equal(RewardType.Boon, player.Player.PendingChoice.ChoiceType);
        Assert.Equal("Confirm", player.Player.PendingChoice.PendingChoiceAction);
        Assert.Single(persisted.Game.Manager.PlayerChoiceSystem.GetPresentation(player)!.Options);
        Assert.False(persisted.Game.Manager.PlayerChoiceSystem.Select(player, 1, persisted.Game.Manager).IsAccepted);
        Assert.True(persisted.Game.Manager.PlayerChoiceSystem.Select(player, 0, persisted.Game.Manager).IsAccepted);
        persisted.Game.Manager.Queue.ProcessQueue(persisted.Game.Manager);
        room = player.Position!.RoomEntity.Room!;
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        room = player.Position!.RoomEntity.Room!;
        Assert.Empty(room.Features[cell].Feature!.Rewards);
        Assert.Null(player.Player!.PendingChoice);
        Assert.False(persisted.Game.Manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
    }

    [Theory]
    [InlineData(RewardType.Item)]
    [InlineData(RewardType.Race)]
    public void Typed_reward_confirmation_and_exhaustion_survive_reload(RewardType rewardType)
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var room = player.Position!.RoomEntity.Room!;
        var manager = persisted.Game.Manager;
        var ownedRaces = AbilityLevelSystem.GetPlayerRaceAbilityNames(player);
        var name = rewardType == RewardType.Item
            ? "dagger"
            : player.Game.Services.Data.PlayerRaces.GetAll()
                .First(race => !ownedRaces.Contains(race.GetRaceAbilityName())).Name;
        var cell = new Point(3, 1);
        PlayerChoiceSystem.CreateRewardFeature(room, cell, "Test reward", [name], rewardType: rewardType);
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        room = player.Position!.RoomEntity.Room!;
        manager = persisted.Game.Manager;
        var feature = room.Features[cell].Feature!;
        var reward = Assert.Single(feature.Rewards);
        Assert.Equal(feature.EntityId, reward.FeatureId);
        Assert.Equal(rewardType, reward.Type);
        Assert.Equal(new[] { name }, reward.Names);
        Assert.Equal("Confirm", player.Player!.PendingChoice!.PendingChoiceAction);
        Assert.Equal(rewardType, player.Player.PendingChoice.ChoiceType);
        var confirmation = player.Player.PendingChoice;
        Assert.Single(manager.PlayerChoiceSystem.GetPresentation(player)!.Options);
        Assert.False(manager.PlayerChoiceSystem.Select(player, 1, manager).IsAccepted);
        Assert.Same(confirmation, player.Player.PendingChoice);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);
        Assert.Empty(feature.Rewards);
        manager.EffectApplicationSystem.ExpireTransitionEffects(player, true, manager, branchChanged: true);
        manager.Queue.ProcessQueue(manager);
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        room = player.Position!.RoomEntity.Room!;
        Assert.Empty(room.Features[cell].Feature!.Rewards);
        Assert.Null(player.Player!.PendingChoice);
        Assert.False(persisted.Game.Manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
        if (rewardType == RewardType.Item)
        {
            Assert.Contains(player.Being!.Items, item => item.Item!.TemplateName == name);
        }
        else
        {
            var race = player.Game.Services.Data.PlayerRaces.Get(name);
            Assert.Contains(race.GetRaceAbilityName(), AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
        }
    }

    [Fact]
    public void Race_pedestal_menu_cards_and_result_are_sent_as_player_deltas()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom("..", game);
        var pedestalCell = new Point(1, 0);
        TerrainSystem.SetTerrain(MapFeature.RacePedestal, pedestalCell, room);
        room.FeatureTags |= RoomFeatures.RacePedestal;
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

        Assert.Equal(RewardType.RacePedestal, menuState.PendingOptions!.Choice);
        Assert.Null(menuState.PendingOptions.Action);
        Assert.Equal(3, menuState.PendingOptions.Options.Count);

        var removeState = WebTestHelper.PerformAndVerify(
            player,
            listener,
            game.Services,
            ActorActionType.SelectChoice,
            1);

        var removeCards = removeState.PendingOptions!;
        Assert.Equal(RewardType.RacePedestal, removeCards.Choice);
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
        Assert.True(room.FeatureTags.HasFlag(RoomFeatures.RacePedestal));
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
        room.FeatureTags |= RoomFeatures.RacePedestal;
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
        Assert.Equal(RewardType.RacePedestal, playerEntity.Player!.PendingChoice!.ChoiceType);
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
        Assert.True(room.FeatureTags.HasFlag(RoomFeatures.RacePedestal));
    }
}
