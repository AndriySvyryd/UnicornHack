using MessagePack;
using MessagePack.Resolvers;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Features;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack;

public class FeatureChangeTest
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithResolver(CompositeResolver.Create(GeneratedResolver.Instance, BuiltinResolver.Instance));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Feature_snapshots_and_deltas_track_losing_and_regaining_perception(bool trap)
    {
        var room = TestHelper.BuildRoom(".....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        player.Position!.Heading = Direction.East;
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var feature = (trap ? FeatureData.SpikeTrap : FeatureData.SmallFountain).Instantiate(room, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);
        var builder = new FeatureChangeBuilder();
        builder.RegisterOnGroups(manager);
        builder.ResetProjection(player);
        var clientFeatures = new Dictionary<int, FeatureChange>();
        ClientStateHelpers.ExpandFeature(feature.Id, FeatureChangeBuilder.SerializeFeature(feature), clientFeatures);
        var previouslyPerceived = true;

        foreach (var heading in new[] { Direction.East, Direction.West, Direction.East })
        {
            var turn = TravelMessage.Create(manager);
            turn.ActorEntity = player;
            turn.TargetCell = player.Position.Cell;
            turn.TargetHeading = heading;
            manager.Process(turn);
            manager.Queue.ProcessQueue(manager);

            var snapshot = FeatureChangeBuilder.SerializeFeature(feature);
            var cancellationToken = TestContext.Current.CancellationToken;
            var bytes = MessagePackSerializer.Serialize(snapshot, Options, cancellationToken);
            using var json = System.Text.Json.JsonDocument.Parse(
                MessagePackSerializer.ConvertToJson(bytes, Options, cancellationToken));
            Assert.Equal(11, json.RootElement.GetArrayLength());
            var perceived = heading == Direction.East;
            Assert.Equal(perceived, json.RootElement[10].GetBoolean());

            var changes = builder.BuildChanges(room);
            if (previouslyPerceived == perceived)
            {
                Assert.Null(changes);
            }
            else
            {
                var delta = Assert.Single(changes!);
                Assert.Equal(feature.Id, delta.Key);
                Assert.Equal((ushort?)((1 << 10) | 1), delta.Value.ChangedProperties);
                var compact = MessagePackSerializer.Deserialize<FeatureChange>(
                    MessagePackSerializer.Serialize(delta.Value, Options, cancellationToken), Options, cancellationToken);
                ClientStateHelpers.ExpandFeature(feature.Id, compact, clientFeatures);
            }
            Assert.Equal(perceived, clientFeatures[feature.Id].IsCurrentlyPerceived);
            builder.Reset();
            previouslyPerceived = perceived;
        }
        builder.UnregisterFromGroups(manager);
    }

    [Fact]
    public void Unperceived_features_are_omitted_until_discovery_from_snapshots_and_deltas()
    {
        var room = TestHelper.BuildRoom(".....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        player.Position!.Heading = Direction.West;
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var feature = FeatureData.SmallFountain.Instantiate(room, new Point(2, 0));
        feature.Feature!.UsedByPlayers = PlayerChoiceSystem.GetPlayerUseMask(player);
        manager.Queue.ProcessQueue(manager);
        Assert.Null(feature.Position!.Knowledge);
        var builder = new FeatureChangeBuilder();
        builder.RegisterOnGroups(manager);
        builder.ResetProjection(player);
        var context = new SerializationContext(null, player, room.Game.Services);
        Assert.Empty(RoomChangeBuilder.SerializeRoom(room.Entity, context).Features!);
        feature.Feature.Destroyed = true;
        feature.Feature.Destroyed = false;
        Assert.Null(builder.BuildChanges(room));

        var turn = TravelMessage.Create(manager);
        turn.ActorEntity = player;
        turn.TargetCell = player.Position.Cell;
        turn.TargetHeading = Direction.East;
        manager.Process(turn);
        manager.Queue.ProcessQueue(manager);

        var discovered = Assert.Single(builder.BuildChanges(room)!);
        Assert.Equal(feature.Id, discovered.Key);
        Assert.Null(discovered.Value.ChangedProperties);
        Assert.Equal(feature.Feature.Name, discovered.Value.Name);
        Assert.Single(RoomChangeBuilder.SerializeRoom(room.Entity, context).Features!);
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature.Feature, player));
    }

    [Fact]
    public void Combined_trap_display_tracks_current_entry_geometry_and_the_forecast_attack_independently()
    {
        var room = TestHelper.BuildRoom("""
            .......
            .......
            .......
            .......
            .......
            .......
            .......
            """);
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        var definition = new Feature
        {
            Name = "test current and forecast display",
            Behavior = UnicornHack.Systems.Features.FeatureBehavior.TriggerOnEntry
                | UnicornHack.Systems.Features.FeatureBehavior.TriggerOnInterval,
            Abilities =
            [
                new Ability
                {
                    Name = "test forecast display attack", Type = AbilityType.RangedAttack, TargetingShape = TargetingShape.Plus,
                    Range = 0, Delay = "100", Effects = [new FireDamage { Amount = "1" }]
                },
                new Ability
                {
                    Name = "test current display attack", Type = AbilityType.MeleeAttack, TargetingShape = TargetingShape.Square,
                    Range = 1, Effects = [new PhysicalDamage { Amount = "1" }]
                }
            ]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);
        feature.ActiveEffects!.Abilities[0].Ability!.CooldownTick = 50;
        var snapshot = FeatureChangeBuilder.SerializeFeature(feature);

        Assert.Equal(9, snapshot.AffectedCells.Length);
        Assert.Equal("test forecast display attack", Assert.Single(snapshot.NextActions!).Name);
        Assert.Equal(new Point(3, 3).ToInt32(), Assert.Single(snapshot.NextActions!).Target);
        Assert.Null(snapshot.CooldownTick);
        var builder = new FeatureChangeBuilder();
        builder.RegisterOnGroups(manager);
        builder.ResetProjection(player);
        feature.ActiveEffects.Abilities[1].Ability!.Range = 2;
        var widened = Assert.Single(builder.BuildChanges(room)!).Value;

        Assert.Equal(25, widened.AffectedCells.Length);
        Assert.Equal(9, snapshot.AffectedCells.Length);
        Assert.Null(widened.NextActions);
    }

    [Fact]
    public void Combined_trigger_flags_publish_both_footprints_without_duplicate_cells()
    {
        var room = TestHelper.BuildRoom(@".....
.....
.....
.....
.....");
        PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var definition = new Feature
        {
            Name = "test combined trap",
            Behavior = UnicornHack.Systems.Features.FeatureBehavior.TriggerOnEntry
                | UnicornHack.Systems.Features.FeatureBehavior.TriggerOnInterval,
            Abilities = [new Ability
            {
                Name = "test combined attack", Type = AbilityType.RangedAttack, TargetingShape = TargetingShape.Line,
                Range = 2, Delay = "100", Windup = 200, Effects = [new FireDamage { Amount = "8" }]
            }]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(2, 2));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var snapshot = FeatureChangeBuilder.SerializeFeature(feature);

        Assert.Contains(new Point(0, 2).ToInt32(), snapshot.AffectedCells!);
        Assert.Contains(new Point(4, 2).ToInt32(), snapshot.AffectedCells!);
        Assert.Equal(snapshot.AffectedCells!.Length, snapshot.AffectedCells.Distinct().Count());
        Assert.Equal(4, snapshot.NextActions!.Length);
        var danger = room.CellThreats!;
        Assert.Contains(new Point(0, 2), danger.Keys);
        Assert.Contains(new Point(4, 2), danger.Keys);
    }

    [Fact]
    public void Telegraph_and_firing_use_the_same_available_authored_attack()
    {
        var room = TestHelper.BuildRoom(@".....
.....
.....
.....
.....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(4, 2));
        var manager = room.Entity.Manager;
        var definition = new Feature
        {
            Name = "test multi attack launcher", Behavior = UnicornHack.Systems.Features.FeatureBehavior.TriggerOnInterval,
            Abilities =
            [
                new Ability { Name = "test first attack", Type = AbilityType.RangedAttack, Range = 1,
                    Delay = "100", Windup = 200, Cooldown = 100, Effects = [new FireDamage { Amount = "8" }] },
                new Ability { Name = "test fallback attack", Type = AbilityType.RangedAttack, Range = 2,
                    Delay = "50", Windup = 50, Cooldown = 100, Effects = [new FireDamage { Amount = "5" }] }
            ]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, feature.ActiveEffects!.Abilities.Count);
        var first = feature.ActiveEffects.Abilities[0].Ability!;
        first.CooldownTick = 600;
        var snapshot = FeatureChangeBuilder.SerializeFeature(feature);

        Assert.All(snapshot.NextActions!, action => Assert.Equal("test fallback attack", action.Name));
        Assert.Contains(new Point(4, 2).ToInt32(), snapshot.AffectedCells!);
        var danger = room.CellThreats!;
        Assert.Contains(new Point(4, 2), danger.Keys);
        var hp = player.Being!.HitPoints;
        room.Game.CurrentTick = 300;
        manager.FeatureSystem.Advance(feature, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.True(player.Being.HitPoints < hp);
        Assert.Equal(600, first.CooldownTick);
        Assert.Equal(450, feature.ActiveEffects.Abilities[1].Ability!.CooldownTick);
        Assert.Equal(400, feature.Feature!.NextActionTick);

        first.IsUsable = false;
        feature.ActiveEffects.Abilities[1].Ability!.IsUsable = false;
        snapshot = FeatureChangeBuilder.SerializeFeature(feature);
        Assert.Null(snapshot.NextActions);
        Assert.Empty(snapshot.AffectedCells!);
        Assert.Same(danger, room.CellThreats);
        Assert.Empty(room.CellThreats!);
    }

    [Fact]
    public void Entry_trigger_footprints_follow_the_selected_ability_and_its_range_changes()
    {
        var room = TestHelper.BuildRoom(@".....
.....
.....
.....
.....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var definition = new Feature
        {
            Name = "test changing entry trap", Behavior = UnicornHack.Systems.Features.FeatureBehavior.TriggerOnEntry,
            Abilities =
            [
                new Ability
                {
                    Name = "test single entry attack", Type = AbilityType.MeleeAttack,
                    TargetingShape = TargetingShape.Square, Range = 0, Effects = [new PhysicalDamage { Amount = "1" }]
                },
                new Ability
                {
                    Name = "test changing cone attack", Type = AbilityType.MeleeAttack,
                    TargetingShape = TargetingShape.Cone, TargetingShapeSize = 2, Range = 1,
                    Effects = [new PhysicalDamage { Amount = "2" }]
                }
            ]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(2, 2));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var builder = new FeatureChangeBuilder();
        builder.RegisterOnGroups(manager);
        builder.ResetProjection(player);
        var snapshot = FeatureChangeBuilder.SerializeFeature(feature);
        Assert.Equal([new Point(2, 2).ToInt32()], snapshot.AffectedCells);

        feature.ActiveEffects!.Abilities[0].Ability!.CooldownTick = 100;
        var fallback = Assert.Single(builder.BuildChanges(room)!).Value;
        Assert.Equal(5, fallback.AffectedCells!.Length);
        Assert.Contains(new Point(2, 1).ToInt32(), fallback.AffectedCells);
        Assert.DoesNotContain(new Point(1, 2).ToInt32(), fallback.AffectedCells);
        builder.Reset();
        feature.ActiveEffects.Abilities[1].Ability!.Range = 2;
        var widened = Assert.Single(builder.BuildChanges(room)!).Value;
        Assert.Contains(new Point(4, 2).ToInt32(), widened.AffectedCells!);
        Assert.Equal([new Point(2, 2).ToInt32()], snapshot.AffectedCells);
    }

    [Fact]
    public void Entry_trigger_footprints_refresh_after_rotation_and_terrain_changes()
    {
        var room = TestHelper.BuildRoom(@".....
.....
.....
.....
.....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var definition = new Feature
        {
            Name = "test line trap", Behavior = UnicornHack.Systems.Features.FeatureBehavior.TriggerOnEntry,
            Abilities = [new Ability
            {
                Name = "test line attack", Type = AbilityType.MeleeAttack, TargetingShape = TargetingShape.Line,
                Range = 2, Effects = [new PhysicalDamage { Amount = "15", ArmorPenetration = "2" }]
            }]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(2, 2));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        room.TerrainChanges = [];
        var builder = new FeatureChangeBuilder();
        builder.RegisterOnGroups(manager);
        builder.ResetProjection(player);

        feature.Position!.Heading = Direction.West;
        var rotated = Assert.Single(builder.BuildChanges(room)!).Value;
        Assert.Contains(new Point(1, 2).ToInt32(), rotated.AffectedCells!);
        Assert.Contains(new Point(0, 2).ToInt32(), rotated.AffectedCells!);
        Assert.DoesNotContain(new Point(3, 2).ToInt32(), rotated.AffectedCells!);
        builder.Reset();
        UnicornHack.Systems.Levels.TerrainSystem.SetTerrain(MapFeature.StoneWall, new Point(1, 2), room);
        var blocked = Assert.Single(builder.BuildChanges(room)!).Value;
        Assert.DoesNotContain(new Point(0, 2).ToInt32(), blocked.AffectedCells!);
    }

    [Fact]
    public void Repeated_change_checks_reuse_prepared_geometry_and_keep_queued_arrays_immutable()
    {
        var room = TestHelper.BuildRoom(@".....
.....
.....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 1));
        var launcher = FeatureData.FireboltLauncher.Instantiate(room, new Point(2, 1));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        room.TerrainChanges?.Clear();
        var builder = new FeatureChangeBuilder();
        builder.RegisterOnGroups(manager);
        builder.ResetProjection(player);
        launcher.Feature!.NextActionTick = 600;
        Assert.True(builder.HasChangesForRoom(room));

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100; index++)
        {
            builder.HasChangesForRoom(room);
        }
        Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - allocatedBefore, 0, 4096);
        var snapshot = FeatureChangeBuilder.SerializeFeature(launcher);
        var cells = snapshot.AffectedCells!.ToArray();
        builder.BuildChanges(room);
        player.Position!.Cell = new Point(1, 1);
        var delta = Assert.Single(builder.BuildChanges(room)!).Value;

        Assert.NotNull(delta.AffectedCells);
        Assert.Equal(cells, snapshot.AffectedCells);
        Assert.DoesNotContain(new Point(0, 1).ToInt32(), delta.AffectedCells!);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Normal_and_destroyed_feature_use_match_the_delta_applied_client_state(bool destroy)
    {
        var room = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var feature = FeatureData.SmallFountain.Instantiate(room, new Point(1, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var listener = WebTestHelper.SetupListeners(manager);
        var menu = WebTestHelper.PerformAndVerify(player.Player!, listener, room.Game.Services,
            ActorActionType.MoveOneCell, (int)Direction.East);
        Assert.NotNull(menu.PendingOptions);
        Assert.Equal(feature.Id, player.Player!.PendingChoice!.SourceFeatureId);
        if (destroy)
        {
            var result = WebTestHelper.PerformAndVerify(player.Player!, listener, room.Game.Services,
                ActorActionType.SelectChoice, player.Player!.PendingChoice!.PendingChoiceOptions.Count);
            Assert.Null(result.PendingOptions);
            Assert.True(PlayerChoiceSystem.HasUsedFeature(feature.Feature!, player));
            Assert.True(result.Room!.Features![feature.Id].Destroyed);
        }
        else
        {
            var result = WebTestHelper.PerformAndVerify(player.Player!, listener, room.Game.Services, ActorActionType.SelectChoice, 0);
            Assert.Null(result.PendingOptions);
            Assert.True(PlayerChoiceSystem.HasUsedFeature(feature.Feature!, player));
            Assert.False(feature.Feature!.Destroyed);
        }

        Assert.Equal(0, room.Game.CurrentTick);
    }

    [Fact]
    public void Personal_claims_keep_use_state_on_the_server_without_shared_feature_changes()
    {
        var room = TestHelper.BuildRoom("...");
        var first = PlayerRace.InstantiatePlayer("First", Sex.Male, room, new Point(0, 0));
        var second = PlayerRace.InstantiatePlayer("Second", Sex.Male, room, new Point(2, 0));
        var feature = FeatureData.MinorShrine.Instantiate(room, new Point(1, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var shared = new FeatureChangeBuilder();
        shared.RegisterOnGroups(manager);
        shared.ResetProjection(first);

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(first, room, new Point(1, 0)));
        Assert.True(manager.PlayerChoiceSystem.Select(first, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature.Feature!, first));
        Assert.False(PlayerChoiceSystem.HasUsedFeature(feature.Feature!, second));
        Assert.Null(shared.BuildChanges(room));
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(second, room, new Point(1, 0)));
    }

    [Fact]
    public void Destruction_clears_all_affected_choices_without_sending_private_use_state()
    {
        var room = TestHelper.BuildRoom(@"...
...");
        var first = PlayerRace.InstantiatePlayer("First", Sex.Male, room, new Point(0, 0));
        var second = PlayerRace.InstantiatePlayer("Second", Sex.Male, room, new Point(2, 0));
        var third = PlayerRace.InstantiatePlayer("Third", Sex.Male, room, new Point(1, 1));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var cell = new Point(1, 0);
        TestHelper.CreateRewardFeature(room, cell, "Test reward",
            [AbilityData.PatientVigor.Name, AbilityData.FurrowsReserve.Name], allowDestroy: true);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(first, room, cell));
        Assert.True(manager.PlayerChoiceSystem.Select(first, 0, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);
        var shared = new FeatureChangeBuilder();
        shared.RegisterOnGroups(manager);
        shared.ResetProjection(first);
        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(third, room, cell));

        Assert.True(manager.PlayerChoiceSystem.TryPresentReward(second, room, cell));
        Assert.True(manager.PlayerChoiceSystem.Select(second, second.Player!.PendingChoice!.PendingChoiceOptions.Count, manager).IsAccepted);
        manager.Queue.ProcessQueue(manager);

        var feature = room.Features[cell].Feature!;
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, first));
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, second));
        Assert.True(PlayerChoiceSystem.HasUsedFeature(feature, third));
        Assert.True(Assert.Single(shared.BuildChanges(room)!).Value.Destroyed);
        Assert.Null(third.Player!.PendingChoice);
        Assert.Equal(0, room.Game.CurrentTick);
    }

    [Fact]
    public void Feature_projection_emits_only_final_changes_and_preserves_known_state_across_reset()
    {
        var room = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var feature = FeatureData.SmallFountain.Instantiate(room, new Point(1, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var builder = new FeatureChangeBuilder();
        builder.RegisterOnGroups(manager);
        builder.ResetProjection(player);

        feature.Feature!.Destroyed = true;
        feature.Feature.Destroyed = false;
        Assert.Null(builder.BuildChanges(room));
        feature.Feature.Destroyed = true;
        var delta = Assert.Single(builder.BuildChanges(room)!).Value;
        Assert.Equal((ushort?)17, delta.ChangedProperties);
        Assert.True(delta.Destroyed);
        builder.Reset();
        Assert.Null(builder.BuildChanges(room));
        feature.Position = null;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal((ushort?)0, Assert.Single(builder.BuildChanges(room)!).Value.ChangedProperties);
    }

    [Fact]
    public void Full_feature_snapshots_round_trip_positions_state_timing_and_attacks()
    {
        var value = new FeatureChange
        {
            ChangedProperties = null,
            Name = "firebolt launcher",
            LevelX = 2,
            LevelY = 3,
            BlocksMovement = true,
            NextActionTick = 300,
            NextActions = [new ActorActionChange { Name = "launcher fire bolt", Target = 258 }],
            AffectedCells = [258, 259]
        };

        var result = MessagePackSerializer.Deserialize<FeatureChange>(
            MessagePackSerializer.Serialize(value, Options, TestContext.Current.CancellationToken), Options,
            TestContext.Current.CancellationToken);
        Assert.Null(result.ChangedProperties);
        Assert.Equal(value.Name, result.Name);
        Assert.Equal(2, result.LevelX);
        Assert.Equal(3, result.LevelY);
        Assert.True(result.BlocksMovement);
        Assert.Equal(300, result.NextActionTick);
        Assert.Equal("launcher fire bolt", Assert.Single(result.NextActions!).Name);
        Assert.Equal(value.AffectedCells, result.AffectedCells);
    }

    [Fact]
    public void Sparse_feature_updates_only_round_trip_selected_fields()
    {
        var value = new FeatureChange { NextActionTick = 600 };
        var bytes = MessagePackSerializer.Serialize(value, Options, TestContext.Current.CancellationToken);
        var result = MessagePackSerializer.Deserialize<FeatureChange>(bytes, Options, TestContext.Current.CancellationToken);

        Assert.Equal(value.ChangedProperties, result.ChangedProperties);
        Assert.Equal(600, result.NextActionTick);
        Assert.Equal("", result.Name);
        Assert.True(bytes.Length < 10);
    }

    [Fact]
    public void Feature_deletions_and_appended_room_collection_round_trip()
    {
        var value = new RoomChange
        {
            Features = new Dictionary<int, FeatureChange> { [42] = new FeatureChange { ChangedProperties = 0 } }
        };
        var result = MessagePackSerializer.Deserialize<RoomChange>(
            MessagePackSerializer.Serialize(value, Options, TestContext.Current.CancellationToken), Options,
            TestContext.Current.CancellationToken);

        Assert.Equal(value.ChangedProperties, result.ChangedProperties);
        Assert.Equal((ushort?)0, Assert.Single(result.Features!).Value.ChangedProperties);
    }
}