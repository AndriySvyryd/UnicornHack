using UnicornHack.Systems.Abilities;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Features;
using UnicornHack.Data.Players;
using UnicornHack.Generation.Effects;
using UnicornHack.Services;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Actors;

public class PlayerChoiceSystemTest
{
    [Theory]
    [InlineData(RewardType.Ritual)]
    [InlineData(RewardType.Ability)]
    [InlineData(RewardType.Effect)]
    [InlineData(RewardType.Item)]
    [InlineData(RewardType.Race)]
    public void Destroy_is_offered_and_completed_even_when_no_reward_is_available(RewardType type)
    {
        var room = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var other = PlayerRace.InstantiatePlayer("Other", Sex.Male, room, new Point(2, 0));
        var manager = room.Entity.Manager;
        var feature = TestHelper.CreateRewardFeature(room, new Point(1, 0), "empty reward", [],
            rewardType: type, allowDestroy: true);
        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        var presentation = manager.PlayerChoiceSystem.GetPresentation(player)!;
        var destroyIndex = presentation.Options.ToList().FindIndex(option => option.Identifier == PlayerChoiceSystem.DestroyAction);
        Assert.True(destroyIndex >= 0);
        Assert.True(char.IsUpper(presentation.Title[0]));
        Assert.Equal($"Destroy the {feature.Name}", presentation.Options[destroyIndex].Title);
        Assert.Equal($"Destroy the {feature.Name} for a random result.", presentation.Options[destroyIndex].Description);
        Assert.True(manager.PlayerChoiceSystem.Select(player, destroyIndex, manager).IsAccepted);
        Assert.True(feature.Destroyed);
        Assert.Null(player.Player!.PendingChoice);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, player));
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, other));
        Assert.Contains(player.Player.LogEntries, entry => entry.Message == "Nothing seems to have happened.");
        Assert.Contains(other.Player!.LogEntries, entry => entry.Message == "Nothing seems to have happened.");
        Assert.Equal(0, room.Game.CurrentTick);
    }

    [Theory]
    [InlineData(RewardType.Ritual)]
    [InlineData(RewardType.Ability)]
    [InlineData(RewardType.Effect)]
    [InlineData(RewardType.Item)]
    [InlineData(RewardType.Race)]
    public void Runtime_player_options_are_granted_on_selection_without_a_feature(RewardType type)
    {
        var room = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var provider = (TestDataProvider)room.Game.Services.Data;
        const string name = "test runtime option";
        if (type == RewardType.Item)
        {
            provider.Items.Add(new Item { Name = name, Type = ItemType.Trinket });
        }
        else if (type == RewardType.Race)
        {
            provider.PlayerRaces.Add(new PlayerRace { Name = name });
        }
        else
        {
            provider.AddAbilities(new Ability
            {
                Name = name,
                Type = type == RewardType.Ritual ? AbilityType.Boon
                    : type == RewardType.Ability ? AbilityType.Feat : AbilityType.Default,
                Effects = type == RewardType.Effect ? [new Heal { Amount = "1" }]
                    : [new ChangeProperty<int> { TargetActivator = true, PropertyName = "Might", Amount = "1" }]
            });
        }

        if (type == RewardType.Effect)
        {
            player.Being!.HitPoints--;
        }

        player.Player!.PendingChoice = new PlayerOptions
        {
            ChoiceType = type,
            PendingChoiceOptions = [name]
        };
        var presentation = manager.PlayerChoiceSystem.GetPresentation(player)!;
        Assert.Equal("Choose a reward", presentation.Title);
        Assert.Equal(name, Assert.Single(presentation.Options).Identifier);
        Assert.Equal("Test runtime option", presentation.Options[0].Title);
        Assert.DoesNotContain(presentation.Options, option => option.Identifier == PlayerChoiceSystem.DestroyAction);
        Assert.False(manager.PlayerChoiceSystem.Select(player, -1, manager).IsAccepted);
        Assert.Null(player.Player.PendingChoice!.SourceFeatureId);
        var result = manager.PlayerChoiceSystem.Select(player, 0, manager);
        Assert.True(result.IsAccepted);
        Assert.Equal(0, result.Delay);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(player.Player.PendingChoice);
        Assert.Empty(room.Features);
        Assert.Equal(0, room.Game.CurrentTick);
        switch (type)
        {
            case RewardType.Ritual:
            case RewardType.Ability:
                Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, name));
                break;
            case RewardType.Effect:
                Assert.Equal(player.Being!.HitPointMaximum, player.Being.HitPoints);
                break;
            case RewardType.Item:
                Assert.True(player.Being!.Items.Any(item => item.Item!.TemplateName == name)
                    || room.Items.Values.Any(item => item.Item!.TemplateName == name));
                break;
            case RewardType.Race:
                Assert.Contains(provider.PlayerRaces.Get(name).GetRaceAbilityName(),
                    AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
                break;
        }
    }

    public class SingleCandidatePlayerChoiceSystem(string name) : PlayerChoiceSystem
    {
        public override IEnumerable<(string Name, float Weight)> GetRewardOptions(Feature definition, RoomComponent room)
            => base.GetRewardOptions(definition, room).Where(option => option.Name == name);
    }

    [Fact]
    public void Player_choice_system_can_override_reward_selection()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        room.Entity.Manager.PlayerChoiceSystem = new SingleRewardPlayerChoiceSystem();
        var entity = FeatureGenerator.Place(room, new Point(1, 1), RoomFeatures.SmallFountain);
        Assert.Same(FeatureData.SmallFountain, entity.Feature!.Template);
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 1));
        room.Entity.Manager.Queue.ProcessQueue(room.Entity.Manager);
        Assert.True(room.Entity.Manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 1)));

        Assert.Equal([UnicornHack.Data.Abilities.AbilityData.Insight.Name],
            room.Entity.Manager.PlayerChoiceSystem.GetRewardOptions(room.Game.Services.Data.Features.Get(entity.Feature.Name), room)
                .Select(option => option.Name));
        Assert.Equal(entity.Id, player.Player!.PendingChoice!.SourceFeatureId);
    }

    public class SingleRewardPlayerChoiceSystem : UnicornHack.Systems.Actors.PlayerChoiceSystem
    {
        public override IEnumerable<(string Name, float Weight)> GetRewardOptions(Feature definition, RoomComponent room)
            => [(UnicornHack.Data.Abilities.AbilityData.Insight.Name, 1f)];

        public override float GetRewardWeight(Feature definition, RoomComponent room, string name) => 1f;
    }

    [Fact]
    public void Feature_use_bits_follow_player_id_order_for_eight_players()
    {
        var room = TestHelper.BuildRoom(".........");
        var players = Enumerable.Range(0, 8)
            .Select(index => PlayerRace.InstantiatePlayer($"Player{index}", Sex.Male, room, new Point((byte)index, 0)))
            .ToArray();
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var feature = new Feature { Name = "test player mask" }.Instantiate(room, new Point(8, 0)).Feature!;

        Assert.Equal(new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 }, players.Select(PlayerChoiceSystem.GetPlayerUseMask));
        foreach (var player in players)
        {
            Assert.False(PlayerChoiceSystem.HasUsedFeature(feature, player));
            feature.UsedByPlayers |= PlayerChoiceSystem.GetPlayerUseMask(player);
            Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, player));
        }
        Assert.Equal(byte.MaxValue, feature.UsedByPlayers);
    }

    [Theory]
    [InlineData(RoomFeatures.MinorShrine, EffectDuration.UntilNextRoom)]
    [InlineData(RoomFeatures.MajorShrine, EffectDuration.UntilNextLevel)]
    public void Shrine_reward_templates_own_the_granted_boons_expiry(RoomFeatures kind, EffectDuration duration)
    {
        var room = TestHelper.BuildRoom(@"...
...
...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        FeatureGenerator.Place(room, new Point(1, 0), kind);
        manager.Queue.ProcessQueue(manager);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        var template = room.Game.Services.Data.Abilities.Get(player.Player!.PendingChoice!.PendingChoiceOptions[0]);
        Assert.NotNull(template.Effects);
        var grant = Assert.IsType<AddAbility>(Assert.Single(template.Effects));
        Assert.Equal(duration, grant.Duration);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, grant.Name));

        if (duration != EffectDuration.UntilNextRoom)
        {
            manager.EffectApplicationSystem.ExpireTransitionEffects(player, false, manager);
            manager.Queue.ProcessQueue(manager);
            Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, grant.Name));
        }
        manager.EffectApplicationSystem.ExpireTransitionEffects(player, duration != EffectDuration.UntilNextRoom, manager,
            branchChanged: duration == EffectDuration.UntilNextBranch);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(player, grant.Name));
    }

    [Theory]
    [InlineData(RewardType.Ritual)]
    [InlineData(RewardType.Ability)]
    [InlineData(RewardType.Effect)]
    [InlineData(RewardType.Item)]
    [InlineData(RewardType.Race)]
    public void Generated_offers_and_destroy_results_use_the_corresponding_template_weights(RewardType type)
    {
        var room = TestHelper.BuildRoom(@"...
...
...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var provider = (TestDataProvider)room.Game.Services.Data;
        var names = Enumerable.Range(0, 5).Select(index => $"weighted reward {index}").ToArray();
        for (var index = 0; index < names.Length; index++)
        {
            var weight = index < 4 ? "$feature == 'test weighted feature' ? Infinity : 0.0" : "0";
            if (type == RewardType.Item)
            {
                provider.Items.Add(new Item { Name = names[index], Type = ItemType.Trinket, GenerationWeight = weight });
            }
            else if (type is RewardType.Race or RewardType.Race)
            {
                provider.PlayerRaces.Add(new PlayerRace { Name = names[index], GenerationWeight = weight });
            }
            else
            {
                provider.AddAbilities(new Ability
                {
                    Name = names[index], GenerationWeight = weight,
                    Type = type == RewardType.Ritual ? AbilityType.Boon : type == RewardType.Ability ? AbilityType.Feat : AbilityType.Default,
                    Effects = type == RewardType.Effect ? [new Heal { Amount = "1" }]
                        : [new ChangeProperty<int> { TargetActivator = true, PropertyName = "Might", Amount = "1" }]
                });
            }
        }

        var definition = new Feature
        {
            Name = "test weighted feature", RewardType = type,
            AbilityRewardType = type == RewardType.Ability ? AbilityType.Feat
                : type == RewardType.Ritual ? AbilityType.Boon : AbilityType.Default
        };
        provider.Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(1, 0));
        Assert.Same(definition, feature.Feature!.Template);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.DoesNotContain(names[4], player.Player!.PendingChoice!.PendingChoiceOptions);
        if (type == RewardType.Race)
        {
            Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        }

        Assert.Equal(names[..3], player.Player!.PendingChoice!.PendingChoiceOptions);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 3, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature.Feature, player));
        Assert.Contains(player.Player.LogEntries, entry => entry.Message.Contains(names[3], StringComparison.Ordinal)
            && entry.Message.Contains(feature.Feature.Name, StringComparison.Ordinal));
        Assert.True(feature.Feature.Destroyed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reward_items_fall_back_to_the_ground_and_log_only_when_inventory_is_full(bool inventoryFull)
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var dagger = room.Game.Services.Data.Items.Get("dagger");
        while (inventoryFull && manager.AbilitySlottingSystem.GetFirstFreeSlot(player) != null)
        {
            Assert.InRange(player.Being!.Items.Count, 0, 30);
            Assert.True(dagger.Instantiate(player));
            manager.Queue.ProcessQueue(manager);
        }
        var inventoryCount = player.Being!.Items.Count;
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test item", [dagger.Name], rewardType: RewardType.Item);

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(inventoryCount + (inventoryFull ? 0 : 1), player.Being.Items.Count);
        if (inventoryFull)
        {
            Assert.Equal(dagger.Name, Assert.Single(room.Items).Value.Item!.TemplateName);
        }
        else
        {
            Assert.Empty(room.Items);
            Assert.Contains(player.Being.Items, item => item.Item!.TemplateName == dagger.Name);
        }
        Assert.Equal(inventoryFull ? 1 : 0, player.Player!.LogEntries.Count(
            entry => entry.Message == "Your inventory is full, so you drop the dagger at your feet."));
        Assert.Null(player.Player!.PendingChoice);
        Assert.Equal(0, room.Game.CurrentTick);
    }

    [Fact]
    public void Permanent_boons_can_level_up_until_their_maximum_level()
    {
        var room = TestHelper.BuildRoom(@"...
...
...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(1, 1));
        var manager = room.Entity.Manager;
        var boon = new LeveledAbility
        {
            Name = "test permanent boon", Type = AbilityType.Boon,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { [1] = [], [2] = [] }
        };
        ((TestDataProvider)room.Game.Services.Data).AddAbilities(boon);
        manager.Queue.ProcessQueue(manager);
        foreach (var cell in new[] { new Point(0, 1), new Point(2, 1), new Point(1, 2) })
        {
            TestHelper.CreateRewardFeature(room, cell, "Test boon", [boon.Name]);
        }

        foreach (var cell in new[] { new Point(0, 1), new Point(2, 1) })
        {
            Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, cell));
            Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(player, boon.Name));
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 2)));
    }

    [Theory]
    [InlineData(EffectDuration.UntilTimeout)]
    [InlineData(EffectDuration.UntilXPGained)]
    [InlineData(EffectDuration.UntilNextRoom)]
    [InlineData(EffectDuration.UntilNextLevel)]
    [InlineData(EffectDuration.UntilNextBranch)]
    public void Active_temporary_boons_are_not_offered_or_overwritten(EffectDuration duration)
    {
        var room = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.AbilityLevelSystem.AddAbilityEffect(player, AbilityData.PatientVigor,
            expiration: new EffectExpiration(duration, 300));
        manager.Queue.ProcessQueue(manager);
        var existing = Assert.Single(player.ActiveEffects!.ComponentEffects.GetValues(AbilityData.PatientVigor.Name));
        var originalTarget = existing.ExpirationTarget;
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward",
            [AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name]);

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.Equal([AbilityData.FurrowsReserve.Name], player.Player!.PendingChoice!.PendingChoiceOptions);
        Assert.Equal(duration, existing.ExpirationEvent);
        Assert.Equal(originalTarget, existing.ExpirationTarget);
    }

    [Fact]
    public void Selecting_a_reward_completes_the_feature_without_a_destroy_confirmation()
    {
        var room = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var initialEp = player.Being!.EnergyPointMaximum;
        var initialHp = player.Being.HitPointMaximum;
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward",
            [AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name], allowDestroy: true);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.Equal([AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name], player.Player!.PendingChoice!.PendingChoiceOptions);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        Assert.Null(player.Player.PendingChoice);
        Assert.Null(manager.PlayerChoiceSystem.GetPresentation(player));
        Assert.False(manager.PlayerChoiceSystem.Select(player, 1, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(initialEp + 10, player.Being.EnergyPointMaximum);
        Assert.Equal(initialHp, player.Being.HitPointMaximum);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, AbilityData.PatientVigor.Name));
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(player, AbilityData.FurrowsReserve.Name));
        Assert.False(room.Features[new Point(1, 0)].Feature!.Destroyed);
    }

    [Fact]
    public void Destruction_chooses_from_the_current_pool_without_previewing_the_result()
    {
        var room = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var reward = TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward",
            [AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name], allowDestroy: true);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.Equal([AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name], player.Player!.PendingChoice!.PendingChoiceOptions);
        Assert.Equal([AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name, PlayerChoiceSystem.DestroyAction],
            manager.PlayerChoiceSystem.GetPresentation(player)!.Options.Select(option => option.Identifier));
        const string replacementName = "Hive-sense";
        var generator = Assert.IsType<TestHelper.RewardTestPlayerChoiceSystem>(room.Entity.Manager.PlayerChoiceSystem);
        generator.Candidates[room.Features[new Point(1, 0)].Feature!.Name] = [AbilityData.PatientVigor.Name, replacementName];
        var initialRange = player.Being!.RoomPerceptionRange;

        Assert.True(manager.PlayerChoiceSystem.Select(player, player.Player.PendingChoice.PendingChoiceOptions.Count, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, replacementName));
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(player, AbilityData.FurrowsReserve.Name));
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(player, AbilityData.PatientVigor.Name));
        Assert.Equal(initialRange + 1, player.Being.RoomPerceptionRange);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Destroying_during_race_replacement_preserves_existing_races(bool selectNewRace)
    {
        var (player, room, cell) = Setup();
        BumpPedestal(player);
        SelectOption(player, 1);
        if (selectNewRace)
        {
            SelectOption(player, 0);
        }
        var destroyIndex = player.Player!.PendingChoice!.PendingChoiceOptions.Count;
        SelectOption(player, destroyIndex);

        Assert.True(player.Being!.IsAlive);
        Assert.Contains(PlayerRaceData.Human.GetRaceAbilityName(), AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
        Assert.True(room.Features[cell].Feature!.Destroyed);
        Assert.Null(player.Player.PendingChoice);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(room.Features[cell].Feature!, player));
    }

    [Theory]
    [InlineData(RoomFeatures.SmallFountain)]
    [InlineData(RoomFeatures.BigFountain)]
    [InlineData(RoomFeatures.RacePedestal)]
    [InlineData(RoomFeatures.SkillPedestal)]
    [InlineData(RoomFeatures.TraitPedestal)]
    [InlineData(RoomFeatures.FeatPedestal)]
    [InlineData(RoomFeatures.MinorShrine)]
    [InlineData(RoomFeatures.MajorShrine)]
    [InlineData(RoomFeatures.RitualShrine)]
    [InlineData(RoomFeatures.ArmorStand)]
    [InlineData(RoomFeatures.GildedArmorStand)]
    [InlineData(RoomFeatures.WeaponStand)]
    [InlineData(RoomFeatures.GildedWeaponStand)]
    [InlineData(RoomFeatures.ItemCase)]
    [InlineData(RoomFeatures.GildedItemCase)]
    public void Every_generated_reward_kind_has_a_resolvable_destroy_option(RoomFeatures kind)
    {
        var room = TestHelper.BuildRoom(@"...
...
...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var feature = FeatureGenerator.Place(room, new Point(1, 0), kind);
        var definition = room.Game.Services.Data.Features.GetAll().Single(template => template.RoomFeature == kind);
        Assert.Same(definition, feature.Feature!.Template);
        Assert.Empty(definition.Abilities);
        var names = room.Entity.Manager.PlayerChoiceSystem.GetRewardOptions(definition, room).Select(option => option.Name).ToArray();
        foreach (var excluded in new[] { "Furrow's Reserve", "Hive-sense", "Patient Vigor", "brief vigor", "earned reserve" })
        {
            Assert.DoesNotContain(excluded, names);
        }
        foreach (var boon in new[] { AbilityData.BloodPact, AbilityData.KeenBargain, AbilityData.SwiftSacrifice, AbilityData.FocusedOffering })
        {
            Assert.Equal(kind == RoomFeatures.RitualShrine, names.Contains(boon.Name));
            Assert.Equal(kind == RoomFeatures.RitualShrine ? 1f : 0f, boon.GetWeight(room, feature.Feature.Name));
            Assert.Equal(0f, boon.GetWeight(room));
        }
        Assert.NotEqual(feature.Feature!.Name,
            room.Game.Services.Language.GetDescription(feature.Feature.Name, DescriptionCategory.Feature));
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        var options = manager.PlayerChoiceSystem.GetPresentation(player)!.Options;
        Assert.All(options, option => Assert.False(string.IsNullOrWhiteSpace(option.Description)));
        var destroyIndex = options.ToList().FindIndex(option => option.Identifier == PlayerChoiceSystem.DestroyAction);
        Assert.True(destroyIndex >= 0);
        var result = manager.PlayerChoiceSystem.Select(player, destroyIndex, manager);
        Assert.True(result.IsAccepted, result.ErrorMessage);
        manager.Queue.ProcessQueue(manager);
        Assert.True(feature.Feature!.Destroyed);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature.Feature, player));
        Assert.Equal(0, room.Game.CurrentTick);
    }

    [Fact]
    public void Fountain_experience_rewards_are_granted_to_all_players()
    {
        var room = TestHelper.BuildRoom("...");
        var first = PlayerRace.InstantiatePlayer("First", Sex.Male, room, new Point(0, 0));
        var second = PlayerRace.InstantiatePlayer("Second", Sex.Male, room, new Point(2, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var firstXp = first.Being!.ExperiencePoints;
        var secondXp = second.Being!.ExperiencePoints;
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Insight", [AbilityData.Insight.Name],
            rewardType: RewardType.Effect);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(first, room, new Point(1, 0)));
        Assert.True(manager.PlayerChoiceSystem.Select(first, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(firstXp + 20, first.Being.ExperiencePoints);
        Assert.Equal(secondXp + 20, second.Being.ExperiencePoints);
    }

    [Theory]
    [InlineData(EffectDuration.UntilTimeout)]
    [InlineData(EffectDuration.UntilXPGained)]
    [InlineData(EffectDuration.UntilNextRoom)]
    [InlineData(EffectDuration.UntilNextLevel)]
    [InlineData(EffectDuration.UntilNextBranch)]
    public void Authored_boon_expiry_removes_the_grant_at_the_exact_boundary(EffectDuration duration)
    {
        var room = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var initialEp = player.Being!.EnergyPointMaximum;
        var rewardTemplate = new Ability
        {
            Name = "test chosen timed reward", Type = AbilityType.Buff, Activation = ActivationType.Default,
            Effects = [new AddAbility
            {
                Name = AbilityData.PatientVigor.Name, Duration = duration,
                DurationAmount = duration is EffectDuration.UntilTimeout or EffectDuration.UntilXPGained ? "10" : null
            }]
        };
        ((TestDataProvider)room.Game.Services.Data).AddAbilities(rewardTemplate);
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test boon", [rewardTemplate.Name],
            rewardType: RewardType.Effect);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(initialEp + 10, player.Being.EnergyPointMaximum);

        if (duration == EffectDuration.UntilTimeout)
        {
            Assert.Equal(10, player.ActiveEffects!.NextEffectTick);
            manager.Game.CurrentTick = 9;
            manager.EffectApplicationSystem.ExpireTimedPropertyEffects(player, manager);
            Assert.Equal(initialEp + 10, player.Being.EnergyPointMaximum);
            manager.Game.CurrentTick = 10;
            manager.EffectApplicationSystem.ExpireTimedPropertyEffects(player, manager);
        }
        else if (duration == EffectDuration.UntilXPGained)
        {
            GainExperience(9);
            Assert.Equal(initialEp + 10, player.Being.EnergyPointMaximum);
            GainExperience(1);
        }
        else
        {
            if (duration != EffectDuration.UntilNextRoom)
            {
                manager.EffectApplicationSystem.ExpireTransitionEffects(player, levelChanged: false, manager);
                Assert.Equal(initialEp + 10, player.Being.EnergyPointMaximum);
            }
            if (duration == EffectDuration.UntilNextBranch)
            {
                manager.EffectApplicationSystem.ExpireTransitionEffects(player, levelChanged: true, manager);
                Assert.Equal(initialEp + 10, player.Being.EnergyPointMaximum);
            }
            manager.EffectApplicationSystem.ExpireTransitionEffects(player,
                levelChanged: duration != EffectDuration.UntilNextRoom, manager,
                branchChanged: duration == EffectDuration.UntilNextBranch);
        }
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(initialEp, player.Being.EnergyPointMaximum);

        void GainExperience(int amount)
        {
            var ability = new Ability { Name = $"test XP {amount}", Effects = [new GainXP { Amount = amount.ToString() }] }
                .AddToAffectable(player);
            var activate = ActivateAbilityMessage.Create(manager);
            activate.AbilityEntity = ability.Entity;
            activate.ActivatorEntity = player;
            activate.TargetEntity = player;
            manager.Process(activate);
            manager.Queue.ProcessQueue(manager);
        }
    }

    [Theory]
    [InlineData(RoomFeatures.SmallFountain, false)]
    [InlineData(RoomFeatures.BigFountain, true)]
    public void Curse_removal_reward_eligibility_uses_its_generation_weight(RoomFeatures kind, bool offered)
    {
        var room = TestHelper.BuildRoom(@"...
...
...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.AbilityLevelSystem.AddAbilityEffect(player,
            new Ability { Name = "test removable curse", Type = AbilityType.Curse, Effects = [] });
        manager.Queue.ProcessQueue(manager);
        var feature = FeatureGenerator.Place(room, new Point(1, 0), kind);
        var definition = feature.Feature!.Template!;
        feature.Feature.Destroyed = null;
        var generator = new SingleCandidatePlayerChoiceSystem(AbilityData.LiftMajorCurse.Name);
        room.Entity.Manager.PlayerChoiceSystem = generator;

        Assert.Equal(offered, generator.GetRewardOptions(definition, room).Any(option => option.Name == AbilityData.LiftMajorCurse.Name));
        Assert.Equal(offered ? 1f : 0f, AbilityData.LiftMajorCurse.GetWeight(room, feature.Feature.Name));
        Assert.Equal(offered, manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        if (!offered)
        {
            return;
        }

        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Empty(AbilityLevelSystem.GetRemovableAbilities(player, AbilityType.Curse));
    }

    [Fact]
    public void Entity_pedestal_replacement_keeps_the_old_race_until_the_final_choice()
    {
        var room = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var definition = new Feature { Name = "test pedestal", RewardType = RewardType.Race };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        definition.Instantiate(room, new Point(1, 0));
        var originalRaces = AbilityLevelSystem.GetPlayerRaceAbilityNames(player).ToArray();

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        var sourceId = player.Player!.PendingChoice!.SourceFeatureId;
        Assert.True(manager.PlayerChoiceSystem.Select(player, 1, manager).IsAccepted);
        Assert.Equal(PlayerChoiceSystem.ReplaceAction, player.Player.PendingChoice!.PendingChoiceAction);
        var newRace = player.Player.PendingChoice.PendingChoiceOptions[0];
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        Assert.Equal(originalRaces, AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
        Assert.Equal(sourceId, player.Player.PendingChoice!.SourceFeatureId);
        Assert.Equal(PlayerChoiceSystem.RemoveAction, player.Player.PendingChoice.PendingChoiceAction);
        Assert.Equal(newRace, player.Player.PendingChoice.SelectedOption);
        Assert.DoesNotContain(newRace, player.Player.PendingChoice.PendingChoiceOptions);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.True(player.Being!.IsAlive);
        Assert.Equal([manager.Game.Services.Data.PlayerRaces.Get(newRace).GetRaceAbilityName()],
            AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
        Assert.True(PlayerChoiceSystem.HasUsedFeature(room.Features[new Point(1, 0)].Feature!, player));
        Assert.Equal(0, room.Game.CurrentTick);
    }

    [Fact]
    public void Removing_a_feature_prevents_presenting_its_rewards()
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward", ["Patient Vigor"]);
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
        var reward = TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward", ["Patient Vigor"]);
        var feature = room.Features[new Point(1, 0)];
        feature.Position!.RoomEntity = otherRoom.Entity;
        manager.Queue.ProcessQueue(manager);

        Assert.Same(reward, feature.Feature);
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, otherRoom, new Point(1, 0)));
        Assert.Null(player.Player!.PendingChoice);
        Assert.Equal(0, feature.Feature!.UsedByPlayers);
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
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test races",
            hasAvailableRace ? [PlayerRaceData.Human.Name, PlayerRaceData.Elf.Name] : [PlayerRaceData.Human.Name],
            rewardType: RewardType.Race);

        Assert.Equal(hasAvailableRace, manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        if (!hasAvailableRace)
        {
            Assert.Null(player.Player!.PendingChoice);
            return;
        }

        Assert.Equal(RewardType.Race, manager.PlayerChoiceSystem.GetPresentation(player)!.Choice);
        Assert.DoesNotContain(manager.PlayerChoiceSystem.GetPresentation(player)!.Options,
            option => option.Identifier == PlayerChoiceSystem.DestroyAction);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        Assert.Equal([PlayerRaceData.Elf.Name], player.Player!.PendingChoice!.PendingChoiceOptions);
        Assert.False(manager.PlayerChoiceSystem.Select(player, 1, manager).IsAccepted);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Contains(PlayerRaceData.Elf.GetRaceAbilityName(), AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
    }

    [Fact]
    public void Race_replacement_never_offers_the_selected_new_race_for_removal_even_if_it_is_already_owned()
    {
        var (player, _, _) = Setup();
        BumpPedestal(player);
        SelectOption(player, 1);
        var newRace = player.Game.Services.Data.PlayerRaces.Get(player.Player!.PendingChoice!.PendingChoiceOptions[0]);
        AddRace(player, newRace);

        SelectOption(player, 0);

        Assert.Equal(PlayerChoiceSystem.RemoveAction, player.Player.PendingChoice!.PendingChoiceAction);
        Assert.Equal(newRace.Name, player.Player.PendingChoice.SelectedOption);
        Assert.DoesNotContain(newRace.Name, player.Player.PendingChoice.PendingChoiceOptions);
        Assert.Contains(PlayerRaceData.Human.Name, player.Player.PendingChoice.PendingChoiceOptions);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Item_rewards_require_a_valid_destination_before_presentation(bool hasFloorDestination)
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test item", ["dagger"], rewardType: RewardType.Item);
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
        manager.Queue.ProcessQueue(manager);

        Assert.Equal("dagger", Assert.Single(room.Items).Value.Item!.TemplateName);
        Assert.Empty(player.Being.Items);
    }

    [Theory]
    [InlineData(RewardType.Item)]
    [InlineData(RewardType.Race)]
    public void Claiming_an_item_or_race_resolves_only_the_players_use(RewardType rewardType)
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        var name = rewardType == RewardType.Item
            ? "dagger"
            : player.Game.Services.Data.PlayerRaces.GetAll()
                .First(race => race.GetRaceAbilityName() != PlayerRaceData.Human.GetRaceAbilityName()).Name;
        var reward = TestHelper.CreateRewardFeature(
            room, new Point(1, 0), "Test reward", [name], rewardType: rewardType);
        var feature = room.Features[new Point(1, 0)].Feature!;
        Assert.Same(reward, feature);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.Equal(rewardType, player.Player!.PendingChoice!.ChoiceType);
        Assert.Equal(rewardType, manager.PlayerChoiceSystem.GetPresentation(player)!.Choice);
        Assert.Equal(rewardType == RewardType.Race ? "Your selection is permanent." : null,
            manager.PlayerChoiceSystem.GetPresentation(player)!.Notice);
        Assert.Empty(player.Being!.Items);
        Assert.Empty(room.Items);
        Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        if (rewardType == RewardType.Race)
        {
            Assert.Equal(PlayerChoiceSystem.AddAction, player.Player.PendingChoice!.PendingChoiceAction);
            Assert.Equal(rewardType, player.Player.PendingChoice.ChoiceType);
            Assert.True(manager.PlayerChoiceSystem.Select(player, 0, manager).IsAccepted);
        }
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

        Assert.Equal(rewardType, feature.Template!.RewardType);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, player));
        Assert.Null(player.Player.PendingChoice);
        Assert.Equal(MapFeature.StoneFloor, room.Terrain[room.PointToIndex![1, 0]]);
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
    }

    [Theory]
    [InlineData("Furrow's Reserve", 10, 0, 0)]
    [InlineData("Patient Vigor", 0, 10, 0)]
    [InlineData("Hive-Sense", 0, 0, 1)]
    public void Reward_preview_is_free_and_one_selection_grants_a_branch_bound_boon(
        string boonName, int hitPoints, int energyPoints, int roomPerceptionRange)
    {
        var room = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        var rewardTemplate = new Ability
        {
            Name = "test chosen branch reward", Type = AbilityType.Buff, Activation = ActivationType.Default,
            Effects = [new AddAbility { Name = boonName, Duration = EffectDuration.UntilNextBranch }]
        };
        ((TestDataProvider)room.Game.Services.Data).AddAbilities(rewardTemplate);
        var reward = TestHelper.CreateRewardFeature(room, new Point(1, 0), "Produce", [rewardTemplate.Name],
            rewardType: RewardType.Effect);
        var feature = Assert.Single(room.Features).Value;
        Assert.Same(reward, feature.Feature);
        Assert.Equal(feature.Id, reward.EntityId);
        Assert.Null(feature.Being);
        Assert.Null(feature.Item);
        Assert.Equal(new Point(1, 0), feature.Position!.Cell);
        var initialHp = player.Being!.HitPointMaximum;
        var initialEp = player.Being.EnergyPointMaximum;
        var initialPerception = player.Being.Perception;
        var initialRoomPerceptionRange = player.Being.RoomPerceptionRange;

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        Assert.False(PlayerChoiceSystem.HasUsedFeature(feature.Feature!, player));
        Assert.Equal(0, player.Game.CurrentTick);
        var result = manager.PlayerChoiceSystem.Select(player, 0, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(result.IsAccepted);
        Assert.Equal(0, result.Delay);
        Assert.Same(reward.Template, feature.Feature!.Template);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature.Feature, player));
        Assert.Equal(MapFeature.StoneFloor, room.Terrain[room.PointToIndex![1, 0]]);
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
    public void Each_player_can_claim_a_reward_once_without_advancing_time()
    {
        var room = TestHelper.BuildRoom("...");
        var first = PlayerRace.InstantiatePlayer("First", Sex.Male, room, new Point(0, 0));
        var second = PlayerRace.InstantiatePlayer("Second", Sex.Male, room, new Point(2, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward", [AbilityData.PatientVigor.Name]);

        foreach (var player in new[] { first, second })
        {
            var initialEnergy = player.Being!.EnergyPointMaximum;
            Assert.True(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
            var result = manager.PlayerChoiceSystem.Select(player, 0, manager);
            Assert.True(result.IsAccepted, result.ErrorMessage);
            Assert.Equal(0, result.Delay);
            manager.Queue.ProcessQueue(manager);
            Assert.Equal(initialEnergy + 10, player.Being.EnergyPointMaximum);
            Assert.False(manager.PlayerChoiceSystem.TryPresentReward(player, room, new Point(1, 0)));
        }

        Assert.Equal(3, room.Features[new Point(1, 0)].Feature!.UsedByPlayers);
        Assert.Equal(0, room.Game.CurrentTick);
    }

    [Fact]
    public void Destroy_rewards_are_personal_and_immediate_for_all_unused_players()
    {
        var room = TestHelper.BuildRoom(@"...
...");
        var first = PlayerRace.InstantiatePlayer("First", Sex.Male, room, new Point(0, 0));
        var second = PlayerRace.InstantiatePlayer("Second", Sex.Male, room, new Point(2, 0));
        var third = PlayerRace.InstantiatePlayer("Third", Sex.Male, room, new Point(1, 1));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward",
            [AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name], allowDestroy: true);
        var firstHp = first.Being!.HitPointMaximum;
        var secondHp = second.Being!.HitPointMaximum;
        var thirdHp = third.Being!.HitPointMaximum;
        var thirdEp = third.Being.EnergyPointMaximum;

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(first, room, new Point(1, 0)));
        Assert.True(manager.PlayerChoiceSystem.Select(first, 0, manager).IsAccepted);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(second, room, new Point(1, 0)));
        Assert.True(manager.PlayerChoiceSystem.Select(second, second.Player!.PendingChoice!.PendingChoiceOptions.Count, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.True(room.Features[new Point(1, 0)].Feature!.Destroyed);
        Assert.Equal(firstHp, first.Being.HitPointMaximum);
        Assert.Equal(secondHp, second.Being!.HitPointMaximum);
        Assert.Contains(second.Player.LogEntries, entry => entry.Message == "Nothing seems to have happened.");
        var thirdReward = Assert.Single(new[] { AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name },
            name => AbilityLevelSystem.GetAbilityLevel(third, name) > 0);
        Assert.Equal(thirdHp + (thirdReward == AbilityData.FurrowsReserve.Name ? 10 : 0), third.Being.HitPointMaximum);
        Assert.Equal(thirdEp + (thirdReward == AbilityData.PatientVigor.Name ? 10 : 0), third.Being.EnergyPointMaximum);
        Assert.Equal(7, room.Features[new Point(1, 0)].Feature!.UsedByPlayers);
        Assert.Null(second.Player!.PendingChoice);
        Assert.Null(third.Player!.PendingChoice);
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(first, room, new Point(1, 0)));
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(second, room, new Point(1, 0)));
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(third, room, new Point(1, 0)));
        Assert.Equal(0, room.Game.CurrentTick);
    }

    [Theory]
    [InlineData(RewardType.Ritual)]
    [InlineData(RewardType.Ability)]
    [InlineData(RewardType.Effect)]
    [InlineData(RewardType.Item)]
    [InlineData(RewardType.Race)]
    public void Destruction_awards_remote_players_using_the_features_generation_context(RewardType type)
    {
        var room = TestHelper.BuildRoom("...", depth: 2);
        var otherRoom = TestHelper.BuildRoom("...", room.Game, depth: 3);
        var destroyer = PlayerRace.InstantiatePlayer("Destroyer", Sex.Male, room, new Point(0, 0));
        var recipient = PlayerRace.InstantiatePlayer("Recipient", Sex.Male, otherRoom, new Point(0, 0));
        var manager = room.Entity.Manager;
        var provider = (TestDataProvider)room.Game.Services.Data;
        const string name = "test destruction reward";
        const string weight = "$depth == 2 && $feature == 'test destruction feature' ? 1.0 : 0.0";
        var abilityType = type == RewardType.Ritual ? AbilityType.Boon
            : type == RewardType.Ability ? AbilityType.Feat : AbilityType.Buff;
        if (type == RewardType.Item)
        {
            provider.Items.Add(new Item { Name = name, Type = ItemType.Trinket, GenerationWeight = weight });
        }
        else if (type == RewardType.Race)
        {
            provider.PlayerRaces.Add(new PlayerRace { Name = name, GenerationWeight = weight });
        }
        else
        {
            provider.AddAbilities(new Ability
            {
                Name = name, Type = abilityType, GenerationWeight = weight,
                Activation = type == RewardType.Effect ? ActivationType.Default : ActivationType.Always,
                Effects = type == RewardType.Effect ? [new Heal { Amount = "5" }]
                    : [new ChangeProperty<int> { TargetActivator = true, PropertyName = "Might", Amount = "1" }]
            });
        }

        var definition = new Feature
        {
            Name = "test destruction feature", RewardType = type, AbilityRewardType = abilityType
        };
        provider.Features.Add(definition);
        room.Entity.Manager.PlayerChoiceSystem = new SingleCandidatePlayerChoiceSystem(name);
        var feature = definition.Instantiate(room, new Point(1, 0));
        Assert.Same(definition, feature.Feature!.Template);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1f, room.Entity.Manager.PlayerChoiceSystem.GetRewardWeight(definition, room, name));
        Assert.Equal(0f, room.Entity.Manager.PlayerChoiceSystem.GetRewardWeight(definition, otherRoom, name));
        if (type == RewardType.Effect)
        {
            destroyer.Being!.HitPoints -= 5;
            recipient.Being!.HitPoints -= 5;
        }
        var initial = new[] { destroyer, recipient }
            .Select(player => (Player: player, Might: player.Being!.Might, HitPoints: player.Being.HitPoints)).ToArray();

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(destroyer, room, new Point(1, 0)));
        var destroyIndex = manager.PlayerChoiceSystem.GetPresentation(destroyer)!.Options.ToList()
            .FindIndex(option => option.Identifier == PlayerChoiceSystem.DestroyAction);
        Assert.True(destroyIndex >= 0);
        Assert.True(manager.PlayerChoiceSystem.Select(destroyer, destroyIndex, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.True(feature.Feature.Destroyed);
        Assert.Equal(3, feature.Feature.UsedByPlayers);
        foreach (var (player, might, hitPoints) in initial)
        {
            Assert.True(PlayerChoiceSystem.HasUsedFeature(feature.Feature, player));
            Assert.Null(player.Player!.PendingChoice);
            if (player == destroyer && type != RewardType.Race)
            {
                Assert.Equal(might, player.Being!.Might);
                Assert.Equal(hitPoints, player.Being.HitPoints);
                Assert.DoesNotContain(player.Being.Items, item => item.Item!.TemplateName == name);
                Assert.Contains(player.Player.LogEntries, entry => entry.Message == "Nothing seems to have happened.");
                continue;
            }
            if (type == RewardType.Item)
            {
                Assert.Contains(player.Being!.Items, item => item.Item!.TemplateName == name);
            }
            else if (type == RewardType.Race)
            {
                Assert.Contains(provider.PlayerRaces.Get(name).GetRaceAbilityName(),
                    AbilityLevelSystem.GetPlayerRaceAbilityNames(player));
            }
            else if (type == RewardType.Effect)
            {
                Assert.Equal(hitPoints + 5, player.Being!.HitPoints);
            }
            else
            {
                Assert.Equal(might + 1, player.Being!.Might);
            }
        }
        Assert.Equal(0, room.Game.CurrentTick);
        var latePlayer = PlayerRace.InstantiatePlayer("Late player", Sex.Male, room, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(latePlayer, room, new Point(1, 0)));
        Assert.False(PlayerChoiceSystem.HasUsedFeature(feature.Feature, latePlayer));
    }

    [Fact]
    public void Destruction_finishes_a_pending_choice_without_granting_an_ineligible_reward()
    {
        var room = TestHelper.BuildRoom("...");
        var destroyer = PlayerRace.InstantiatePlayer("Destroyer", Sex.Male, room, new Point(0, 0));
        var recipient = PlayerRace.InstantiatePlayer("Recipient", Sex.Male, room, new Point(2, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var cell = new Point(1, 0);
        TestHelper.CreateRewardFeature(room, cell, "Test reward",
            [AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name], allowDestroy: true);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(recipient, room, cell));
        manager.AbilityLevelSystem.AddAbilityEffect(recipient, AbilityData.PatientVigor);
        manager.AbilityLevelSystem.AddAbilityEffect(recipient, AbilityData.FurrowsReserve);
        manager.Queue.ProcessQueue(manager);
        var initialHp = recipient.Being!.HitPointMaximum;
        var initialEp = recipient.Being.EnergyPointMaximum;

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(destroyer, room, cell));
        Assert.True(manager.PlayerChoiceSystem.Select(destroyer, destroyer.Player!.PendingChoice!.PendingChoiceOptions.Count, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(initialHp, recipient.Being.HitPointMaximum);
    Assert.Equal(initialEp, recipient.Being.EnergyPointMaximum);
    Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(recipient, AbilityData.PatientVigor.Name));
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(recipient, AbilityData.FurrowsReserve.Name));
        Assert.Null(recipient.Player!.PendingChoice);
        Assert.Equal(3, room.Features[cell].Feature!.UsedByPlayers);
        Assert.False(manager.PlayerChoiceSystem.TryPresentReward(recipient, room, cell));
    }

    [Fact]
    public void Bumping_a_race_pedestal_opens_the_menu_without_advancing_time()
    {
        var (playerEntity, room, pedestalCell) = Setup();
        var player = playerEntity.Player!;

        BumpPedestal(playerEntity);

        Assert.Equal(RewardType.Race, player.PendingChoice!.ChoiceType);
        Assert.Null(player.PendingChoice.PendingChoiceAction);
        Assert.Equal(
            [PlayerChoiceSystem.AddAction, PlayerChoiceSystem.RemoveAction, PlayerChoiceSystem.DestroyAction],
            player.PendingChoice.PendingChoiceOptions);
        var presentation = playerEntity.Manager.PlayerChoiceSystem.GetPresentation(playerEntity)!;
        Assert.Equal(RewardType.Race, presentation.Choice);
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
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, pedestalCell));
    }

    [Fact]
    public void Clicking_an_adjacent_race_pedestal_opens_the_menu()
    {
        var (playerEntity, _, pedestalCell) = Setup();
        playerEntity.Player!.NextAction = ActorActionType.MoveToCell;
        playerEntity.Player.NextActionTarget = pedestalCell.ToInt32();

        TestHelper.PerformTurn(playerEntity, playerEntity.Manager);

        Assert.Equal(
            RewardType.Race,
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
        Assert.Equal(3, raceNames.Count);
        Assert.DoesNotContain(PlayerRaceData.Human.Name, raceNames);
        var presentation = playerEntity.Manager.PlayerChoiceSystem.GetPresentation(playerEntity)!;
        Assert.Equal("Choose a race to add", presentation.Title);
        Assert.Equal(
            [.. raceNames, PlayerChoiceSystem.DestroyAction],
            presentation.Options.Select(option => option.Identifier));
        Assert.All(
            presentation.Options.Take(raceNames.Count),
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
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, pedestalCell));
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
        SelectOption(playerEntity, optionIndex: presentedRaceNames.Count);

        var race = Assert.Single(playerEntity.Being!.Races);
        Assert.Equal(PlayerRaceData.Human.GetRaceAbilityName(), race.Ability!.Name);
        Assert.Equal(2, race.Ability.Level);
        Assert.DoesNotContain(PlayerRaceData.Human.Name, presentedRaceNames);
        Assert.Null(playerEntity.Player.PendingChoice);
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, pedestalCell));
        Assert.True(room.Features[pedestalCell].Feature!.Destroyed);
        Assert.Equal(
            [
                $"Your {RaceRef(PlayerRaceData.Human.Name)} race increases to level 2.",
                "The destroyed race pedestal makes you a human."
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
        var (playerEntity, room, pedestalCell) = Setup();
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
            MapFeature.StoneFloor,
            GetTerrain(room, pedestalCell));
    }

    [Fact]
    public void Replace_selects_the_new_race_before_the_final_swap()
    {
        var (playerEntity, room, pedestalCell) = Setup();

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 1);

        Assert.Equal(PlayerChoiceSystem.ReplaceAction, playerEntity.Player!.PendingChoice!.PendingChoiceAction);
        Assert.DoesNotContain(PlayerRaceData.Human.Name, playerEntity.Player.PendingChoice.PendingChoiceOptions);
        var replacementRace = playerEntity.Player.PendingChoice.PendingChoiceOptions[0];

        playerEntity.Player.LogEntries.Clear();
        SelectOption(playerEntity, optionIndex: 0);

        Assert.Single(playerEntity.Being!.Races);
        Assert.Equal(PlayerChoiceSystem.RemoveAction, playerEntity.Player.PendingChoice!.PendingChoiceAction);
        Assert.Equal([PlayerRaceData.Human.Name], playerEntity.Player.PendingChoice.PendingChoiceOptions);
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, pedestalCell));
        Assert.Empty(playerEntity.Player.LogEntries);

        playerEntity.Player.LogEntries.Clear();
        SelectOption(playerEntity, optionIndex: 0);

        Assert.Single(playerEntity.Being.Races);
        Assert.Null(playerEntity.Player.PendingChoice);
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, pedestalCell));
        Assert.Equal(
            [$"You gain the {RaceRef(replacementRace)} race.", $"You lose the {RaceRef(PlayerRaceData.Human.Name)} race."],
            playerEntity.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Replace_offers_non_owned_races_then_current_races()
    {
        var (playerEntity, room, pedestalCell) = Setup();
        AddRace(playerEntity, PlayerRaceData.Elf);

        BumpPedestal(playerEntity);
        SelectOption(playerEntity, optionIndex: 1);

        Assert.Equal(PlayerChoiceSystem.ReplaceAction, playerEntity.Player!.PendingChoice!.PendingChoiceAction);
        Assert.DoesNotContain(PlayerRaceData.Elf.Name, playerEntity.Player.PendingChoice.PendingChoiceOptions);
        Assert.DoesNotContain(PlayerRaceData.Human.Name, playerEntity.Player.PendingChoice.PendingChoiceOptions);
        var replacementName = playerEntity.Player.PendingChoice.PendingChoiceOptions[0];
        SelectOption(playerEntity, 0);

        Assert.Equal(2, playerEntity.Being!.Races.Count);
        Assert.Equal(PlayerChoiceSystem.RemoveAction, playerEntity.Player.PendingChoice!.PendingChoiceAction);
        Assert.Equal([PlayerRaceData.Elf.Name, PlayerRaceData.Human.Name], playerEntity.Player.PendingChoice.PendingChoiceOptions);
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, pedestalCell));
        var elfIndex = playerEntity.Player.PendingChoice.PendingChoiceOptions.IndexOf(PlayerRaceData.Elf.Name);
        SelectOption(playerEntity, elfIndex);

        Assert.Equal(2, playerEntity.Being.Races.Count);
        Assert.DoesNotContain(PlayerRaceData.Elf.GetRaceAbilityName(), AbilityLevelSystem.GetPlayerRaceAbilityNames(playerEntity));
        Assert.Contains(
            playerEntity.Being.Races,
            race => (race.Ability!.Template?.Name ?? race.Ability.Name)
                    == playerEntity.Game.Services.Data.PlayerRaces
                        .Get(replacementName).GetRaceAbilityName());
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, pedestalCell));
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
        Assert.Equal(PlayerChoiceSystem.ReplaceAction, playerEntity.Player!.PendingChoice!.PendingChoiceAction);
        Assert.True(manager.PlayerChoiceSystem.Select(playerEntity, 0, manager).IsAccepted);
        Assert.Equal(PlayerChoiceSystem.RemoveAction, playerEntity.Player.PendingChoice!.PendingChoiceAction);
        var removeOptions = playerEntity.Player!.PendingChoice!.PendingChoiceOptions;
        Assert.Equal(3, removeOptions.Count);
        Assert.DoesNotContain(CreatureData.Tiger.Name, removeOptions);

        var elfIndex = removeOptions.IndexOf(PlayerRaceData.Elf.Name);
        var completion = manager.PlayerChoiceSystem.Select(
            playerEntity, elfIndex, manager);
        Assert.True(completion.IsAccepted, completion.ErrorMessage);
        Assert.Null(playerEntity.Player.PendingChoice);

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
        SelectOption(playerEntity, 0);
        var elfIndex = playerEntity.Player!.PendingChoice!.PendingChoiceOptions.IndexOf(PlayerRaceData.Elf.Name);
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
    public void Selection_resolves_the_source_pedestal_independently_of_the_players_heading()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var eastPedestal = new Point(2, 1);
        var northPedestal = new Point(1, 0);
        FeatureGenerator.Place(room, eastPedestal, RoomFeatures.RacePedestal);
        FeatureGenerator.Place(room, northPedestal, RoomFeatures.RacePedestal);
        room.FeatureTags |= RoomFeatures.RacePedestal;
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(1, 1));
        playerEntity.Manager.Queue.ProcessQueue(playerEntity.Manager);
        playerEntity.Position!.Heading = Direction.East;
        Assert.True(TerrainSystem.TryInteract(
            playerEntity, eastPedestal, room, playerEntity.Manager));

        playerEntity.Position.Heading = Direction.North;
        SelectOption(playerEntity, optionIndex: 0);
        SelectOption(playerEntity, optionIndex: 0);

        Assert.True(PlayerChoiceSystem.HasUsedFeature(room.Features[eastPedestal].Feature!, playerEntity));
        Assert.Equal(0, room.Features[northPedestal].Feature!.UsedByPlayers);
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, eastPedestal));
        Assert.Equal(MapFeature.StoneFloor, GetTerrain(room, northPedestal));
        Assert.True(room.FeatureTags.HasFlag(RoomFeatures.RacePedestal));
    }

    private static (GameEntity Player, RoomComponent Room, Point PedestalCell) Setup()
    {
        var room = TestHelper.BuildRoom("..");
        var pedestalCell = new Point(1, 0);
        FeatureGenerator.Place(room, pedestalCell, RoomFeatures.RacePedestal);
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
