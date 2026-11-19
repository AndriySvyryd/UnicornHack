using UnicornHack.Systems.Abilities;
using UnicornHack.Generation;
using UnicornHack.Data.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Features;
using UnicornHack.Data.Features;
using Xunit;

namespace UnicornHack.Persistence;

public class PlayerChoicesPersistenceTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Feature_destroy_permission_and_template_derived_collision_survive_reload(bool destroyable)
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var room = persisted.PlayerEntity.Position!.RoomEntity.Room!;
        var definition = new Feature
        {
            Name = "test persisted feature state",
            Behavior = destroyable ? FeatureBehavior.BlocksMovement | FeatureBehavior.AllowDestroy : FeatureBehavior.None
        };
        ((TestDataProvider)persisted.Game.Services.Data).Features.Add(definition);
        var entity = definition.Instantiate(room, new Point(3, 1));
        var featureId = entity.Id;
        persisted.Game.Manager.Queue.ProcessQueue(persisted.Game.Manager);
        persisted.Context.SaveChanges();

        persisted.Reload(configureData: data => data.Features.Add(definition));

        var feature = persisted.Game.Manager.FindEntity(featureId)!.Feature!;
        Assert.Same(definition, feature.Template);
        Assert.Equal(destroyable ? false : (bool?)null, feature.Destroyed);
        Assert.Equal(destroyable, feature.BlocksMovement);

        if (destroyable)
        {
            feature.Destroyed = true;
            persisted.Context.SaveChanges();
            persisted.Reload(configureData: data => data.Features.Add(definition));
            feature = persisted.Game.Manager.FindEntity(featureId)!.Feature!;
            Assert.True(feature.Destroyed);
            Assert.True(feature.BlocksMovement);
        }
    }

    [Fact]
    public void Runtime_player_options_survive_reload_without_a_feature_or_reward_record()
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var name = AbilityData.FurrowsReserve.Name;
        player.Player!.PendingChoice = new PlayerOptions
        {
            ChoiceType = RewardType.Ritual,
            PendingChoiceOptions = [name]
        };
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        var manager = player.Manager;
        Assert.Null(player.Player!.PendingChoice!.SourceFeatureId);
        Assert.IsType<List<string>>(player.Player.PendingChoice.PendingChoiceOptions);
        Assert.Equal([name], player.Player.PendingChoice.PendingChoiceOptions);
        Assert.Equal(name, Assert.Single(manager.PlayerChoiceSystem.GetPresentation(player)!.Options).Identifier);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        Assert.Null(player.Player!.PendingChoice);
        Assert.False(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, name));
        Assert.Null(player.Player.PendingChoice);
        Assert.Empty(player.Position!.RoomEntity.Room!.Features);
    }

    [Fact]
    public void Perceived_feature_knowledge_survives_reload_and_controls_feature_snapshots()
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var room = player.Position!.RoomEntity.Room!;
        var manager = player.Manager;
        var turn = TravelMessage.Create(manager);
        turn.ActorEntity = player;
        turn.TargetCell = player.Position.Cell;
        turn.TargetHeading = Direction.East;
        turn.SuppressDelay = true;
        manager.Process(turn);
        manager.Queue.ProcessQueue(manager);
        var known = FeatureData.SmallFountain.Instantiate(room, new Point(3, 1));
        var unknown = FeatureData.SmallFountain.Instantiate(room, new Point(0, 1));
        manager.Queue.ProcessQueue(manager);
        Assert.NotNull(known.Position!.Knowledge);
        Assert.Null(unknown.Position!.Knowledge);
        var knownId = known.Id;
        var unknownId = unknown.Id;
        var knowledgeId = known.Position.Knowledge.Id;
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        room = player.Position!.RoomEntity.Room!;
        known = manager.FindEntity(knownId)!;
        unknown = manager.FindEntity(unknownId)!;
        Assert.Equal(knowledgeId, known.Position!.Knowledge!.Id);
        Assert.Same(known.Position.Knowledge, room.KnownFeatures[new Point(3, 1)]);
        Assert.Null(unknown.Position!.Knowledge);
        var state = GameStateManager.GetState(player, persisted.Game.Services);
        Assert.Contains(knownId, state.Room!.Features!.Keys);
        Assert.DoesNotContain(unknownId, state.Room.Features.Keys);
    }

    [Fact]
    public void Standalone_reward_boons_keep_their_template_and_personal_use_after_reload()
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        var room = player.Position!.RoomEntity.Room!;
        var cell = new Point(3, 1);
        var feature = FeatureData.RitualShrine.Instantiate(room, cell);
        manager.Queue.ProcessQueue(manager);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
        var name = player.Player!.PendingChoice!.PendingChoiceOptions[0];
        Assert.NotNull(Ability.Loader.Find(name));
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);
        var featureId = feature.Id;
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        var ability = manager.AffectableAbilitiesIndex[(player.Id, name)]!.Ability!;
        Assert.Equal(name, ability.Template!.Name);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, name));
        Assert.True(PlayerChoiceSystem.HasUsedFeature(manager.FindEntity(featureId)!.Feature!, player));
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, player.Position!.RoomEntity.Room!, cell));
    }

    [Theory]
    [InlineData(EffectDuration.UntilTimeout, 450)]
    [InlineData(EffectDuration.UntilXPGained, 73)]
    public void Boon_lifetime_and_trap_deadlines_are_restored_without_resetting(EffectDuration duration, int target)
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        var room = player.Position!.RoomEntity.Room!;
        var launcher = FeatureData.FireboltLauncher.Instantiate(room, new Point(3, 1));
        var spike = FeatureData.SpikeTrap.Instantiate(room, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);
        launcher.Feature!.NextActionTick = 700;
        spike.ActiveEffects!.Abilities.Single().Ability!.CooldownTick = 456;
        manager.Game.CurrentTick = 123;
        manager.AbilityLevelSystem.AddAbilityEffect(player, AbilityData.PatientVigor,
            expiration: new EffectExpiration(duration, target));
        manager.Queue.ProcessQueue(manager);
        var launcherId = launcher.Id;
        var spikeId = spike.Id;
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        launcher = manager.FindEntity(launcherId)!;
        spike = manager.FindEntity(spikeId)!;
        Assert.Equal(123, manager.Game.CurrentTick);
        Assert.Equal(700, launcher.Feature!.NextActionTick);
        Assert.Same(launcher, manager.TemporalEntitiesIndex[(700, (int)EntityComponent.Feature, launcher.Id)]);
        Assert.Null(spike.Feature!.NextActionTick);
        Assert.Equal(456, spike.ActiveEffects!.Abilities.Single().Ability!.CooldownTick);
        Assert.Same(manager.Game.Services.Data.Features.Get(launcher.Feature.Name).Abilities[0],
            launcher.ActiveEffects!.Abilities.Single().Ability!.Template);
        Assert.Same(manager.Game.Services.Data.Features.Get(spike.Feature.Name).Abilities[0],
            spike.ActiveEffects.Abilities.Single().Ability!.Template);
        var grant = player.ActiveEffects!.ComponentEffects.Single(effect => effect.TargetName == AbilityData.PatientVigor.Name);
        Assert.Equal(duration, grant.ExpirationEvent);
        Assert.Equal(target, grant.ExpirationTarget);
    }

    [Fact]
    public void Reward_preview_and_immediate_claim_are_sent_as_player_deltas()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom("..", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        game.Manager.Queue.ProcessQueue(game.Manager);
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Barley", [AbilityData.BriefVigor.Name],
            rewardType: RewardType.Effect);
        var listener = WebTestHelper.SetupListeners(game.Manager);
        var initialEnergy = playerEntity.Being!.EnergyPointMaximum;

        var menu = WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.East);
        Assert.Equal(RewardType.Effect, menu.PendingOptions!.Choice);
        Assert.Equal(room.Features[new Point(1, 0)].Feature!.Template!.Name, menu.PendingOptions.Title);

        var claimed = WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.SelectChoice, 0);
        Assert.Null(claimed.PendingOptions);
        Assert.Equal(initialEnergy + 10, playerEntity.Being.EnergyPointMaximum);
        var grant = Assert.Single(playerEntity.ActiveEffects!.ComponentEffects.GetValues(AbilityData.PatientVigor.Name));
        Assert.Equal(EffectDuration.UntilTimeout, grant.ExpirationEvent);
        Assert.Equal(300, grant.ExpirationTarget);
        Assert.Equal(RewardType.Effect, room.Features[new Point(1, 0)].Feature!.Template!.RewardType);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(room.Features[new Point(1, 0)].Feature!, playerEntity));
    }

    [Fact]
    public void Reward_source_options_and_personal_claim_survive_reload()
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var room = player.Position!.RoomEntity.Room!;
        var cell = new Point(3, 1);
        var reward = TestHelper.CreateRewardFeature(room, cell, "Wheat", [AbilityData.EarnedReserve.Name],
            rewardType: RewardType.Effect);
        var definition = reward.Template!;
        var branchDefinition = persisted.Game.Services.Data.Branches.Get(room.BranchName);
        Action<TestDataProvider> configureData = data =>
        {
            data.Features.Add(definition);
            data.Branches.Add(branchDefinition);
        };
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        Assert.True(persisted.Game.Manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
        var pendingOptions = player.Player!.PendingChoice!.PendingChoiceOptions;
        var rewardId = reward.EntityId;
        var roomId = room.EntityId;
        persisted.Context.SaveChanges();

        persisted.Reload(configureData: configureData);

        player = persisted.PlayerEntity;
        Assert.Equal(roomId, player.Position!.RoomId);
        Assert.Equal(rewardId, player.Player!.PendingChoice!.SourceFeatureId);
        var feature = player.Position!.RoomEntity.Room!.Features[cell];
        Assert.Equal(rewardId, feature.Id);
        Assert.Null(feature.Item);
        Assert.Null(feature.Being);
        Assert.Equal(RewardType.Effect, feature.Feature!.Template!.RewardType);
        Assert.Equal(RewardType.Effect, player.Player.PendingChoice.ChoiceType);
        Assert.Null(player.Player.PendingChoice.PendingChoiceAction);
        Assert.Equal(pendingOptions, player.Player.PendingChoice.PendingChoiceOptions);
        Assert.Single(persisted.Game.Manager.PlayerChoiceSystem.GetPresentation(player)!.Options);
        Assert.False(persisted.Game.Manager.PlayerChoiceSystem.Select(player, 1, persisted.Game.Manager).IsAccepted);
        Assert.True(persisted.Game.Manager.PlayerChoiceSystem.Select(player, 0, persisted.Game.Manager).IsAccepted);
        persisted.Game.Manager.Queue.ProcessQueue(persisted.Game.Manager);
        room = player.Position!.RoomEntity.Room!;
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        persisted.Context.SaveChanges();

        persisted.Reload(configureData: configureData);

        player = persisted.PlayerEntity;
        room = player.Position!.RoomEntity.Room!;
        Assert.Equal(RewardType.Effect, room.Features[cell].Feature!.Template!.RewardType);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(room.Features[cell].Feature!, player));
        Assert.Null(player.Player!.PendingChoice);
        Assert.False(persisted.Game.Manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
    }

    [Theory]
    [InlineData(RewardType.Item)]
    [InlineData(RewardType.Race)]
    public void Typed_reward_options_and_exhaustion_survive_reload(RewardType rewardType)
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
        var definition = TestHelper.CreateRewardFeature(room, cell, "Test reward", [name], rewardType: rewardType).Template!;
        var branchDefinition = persisted.Game.Services.Data.Branches.Get(room.BranchName);
        Action<TestDataProvider> configureData = data =>
        {
            data.Features.Add(definition);
            data.Branches.Add(branchDefinition);
        };
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
        if (rewardType == RewardType.Race)
        {
            Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        }
        persisted.Context.SaveChanges();

        persisted.Reload(configureData: configureData);

        player = persisted.PlayerEntity;
        room = player.Position!.RoomEntity.Room!;
        manager = persisted.Game.Manager;
        var feature = room.Features[cell].Feature!;
        Assert.Equal(rewardType, feature.Template!.RewardType);
        Assert.Equal(new[] { name }, player.Player!.PendingChoice!.PendingChoiceOptions);
        Assert.Equal(rewardType == RewardType.Race ? PlayerChoiceSystem.AddAction : null,
            player.Player!.PendingChoice!.PendingChoiceAction);
        Assert.Equal(rewardType, player.Player.PendingChoice.ChoiceType);
        var pendingChoice = player.Player.PendingChoice;
        Assert.Single(manager.PlayerChoiceSystem.GetPresentation(player)!.Options);
        Assert.False(manager.PlayerChoiceSystem.Select(player, 1, manager).IsAccepted);
        Assert.Same(pendingChoice, player.Player.PendingChoice);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(rewardType, feature.Template.RewardType);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, player));
        manager.EffectApplicationSystem.ExpireTransitionEffects(player, true, manager, branchChanged: true);
        manager.Queue.ProcessQueue(manager);
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        persisted.Context.SaveChanges();

        persisted.Reload(configureData: configureData);

        player = persisted.PlayerEntity;
        room = player.Position!.RoomEntity.Room!;
        Assert.Equal(rewardType, room.Features[cell].Feature!.Template!.RewardType);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(room.Features[cell].Feature!, player));
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
        FeatureGenerator.Place(room, pedestalCell, RoomFeatures.RacePedestal);
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

        Assert.Equal(RewardType.Race, menuState.PendingOptions!.Choice);
        Assert.Null(menuState.PendingOptions.Action);
        Assert.Equal(3, menuState.PendingOptions.Options.Count);

        var replacementState = WebTestHelper.PerformAndVerify(
            player,
            listener,
            game.Services,
            ActorActionType.SelectChoice,
            1);

        var replacementCards = replacementState.PendingOptions!;
        Assert.Equal(RewardType.Race, replacementCards.Choice);
        Assert.Equal(PlayerChoiceSystem.ReplaceAction, replacementCards.Action);
        Assert.Equal(4, replacementCards.Options.Count);

        var removeState = WebTestHelper.PerformAndVerify(
            player,
            listener,
            game.Services,
            ActorActionType.SelectChoice,
            0);

        var removeCards = removeState.PendingOptions!;
        Assert.Equal(PlayerChoiceSystem.RemoveAction, removeCards.Action);
        Assert.Equal(2, removeCards.Options.Count);
        Assert.Single(playerEntity.Being!.Races);
        Assert.Equal(MapFeature.StoneFloor, room.Terrain[
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
            MapFeature.StoneFloor,
            room.Terrain[room.PointToIndex![pedestalCell.X, pedestalCell.Y]]);
        Assert.True(room.FeatureTags.HasFlag(RoomFeatures.RacePedestal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Race_pedestal_replacement_survives_reload_and_resolves_in_core(bool selectNewRaceBeforeReload)
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var playerEntity = persisted.PlayerEntity;
        var room = playerEntity.Position!.RoomEntity.Room!;
        var pedestalCell = new Point(3, 1);
        var featureId = FeatureGenerator.Place(room, pedestalCell, RoomFeatures.RacePedestal).Id;
        room.FeatureTags |= RoomFeatures.RacePedestal;
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;

        playerEntity.Player!.NextAction = ActorActionType.MoveOneCell;
        playerEntity.Player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, persisted.Game.Manager);
        var replacementTransition = persisted.Game.Manager.PlayerChoiceSystem.Select(
            playerEntity, optionIndex: 1, persisted.Game.Manager);
        Assert.True(replacementTransition.IsAccepted, replacementTransition.ErrorMessage);
        var newRace = playerEntity.Player!.PendingChoice!.PendingChoiceOptions[0];
        if (selectNewRaceBeforeReload)
        {
            var removeTransition = persisted.Game.Manager.PlayerChoiceSystem.Select(
                playerEntity, optionIndex: 0, persisted.Game.Manager);
            Assert.True(removeTransition.IsAccepted, removeTransition.ErrorMessage);
        }
        Assert.Single(playerEntity.Being!.Races);
        var expectedAction = selectNewRaceBeforeReload ? PlayerChoiceSystem.RemoveAction : PlayerChoiceSystem.ReplaceAction;
        Assert.Equal(expectedAction, playerEntity.Player!.PendingChoice!.PendingChoiceAction);
        var expectedOptions = playerEntity.Player.PendingChoice!.PendingChoiceOptions;
        persisted.Context.SaveChanges();

        persisted.Reload();

        playerEntity = persisted.PlayerEntity;
        Assert.Equal(RewardType.Race, playerEntity.Player!.PendingChoice!.ChoiceType);
        Assert.Equal(expectedAction, playerEntity.Player.PendingChoice.PendingChoiceAction);
        Assert.Equal(expectedOptions, playerEntity.Player.PendingChoice.PendingChoiceOptions);
        var feature = persisted.Game.Manager.FindEntity(featureId)!.Feature!;
        Assert.Equal(selectNewRaceBeforeReload ? newRace : null, playerEntity.Player.PendingChoice.SelectedOption);
        Assert.False(PlayerChoiceSystem.HasUsedFeature(feature, playerEntity));
        if (!selectNewRaceBeforeReload)
        {
            Assert.True(persisted.Game.Manager.PlayerChoiceSystem.Select(playerEntity, 0, persisted.Game.Manager).IsAccepted);
        }
        Assert.Equal(PlayerChoiceSystem.RemoveAction, playerEntity.Player.PendingChoice!.PendingChoiceAction);
        Assert.Equal(newRace, playerEntity.Player.PendingChoice.SelectedOption);
        Assert.DoesNotContain(newRace, playerEntity.Player.PendingChoice.PendingChoiceOptions);
        Assert.Single(playerEntity.Being!.Races);
        var completion = persisted.Game.Manager.PlayerChoiceSystem.Select(
            playerEntity, optionIndex: 0, persisted.Game.Manager);
        Assert.True(completion.IsAccepted, completion.ErrorMessage);
        room = playerEntity.Position!.RoomEntity.Room!;
        persisted.Context.Entry(room).Property(candidate => candidate.Terrain).IsModified = true;
        persisted.Context.SaveChanges();

        persisted.Reload();

        Assert.Null(persisted.PlayerEntity.Player!.PendingChoice);
        Assert.Single(persisted.PlayerEntity.Being!.Races);
        Assert.Contains(persisted.Game.Services.Data.PlayerRaces.Get(newRace).GetRaceAbilityName(),
            AbilityLevelSystem.GetPlayerRaceAbilityNames(persisted.PlayerEntity));
        feature = persisted.Game.Manager.FindEntity(featureId)!.Feature!;
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, persisted.PlayerEntity));
        room = persisted.PlayerEntity.Position!.RoomEntity.Room!;
        Assert.Equal(
            MapFeature.StoneFloor,
            room.Terrain[room.PointToIndex![pedestalCell.X, pedestalCell.Y]]);
        Assert.True(room.FeatureTags.HasFlag(RoomFeatures.RacePedestal));
    }
}
