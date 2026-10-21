using UnicornHack.Systems.Abilities;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Players;
using UnicornHack.Services;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Actors;

public class PlayerChoiceSystemTest
{
    [Fact]
    public void Feature_positions_update_the_room_index_without_entering_item_or_actor_collections()
    {
        var room = TestHelper.BuildRoom("...");
        var manager = room.Entity.Manager;
        var reward = PlayerChoiceSystem.CreateRewardFeature(room, new Point(1, 0), "Test reward", ["Patient Vigor"]);
        var entity = room.Features[new Point(1, 0)];
        var component = entity.Feature!;

        entity.Position!.Cell = new Point(2, 0);
        manager.Queue.ProcessQueue(manager);

        Assert.False(room.Features.ContainsKey(new Point(1, 0)));
        Assert.Same(entity, room.Features[new Point(2, 0)]);
        Assert.Empty(room.Items);
        Assert.Empty(room.Actors);
        Assert.Same(reward, Assert.Single(component.Rewards));
        entity.Position = null;
        manager.Queue.ProcessQueue(manager);
        Assert.Empty(room.Features);
        Assert.Empty(component.Rewards);
    }

    [Fact]
    public void Removing_a_feature_prevents_presenting_its_rewards()
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        PlayerChoiceSystem.CreateRewardFeature(room, new Point(1, 0), "Test reward", ["Patient Vigor"]);
        room.Features[new Point(1, 0)].Position = null;
        manager.Queue.ProcessQueue(manager);

        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.Null(player.Player!.PendingChoice);
    }

    [Fact]
    public void Rewards_in_another_room_are_not_presented()
    {
        var room = TestHelper.BuildRoom("..");
        var otherRoom = TestHelper.BuildRoom("..", room.Game);
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        var reward = PlayerChoiceSystem.CreateRewardFeature(room, new Point(1, 0), "Test reward", ["Patient Vigor"]);
        var feature = room.Features[new Point(1, 0)];
        feature.Position!.RoomEntity = otherRoom.Entity;
        manager.Queue.ProcessQueue(manager);

        Assert.Same(reward, Assert.Single(feature.Feature!.Rewards));
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, otherRoom, new Point(1, 0)));
        Assert.Null(player.Player!.PendingChoice);
        Assert.Null(reward.ClaimantEntityId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Race_rewards_filter_owned_races_before_presenting_options(bool hasAvailableRace)
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        PlayerChoiceSystem.CreateRewardFeature(room, new Point(1, 0), "Test races",
            hasAvailableRace ? [PlayerRaceData.Human.Name, PlayerRaceData.Elf.Name] : [PlayerRaceData.Human.Name],
            rewardType: RewardType.Race);

        Assert.Equal(hasAvailableRace, manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        if (!hasAvailableRace)
        {
            Assert.Null(player.Player!.PendingChoice);
            return;
        }

        Assert.Equal([PlayerRaceData.Elf.Name], player.Player!.PendingChoice!.PendingChoiceOptions);
        Assert.Equal(RewardType.Race, manager.PlayerChoiceSystem.GetPresentation(player)!.Choice);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Contains(PlayerRaceData.Elf.GetRaceAbilityName(), AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
    }

    [Fact]
    public void Removing_a_room_releases_its_features_and_owned_rewards()
    {
        var room = TestHelper.BuildRoom("..");
        var manager = room.Entity.Manager;
        var reward = PlayerChoiceSystem.CreateRewardFeature(room, new Point(1, 0), "Test reward", ["Patient Vigor"]);
        var component = room.Features[new Point(1, 0)].Feature!;

        room.Entity.Room = null;
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.FindEntity(reward.Id));
        Assert.Empty(component.Rewards);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Item_rewards_require_a_valid_destination_before_presentation(bool hasFloorDestination)
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        PlayerChoiceSystem.CreateRewardFeature(room, new Point(1, 0), "Test item", ["dagger"], rewardType: RewardType.Item);
        manager.AbilityLevelSystem.AddAbilityEffect(player, AbilityData.Stuck);
        manager.Queue.ProcessQueue(manager);
        if (!hasFloorDestination)
        {
            TerrainSystem.SetTerrain(MapFeature.RockWall, new Point(0, 0), room);
        }

        Assert.Equal(hasFloorDestination, manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.Empty(player.Being!.Items);
        Assert.Empty(room.Items);
        if (!hasFloorDestination)
        {
            Assert.Null(player.Player!.PendingChoice);
            return;
        }

        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal("dagger", Assert.Single(room.Items).Value.Item!.TemplateName);
        Assert.Empty(player.Being.Items);
    }

    [Theory]
    [InlineData(RewardType.Item)]
    [InlineData(RewardType.Race)]
    public void Claiming_an_item_or_race_removes_every_reward_from_the_feature(RewardType rewardType)
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        var name = rewardType == RewardType.Item
            ? "dagger"
            : player.Game.Services.Data.PlayerRaces.GetAll()
                .First(race => race.GetRaceAbilityName() != PlayerRaceData.Human.GetRaceAbilityName()).Name;
        var reward = PlayerChoiceSystem.CreateRewardFeature(
            room, new Point(1, 0), "Test reward", [name], rewardType: rewardType);
        var feature = room.Features[new Point(1, 0)].Feature!;
        feature.Rewards.Add(new Reward { Id = reward.Id + 1, Names = ["Patient Vigor"] });
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.Equal(rewardType, player.Player!.PendingChoice!.ChoiceType);
        Assert.Equal(rewardType, manager.PlayerChoiceSystem.GetPresentation(player)!.Choice);
        Assert.Null(manager.PlayerChoiceSystem.GetPresentation(player)!.Notice);
        Assert.Empty(player.Being!.Items);
        Assert.Empty(room.Items);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        Assert.Equal("Confirm", player.Player.PendingChoice!.PendingChoiceAction);
        Assert.Equal(rewardType, player.Player.PendingChoice.ChoiceType);

        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        if (rewardType == RewardType.Item)
        {
            Assert.Contains(player.Being!.Items, item => item.Item!.TemplateName == name);
        }
        else
        {
            var race = player.Game.Services.Data.PlayerRaces.Get(name);
            Assert.Contains(race.GetRaceAbilityName(), AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
        }

        Assert.Empty(feature.Rewards);
        Assert.Null(player.Player.PendingChoice);
        Assert.Equal(MapFeature.InactiveSmallFountain, room.Terrain[room.PointToIndex![1, 0]]);
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
    }

    [Theory]
    [InlineData("Furrow's Reserve", 10, 0, 0)]
    [InlineData("Patient Vigor", 0, 10, 0)]
    [InlineData("Hive-Sense", 0, 0, 1)]
    public void Reward_preview_is_free_and_one_confirmed_claim_grants_a_branch_bound_boon(
        string boonName, int hitPoints, int energyPoints, int roomPerceptionRange)
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        var reward = PlayerChoiceSystem.CreateRewardFeature(room, new Point(1, 0), "Produce", [boonName]);
        var feature = Assert.Single(room.Features).Value;
        Assert.Same(reward, Assert.Single(feature.Feature!.Rewards));
        Assert.Equal(feature.Id, reward.Id);
        Assert.Null(feature.Being);
        Assert.Null(feature.Item);
        Assert.Equal(new Point(1, 0), feature.Position!.Cell);
        var initialHp = player.Being!.HitPointMaximum;
        var initialEp = player.Being.EnergyPointMaximum;
        var initialPerception = player.Being.Perception;
        var initialRoomPerceptionRange = player.Being.RoomPerceptionRange;

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.Equal(0, manager.PlayerChoiceSystem.Select(player, 0, manager).Delay);
        Assert.Null(reward.ClaimantEntityId);
        Assert.Equal(0, player.Game.CurrentTick);
        var result = manager.PlayerChoiceSystem.Select(player, 0, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(result.IsAccepted);
        Assert.True(result.Delay > 0);
        Assert.Equal(player.Id, reward.ClaimantEntityId);
        Assert.Empty(feature.Feature!.Rewards);
        Assert.Equal(MapFeature.InactiveSmallFountain, room.Terrain[room.PointToIndex![1, 0]]);
        Assert.Equal(initialHp + hitPoints, player.Being.HitPointMaximum);
        Assert.Equal(initialEp + energyPoints, player.Being.EnergyPointMaximum);
        Assert.Equal(initialPerception, player.Being.Perception);
        Assert.Equal(initialRoomPerceptionRange + roomPerceptionRange, player.Being.RoomPerceptionRange);
        Assert.False(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));

        manager.EffectApplicationSystem.ExpireTransitionEffects(player, true, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(initialHp + hitPoints, player.Being.HitPointMaximum);
        Assert.Equal(initialRoomPerceptionRange + roomPerceptionRange, player.Being.RoomPerceptionRange);
        manager.EffectApplicationSystem.ExpireTransitionEffects(player, true, manager, branchChanged: true);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(initialHp, player.Being.HitPointMaximum);
        Assert.Equal(initialEp, player.Being.EnergyPointMaximum);
        Assert.Equal(initialPerception, player.Being.Perception);
        Assert.Equal(initialRoomPerceptionRange, player.Being.RoomPerceptionRange);
    }

    [Fact]
    public void Bumping_a_race_pedestal_opens_the_menu_without_advancing_time()
    {
        var (playerEntity, room, pedestalCell) = Setup();
        var player = playerEntity.Player!;

        BumpPedestal(playerEntity);

        Assert.Equal(RewardType.RacePedestal, player.PendingChoice!.ChoiceType);
        Assert.Null(player.PendingChoice.PendingChoiceAction);
        Assert.Equal(
            [PlayerChoiceSystem.AddAction, PlayerChoiceSystem.RemoveAction, PlayerChoiceSystem.DestroyAction],
            player.PendingChoice.PendingChoiceOptions);
        var presentation = playerEntity.Manager.PlayerChoiceSystem.GetPresentation(playerEntity)!;
        Assert.Equal(RewardType.RacePedestal, presentation.Choice);
        Assert.Equal("Race pedestal", presentation.Title);
        Assert.Equal(
            [
                PlayerChoiceSystem.AddAction,
                PlayerChoiceSystem.RemoveAction,
                PlayerChoiceSystem.DestroyAction
            ],
            presentation.Options.Select(option => option.Identifier));
        Assert.Null(presentation.Options[0].DisabledReason);
        Assert.Null(presentation.Options[1].DisabledReason);
        Assert.Null(presentation.Options[2].DisabledReason);
        Assert.Equal(0, player.NextActionTick);
        Assert.Equal(0, playerEntity.Game.CurrentTick);
        Assert.Equal(new Point(0, 0), playerEntity.Position!.Cell);
        Assert.Equal(Direction.East, playerEntity.Position.Heading);
        Assert.Equal(MapFeature.RacePedestal, GetTerrain(room, pedestalCell));
    }

    [Fact]
    public void Clicking_an_adjacent_race_pedestal_opens_the_menu()
    {
        var (playerEntity, _, pedestalCell) = Setup();
        playerEntity.Player!.NextAction = ActorActionType.MoveToCell;
        playerEntity.Player.NextActionTarget = pedestalCell.ToInt32();

        TestHelper.PerformTurn(playerEntity, playerEntity.Manager);

        Assert.Equal(
            RewardType.RacePedestal,
            playerEntity.Player.PendingChoice!.ChoiceType);
        Assert.Equal(new Point(0, 0), playerEntity.Position!.Cell);
        Assert.Equal(0, playerEntity.Player.NextActionTick);
    }

    [Fact]
    public void Obtain_offers_three_non_owned_races_and_consumes_the_pedestal()
    {
        var (playerEntity, room, pedestalCell) = Setup();

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 0);

        var raceNames = playerEntity.Player!.PendingChoice!.PendingChoiceOptions;
        Assert.Equal(PlayerChoiceSystem.AddAction, playerEntity.Player.PendingChoice.PendingChoiceAction);
        Assert.Equal(3, raceNames.Length);
        Assert.DoesNotContain(PlayerRaceData.Human.Name, raceNames);
        var presentation = playerEntity.Manager.PlayerChoiceSystem.GetPresentation(playerEntity)!;
        Assert.Equal("Choose a race to add", presentation.Title);
        Assert.Equal(
            [.. raceNames, PlayerChoiceSystem.DestroyAction],
            presentation.Options.Select(option => option.Identifier));
        Assert.All(
            presentation.Options.Take(raceNames.Length),
            option => Assert.Contains("<tooltip", option.Description));

        var selectedRace = raceNames[0];
        playerEntity.Player.LogEntries.Clear();
        SelectOption(playerEntity, optionIndex: 0);

        Assert.Null(playerEntity.Player.PendingChoice);
        Assert.Equal(2, playerEntity.Being!.Races.Count);
        Assert.Contains(
            playerEntity.Being.Races,
            race => (race.Ability!.Template?.Name ?? race.Ability.Name)
                    == playerEntity.Game.Services.Data.PlayerRaces.Get(selectedRace).GetRaceAbilityName());
        Assert.Equal(MapFeature.InactiveRacePedestal, GetTerrain(room, pedestalCell));
        Assert.True(room.FeatureTags.HasFlag(RoomFeatures.RacePedestal));
        Assert.Equal(
            [$"You gain the {RaceRef(selectedRace)} race."],
            playerEntity.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Destroy_card_grants_a_random_result_not_presented_as_a_card()
    {
        var (playerEntity, room, pedestalCell) = Setup();

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 0);
        var presentedRaceNames = playerEntity.Player!.PendingChoice!.PendingChoiceOptions;
        var presentation = playerEntity.Manager.PlayerChoiceSystem.GetPresentation(playerEntity)!;
        Assert.Equal(
            PlayerChoiceSystem.DestroyAction,
            presentation.Options[^1].Identifier);

        playerEntity.Player.LogEntries.Clear();
        SelectOption(playerEntity, optionIndex: presentedRaceNames.Length);

        var race = Assert.Single(playerEntity.Being!.Races);
        Assert.Equal(PlayerRaceData.Human.GetRaceAbilityName(), race.Ability!.Name);
        Assert.Equal(2, race.Ability.Level);
        Assert.DoesNotContain(PlayerRaceData.Human.Name, presentedRaceNames);
        Assert.Null(playerEntity.Player.PendingChoice);
        Assert.Equal(MapFeature.InactiveRacePedestal, GetTerrain(room, pedestalCell));
        Assert.Equal(
            [
                $"Your {RaceRef(PlayerRaceData.Human.Name)} race increases to level 2."
            ],
            playerEntity.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Obtain_filters_every_existing_race_and_allows_a_single_remaining_card()
    {
        var (playerEntity, _, _) = Setup();
        AddRace(playerEntity, PlayerRaceData.Dwarf);
        AddRace(playerEntity, PlayerRaceData.Elf);

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 0);

        Assert.Equal(
            [PlayerRaceData.Ogre.Name],
            playerEntity.Player!.PendingChoice!.PendingChoiceOptions);
    }

    [Fact]
    public void Obtain_allows_a_fifth_distinct_player_race()
    {
        var (playerEntity, _, _) = Setup();
        AddRace(playerEntity, PlayerRaceData.Dwarf);
        AddRace(playerEntity, PlayerRaceData.Elf);
        AddRace(playerEntity, PlayerRaceData.Ogre);
        var fifthRace = new PlayerRace
        {
            Name = "fifth",
            SpeciesClass = SpeciesClass.Humanoid
        };
        ((TestDataProvider)playerEntity.Game.Services.Data).PlayerRaces.Add(fifthRace);

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 0);

        Assert.Equal(
            [fifthRace.Name],
            playerEntity.Player!.PendingChoice!.PendingChoiceOptions);
        SelectOption(playerEntity, optionIndex: 0);
        Assert.Equal(5, playerEntity.Being!.Races.Count);
    }

    [Fact]
    public void Both_menu_options_are_disabled_when_every_race_is_owned()
    {
        var (playerEntity, room, pedestalCell) = Setup();
        AddRace(playerEntity, PlayerRaceData.Dwarf);
        AddRace(playerEntity, PlayerRaceData.Elf);
        AddRace(playerEntity, PlayerRaceData.Ogre);

        BumpPedestal(playerEntity);

        var presentation = playerEntity.Manager.PlayerChoiceSystem
            .GetPresentation(playerEntity)!;
        Assert.All(
            presentation.Options.Take(2),
            option => Assert.NotNull(option.DisabledReason));
        Assert.Null(presentation.Options[2].DisabledReason);

        SelectOption(playerEntity, optionIndex: 0);
        Assert.Null(playerEntity.Player!.PendingChoice!.PendingChoiceAction);
        SelectOption(playerEntity, optionIndex: 1);
        Assert.Null(playerEntity.Player.PendingChoice.PendingChoiceAction);

        SelectOption(playerEntity, optionIndex: 2);
        Assert.Null(playerEntity.Player.PendingChoice);
        Assert.Equal(
            MapFeature.InactiveRacePedestal,
            GetTerrain(room, pedestalCell));
    }

    [Fact]
    public void Replace_removes_the_selected_race_then_requires_an_addition()
    {
        var (playerEntity, room, pedestalCell) = Setup();

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 1);

        Assert.Equal(PlayerChoiceSystem.RemoveAction, playerEntity.Player!.PendingChoice!.PendingChoiceAction);
        Assert.Equal([PlayerRaceData.Human.Name], playerEntity.Player.PendingChoice.PendingChoiceOptions);

        playerEntity.Player.LogEntries.Clear();
        SelectOption(playerEntity, optionIndex: 0);

        Assert.Empty(playerEntity.Being!.Races);
        Assert.Equal(PlayerChoiceSystem.AddAction, playerEntity.Player.PendingChoice!.PendingChoiceAction);
        Assert.Contains(PlayerRaceData.Human.Name, playerEntity.Player.PendingChoice.PendingChoiceOptions);
        Assert.Equal(MapFeature.RacePedestal, GetTerrain(room, pedestalCell));
        Assert.Equal(
            [$"You lose the {RaceRef(PlayerRaceData.Human.Name)} race."],
            playerEntity.Player.LogEntries.Select(entry => entry.Message));

        var replacementRace = playerEntity.Player.PendingChoice.PendingChoiceOptions[0];
        playerEntity.Player.LogEntries.Clear();
        SelectOption(playerEntity, optionIndex: 0);

        Assert.Single(playerEntity.Being.Races);
        Assert.Null(playerEntity.Player.PendingChoice);
        Assert.Equal(MapFeature.InactiveRacePedestal, GetTerrain(room, pedestalCell));
        Assert.Equal(
            [$"You gain the {RaceRef(replacementRace)} race."],
            playerEntity.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Replace_offers_current_races_then_adds_a_non_owned_race()
    {
        var (playerEntity, room, pedestalCell) = Setup();
        AddRace(playerEntity, PlayerRaceData.Elf);

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 1);

        Assert.Equal(PlayerChoiceSystem.RemoveAction, playerEntity.Player!.PendingChoice!.PendingChoiceAction);
        Assert.Equal(
            [PlayerRaceData.Elf.Name, PlayerRaceData.Human.Name],
            playerEntity.Player.PendingChoice.PendingChoiceOptions);

        var elfIndex = Array.IndexOf(
            playerEntity.Player.PendingChoice.PendingChoiceOptions, PlayerRaceData.Elf.Name);
        SelectOption(playerEntity, elfIndex);

        Assert.Single(playerEntity.Being!.Races);
        Assert.Equal(PlayerChoiceSystem.AddAction, playerEntity.Player.PendingChoice!.PendingChoiceAction);
        Assert.Equal(MapFeature.RacePedestal, GetTerrain(room, pedestalCell));
        var replacementIndex = Array.FindIndex(
            playerEntity.Player.PendingChoice.PendingChoiceOptions,
            raceName => raceName != PlayerRaceData.Elf.Name);
        Assert.True(replacementIndex >= 0);
        var replacementName = playerEntity.Player.PendingChoice.PendingChoiceOptions[replacementIndex];

        SelectOption(playerEntity, replacementIndex);

        Assert.Equal(2, playerEntity.Being.Races.Count);
        Assert.Contains(
            playerEntity.Being.Races,
            race => (race.Ability!.Template?.Name ?? race.Ability.Name)
                    == playerEntity.Game.Services.Data.PlayerRaces
                        .Get(replacementName).GetRaceAbilityName());
        Assert.Equal(MapFeature.InactiveRacePedestal, GetTerrain(room, pedestalCell));
        Assert.True(room.FeatureTags.HasFlag(RoomFeatures.RacePedestal));
    }

    [Fact]
    public void Suppressed_races_are_available_as_replacement_choices()
    {
        var (playerEntity, room, pedestalCell) = Setup();
        AddRace(playerEntity, PlayerRaceData.Dwarf);
        AddRace(playerEntity, PlayerRaceData.Elf);
        var manager = playerEntity.Manager;
        manager.EffectApplicationSystem.ApplyPolymorph(
            CreatureData.Tiger, playerEntity, duration: 50, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Single(playerEntity.Being!.Races);

        playerEntity.Position!.Heading = Direction.East;
        Assert.True(TerrainSystem.TryInteract(
            playerEntity, pedestalCell, room, manager));

        var removeResult = manager.PlayerChoiceSystem.Select(
            playerEntity, optionIndex: 1, manager);
        Assert.True(removeResult.IsAccepted, removeResult.ErrorMessage);
        var removeOptions = playerEntity.Player!.PendingChoice!.PendingChoiceOptions;
        Assert.Equal(3, removeOptions.Length);
        Assert.DoesNotContain(CreatureData.Tiger.Name, removeOptions);

        var elfIndex = Array.IndexOf(removeOptions, PlayerRaceData.Elf.Name);
        var completion = manager.PlayerChoiceSystem.Select(
            playerEntity, elfIndex, manager);
        Assert.True(completion.IsAccepted, completion.ErrorMessage);
        Assert.Equal(PlayerChoiceSystem.AddAction, playerEntity.Player!.PendingChoice!.PendingChoiceAction);

        manager.Game.CurrentTick = 50;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.DoesNotContain(
            playerEntity.Being.Races,
            race => (race.Ability!.Template?.Name ?? race.Ability.Name)
                    == PlayerRaceData.Elf.GetRaceAbilityName());
    }

    [Fact]
    public void Remove_clears_every_grant_source_for_the_selected_race()
    {
        var (playerEntity, _, _) = Setup();
        AddRace(playerEntity, PlayerRaceData.Elf, sourceAbilityId: 100);
        AddRace(playerEntity, PlayerRaceData.Elf, sourceAbilityId: 200);

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 1);
        var elfIndex = Array.IndexOf(
            playerEntity.Player!.PendingChoice!.PendingChoiceOptions, PlayerRaceData.Elf.Name);
        SelectOption(playerEntity, elfIndex);

        Assert.DoesNotContain(
            playerEntity.ActiveEffects!.ComponentEffects,
            effect => effect is
            {
                EffectType: EffectType.AddAbility,
                TargetName: var targetName
            } && targetName == PlayerRaceData.Elf.GetRaceAbilityName());
    }

    [Fact]
    public void Effect_application_allows_a_fifth_distinct_player_race()
    {
        var (playerEntity, _, _) = Setup();
        AddRace(playerEntity, PlayerRaceData.Dwarf);
        AddRace(playerEntity, PlayerRaceData.Elf);
        AddRace(playerEntity, PlayerRaceData.Ogre);
        var fifthRace = new PlayerRace
        {
            Name = "fifth",
            SpeciesClass = SpeciesClass.Humanoid
        };
        ((TestDataProvider)playerEntity.Game.Services.Data).PlayerRaces.Add(fifthRace);

        playerEntity.Manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, fifthRace.GetRaceAbilityTemplate());
        playerEntity.Manager.Queue.ProcessQueue(playerEntity.Manager);
        Assert.Equal(5, playerEntity.Being!.Races.Count);

        playerEntity.Manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, PlayerRaceData.Human.GetRaceAbilityTemplate());
        Assert.Equal(5, playerEntity.Being.Races.Count);
    }

    [Fact]
    public void Selection_deactivates_the_pedestal_in_the_players_heading()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var eastPedestal = new Point(2, 1);
        var northPedestal = new Point(1, 0);
        TerrainSystem.SetTerrain(MapFeature.RacePedestal, eastPedestal, room);
        TerrainSystem.SetTerrain(MapFeature.RacePedestal, northPedestal, room);
        room.FeatureTags |= RoomFeatures.RacePedestal;
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(1, 1));
        playerEntity.Manager.Queue.ProcessQueue(playerEntity.Manager);
        playerEntity.Position!.Heading = Direction.East;
        Assert.True(TerrainSystem.TryInteract(
            playerEntity, eastPedestal, room, playerEntity.Manager));

        SelectOption(playerEntity, optionIndex: 0);
        SelectOption(playerEntity, optionIndex: 0);

        Assert.Equal(MapFeature.InactiveRacePedestal, GetTerrain(room, eastPedestal));
        Assert.Equal(MapFeature.RacePedestal, GetTerrain(room, northPedestal));
        Assert.True(room.FeatureTags.HasFlag(RoomFeatures.RacePedestal));
    }

    private static (GameEntity Player, RoomComponent Room, Point PedestalCell) Setup()
    {
        var room = TestHelper.BuildRoom("..");
        var pedestalCell = new Point(1, 0);
        TerrainSystem.SetTerrain(MapFeature.RacePedestal, pedestalCell, room);
        room.FeatureTags |= RoomFeatures.RacePedestal;
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(0, 0));
        player.Manager.Queue.ProcessQueue(player.Manager);
        return (player, room, pedestalCell);
    }

    private static void AddRace(
        GameEntity playerEntity, PlayerRace race, int sourceAbilityId = 0)
    {
        playerEntity.Manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, race.GetRaceAbilityTemplate(),
            sourceAbilityEntityId: sourceAbilityId);
        playerEntity.Manager.Queue.ProcessQueue(playerEntity.Manager);
    }

    private static void BumpPedestal(GameEntity playerEntity)
    {
        playerEntity.Player!.NextAction = ActorActionType.MoveOneCell;
        playerEntity.Player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, playerEntity.Manager);
    }

    private static void SelectOption(GameEntity playerEntity, int optionIndex)
    {
        playerEntity.Player!.NextAction = ActorActionType.SelectChoice;
        playerEntity.Player.NextActionTarget = optionIndex;
        playerEntity.Player.NextActionTarget2 = null;
        TestHelper.PerformTurn(playerEntity, playerEntity.Manager);
    }

    private static MapFeature GetTerrain(RoomComponent room, Point point)
        => room.Terrain[room.PointToIndex![point.X, point.Y]];

    private static string RaceRef(string raceName)
        => $"<tooltip id='{raceName}' category='{(int)DescriptionCategory.Race}'>{raceName}</tooltip>";
}
