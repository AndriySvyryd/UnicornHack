using UnicornHack.Data.Creatures;
using UnicornHack.Data.Features;
using UnicornHack.Generation.Effects;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Features;

public class FeatureSystemTest
{
    [Fact]
    public void Feature_positions_update_the_room_index_without_entering_item_or_actor_collections()
    {
        var room = TestHelper.BuildRoom("...");
        var manager = room.Entity.Manager;
        var reward = TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward", ["Patient Vigor"]);
        var entity = room.Features[new Point(1, 0)];
        var component = entity.Feature!;

        entity.Position!.Cell = new Point(2, 0);
        manager.Queue.ProcessQueue(manager);

        Assert.False(room.Features.ContainsKey(new Point(1, 0)));
        Assert.Same(entity, room.Features[new Point(2, 0)]);
        Assert.Empty(room.Items);
        Assert.Empty(room.Actors);
        Assert.Same(reward, component);
        entity.Position = null;
        manager.Queue.ProcessQueue(manager);
        Assert.Empty(room.Features);
        Assert.Equal("", component.Name);
    }

    [Fact]
    public void Removing_a_room_releases_its_features_and_owned_rewards()
    {
        var room = TestHelper.BuildRoom("..");
        var manager = room.Entity.Manager;
        var reward = TestHelper.CreateRewardFeature(room, new Point(1, 0), "Test reward", ["Patient Vigor"]);
        var component = room.Features[new Point(1, 0)].Feature!;
        var featureId = reward.EntityId;

        room.Entity.Room = null;
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.FindEntity(featureId));
        Assert.Equal("", component.Name);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Feature_selection_uses_weights_and_eligible_candidates(bool canBlockMovement)
    {
        var room = TestHelper.BuildRoom(canBlockMovement ? """
            ...
            ...
            ...
            """ : "...");
        var data = (TestDataProvider)room.Game.Services.Data;
        var blocked = new Feature
        {
            Name = "test weighted blocker", RoomFeature = RoomFeatures.Trapped,
            GenerationWeight = $"$branch == '{room.Level.Branch.Name}' && $depth == {room.Level.Depth} ? Infinity : 0.0"
        };
        var traversable = new Feature
        {
            Name = "test weighted traversable", RoomFeature = RoomFeatures.Trapped,
            Behavior = FeatureBehavior.TriggerOnEntry, GenerationWeight = "Infinity"
        };
        data.Features.Add(new Feature
        {
            Name = "test wrong kind", RoomFeature = RoomFeatures.SmallFountain, GenerationWeight = "Infinity"
        });
        data.Features.Add(new Feature
        {
            Name = "test disabled feature", RoomFeature = RoomFeatures.Trapped, GenerationWeight = "0"
        });
        data.Features.Add(blocked);
        data.Features.Add(traversable);
        var feature = FeatureGenerator.Place(room, new Point(1, canBlockMovement ? (byte)1 : (byte)0), RoomFeatures.Trapped);

        Assert.Same(canBlockMovement ? blocked : traversable, feature.Feature!.Template);
    }

    private static List<Point> GetTriggerCells(GameEntity entity)
    {
        var cells = new List<Point>();
        entity.Manager.FeatureSystem.GetTriggerCells(entity, cells, static (target, cell) => target.Add(cell));
        return cells;
    }

    [Fact]
    public void Changed_threatened_cells_preserve_unchanged_cells_and_other_sources()
    {
        var room = TestHelper.BuildRoom(@"
.......
.......
.......
.......
.......
.......
.......");
        var manager = room.Entity.Manager;
        var definition = new Feature
        {
            Name = "test incremental square threat", Behavior = FeatureBehavior.TriggerOnEntry,
            Abilities = [new Ability
            {
                Name = "test square threat", Type = AbilityType.MeleeAttack, TargetingShape = TargetingShape.Square,
                Range = 1, Effects = [new PhysicalDamage { Amount = "1" }]
            }]
        };
        var moving = definition.Instantiate(room, new Point(2, 2));
        var overlapping = FeatureData.SpikeTrap.Instantiate(room, new Point(1, 1));
        var unrelated = FeatureData.SpikeTrap.Instantiate(room, new Point(6, 5));
        manager.Queue.ProcessQueue(manager);
        var threats = room.CellThreats!;
        var retained = threats[new Point(3, 3)];
        var shared = threats[new Point(1, 1)];
        var untouched = threats[new Point(6, 5)];

        moving.Position!.Cell = new Point(3, 2);
        Assert.Same(threats, room.CellThreats);
        Assert.Same(retained, threats[new Point(3, 3)]);
        Assert.Same(shared, threats[new Point(1, 1)]);
        Assert.Equal([overlapping.Id], shared);
        Assert.Same(untouched, threats[new Point(6, 5)]);
        Assert.Equal([unrelated.Id], untouched);
        Assert.DoesNotContain(new Point(1, 2), threats.Keys);
        Assert.Contains(moving.Id, threats[new Point(4, 2)]);

        moving.ActiveEffects!.Abilities[0].Ability!.Range = 0;
        _ = room.CellThreats;
        Assert.DoesNotContain(new Point(3, 3), threats.Keys);
        Assert.Same(shared, threats[new Point(1, 1)]);
        Assert.Same(untouched, threats[new Point(6, 5)]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Threat_queries_movement_and_reused_trigger_output_do_not_allocate(bool refreshSource)
    {
        var room = TestHelper.BuildRoom(new string('.', 64));
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        for (byte x = 2; x < room.Width; x++)
        {
            FeatureData.SpikeTrap.Instantiate(room, new Point(x, 0));
        }
        manager.Queue.ProcessQueue(manager);
        var original = room.CellThreats!;
        var message = TraveledMessage.Create(manager);
        message.Entity = player;
        message.InitialLevel = room.Entity;
        message.InitialLevelCell = new Point(1, 0);
        message.Successful = true;
        var feature = room.Features[new Point(2, 0)];
        var sourceCell = original[new Point(2, 0)];
        var change = new EntityChange<GameEntity>(feature);
        var points = new HashSet<Point>();
        Action<HashSet<Point>, Point> addCell = static (target, cell) => target.Add(cell);
        manager.FeatureSystem.GetTriggerCells(feature, points, addCell);
        for (var iteration = 0; iteration < 10; iteration++)
        {
            if (refreshSource)
            {
                manager.CellThreatTracker.OnEntityAdded(change);
            }
            manager.FeatureSystem.Process(message, manager);
            points.Clear();
            manager.FeatureSystem.GetTriggerCells(feature, points, addCell);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < 1000; iteration++)
        {
            if (refreshSource)
            {
                manager.CellThreatTracker.OnEntityAdded(change);
            }
            manager.FeatureSystem.Process(message, manager);
            points.Clear();
            manager.FeatureSystem.GetTriggerCells(feature, points, addCell);
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        manager.ReturnMessage(message);

        Assert.Equal(0, allocated);
        Assert.Same(original, room.CellThreats);
        Assert.Same(sourceCell, original[new Point(2, 0)]);
        Assert.Equal([new Point(2, 0)], points);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Inline_abilities_are_feature_scoped_and_can_share_names(bool standaloneHasSameName)
    {
        var room = TestHelper.BuildRoom(".....");
        var manager = room.Entity.Manager;
        var data = (TestDataProvider)room.Game.Services.Data;
        const string attackName = "test feature-local attack";
        var standalone = new Ability { Name = attackName, Type = AbilityType.MeleeAttack, Range = 9 };
        if (standaloneHasSameName)
        {
            data.AddAbilities(standalone);
        }

        var first = new Feature
        {
            Name = "test first local feature",
            Abilities = [new Ability
            {
                Name = attackName, Type = AbilityType.MeleeAttack, Range = 1, LevelExpression = "1",
                Effects = [new PhysicalDamage { Amount = "1" }]
            }]
        };
        var second = new Feature
        {
            Name = "test second local feature",
            Abilities = [new Ability
            {
                Name = attackName, Type = AbilityType.MeleeAttack, Range = 2, LevelExpression = "1",
                Effects = [new PhysicalDamage { Amount = "2" }]
            }]
        };
        data.Features.Add(first);
        data.Features.Add(second);
        var firstEntity = first.Instantiate(room, new Point(1, 0));
        var secondEntity = second.Instantiate(room, new Point(3, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.Same(Ability.Loader, DataProvider.Default.Abilities);
        Assert.Same(standaloneHasSameName ? standalone : null, data.Abilities.Find(attackName));
        Assert.DoesNotContain(data.Abilities.GetAll(), ability => ReferenceEquals(ability, first.Abilities[0])
            || ReferenceEquals(ability, second.Abilities[0]));
        Assert.Same(first.Abilities[0], Assert.Single(firstEntity.ActiveEffects!.Abilities).Ability!.Template);
        Assert.Same(second.Abilities[0], Assert.Single(secondEntity.ActiveEffects!.Abilities).Ability!.Template);
        Assert.Equal(1, manager.FeatureSystem.SelectAttack(firstEntity)!.Range);
        Assert.Equal(2, manager.FeatureSystem.SelectAttack(secondEntity)!.Range);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Combined_triggers_use_current_entry_geometry_and_separate_forecast_attacks(bool entersTrigger)
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
        var initialCell = entersTrigger ? new Point(3, 1) : new Point(1, 3);
        var targetCell = entersTrigger ? new Point(3, 2) : new Point(2, 3);
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, initialCell);
        var manager = room.Entity.Manager;
        var definition = new Feature
        {
            Name = "test current and forecast trap",
            Behavior = FeatureBehavior.TriggerOnEntry | FeatureBehavior.TriggerOnInterval,
            Abilities =
            [
                new Ability
                {
                    Name = "test forecast attack", Type = AbilityType.RangedAttack, TargetingShape = TargetingShape.Plus,
                    Range = entersTrigger ? 0 : 2, Delay = "100", Effects = [new FireDamage { Amount = "1" }]
                },
                new Ability
                {
                    Name = "test current entry attack", Type = AbilityType.MeleeAttack, TargetingShape = TargetingShape.Square,
                    Range = entersTrigger ? 1 : 0, Cooldown = 100,
                    Effects = [new PhysicalDamage { Amount = "15", ArmorPenetration = "2" }]
                }
            ]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);
        var forecast = feature.ActiveEffects!.Abilities[0].Ability!;
        var entry = feature.ActiveEffects.Abilities[1].Ability!;
        forecast.CooldownTick = 50;

        Assert.Same(entry, manager.FeatureSystem.SelectAttack(feature));
        Assert.Same(forecast, manager.FeatureSystem.SelectAttack(feature, feature.Feature!.NextActionTick));
        Assert.Equal(entersTrigger ? 9 : 1, GetTriggerCells(feature).Count);
        var attackCells = new List<Point>();
        manager.FeatureSystem.GetAttackCells(feature, attackCells, static (target, cell) => target.Add(cell));
        Assert.Equal(entersTrigger ? 1 : 9, attackCells.Count);
        Assert.Contains(feature.Id, room.CellThreats![targetCell]);
        Assert.Contains(targetCell, feature.Feature!.ThreatenedCells!.Keys);
        Assert.Equal(entersTrigger, manager.FeatureSystem.IsTriggerCell(feature, entry, targetCell));
        var initialHp = player.Being!.HitPoints;
        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = player;
        travel.TargetCell = targetCell;
        travel.TargetHeading = entersTrigger ? Direction.South : Direction.East;
        manager.Process(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(entersTrigger, player.Being.HitPoints < initialHp);
        Assert.Equal(entersTrigger, entry.CooldownTick != null);
        Assert.Equal(50, forecast.CooldownTick);
        Assert.Equal(entersTrigger ? 0 : 1, GetTriggerCells(feature).Count);
    }

    [Fact]
    public void Cell_threats_keep_overlapping_feature_and_planned_actor_ids_and_refresh_plan_changes()
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
        var manager = room.Entity.Manager;
        var actor = CreatureData.Boggart.Instantiate(room, new Point(1, 3));
        var spike = FeatureData.SpikeTrap.Instantiate(room, new Point(5, 3));
        var ability = new Ability
        {
            Name = "test planned square attack", Type = AbilityType.RangedAttack,
            TargetingShape = TargetingShape.Square, Range = 6, Effects = [new PhysicalDamage { Amount = "1" }]
        }.AddToAffectable(actor);
        manager.Queue.ProcessQueue(manager);
        ability.Slot = 5;
        actor.AI!.NextAction = ActorActionType.UseAbilitySlot;
        actor.AI.NextActionTarget = 5;
        actor.AI.NextActionTarget2 = actor.Position!.Cell!.Value.DifferenceTo(new Point(5, 3)).ToInt32();
        var threats = room.CellThreats!;

        Assert.Same(room.CellThreats, threats);
        Assert.Same(actor.AI!.ThreatenedCells,
            manager.CellThreatTracker.GetCachedTargetCells(actor, ability.EntityId, new Point(5, 3)));
        Assert.Contains(new Point(5, 3), actor.AI.ThreatenedCells!.Keys);
        Assert.Equal(2, threats[new Point(5, 3)].Count);
        Assert.Contains(spike.Id, threats[new Point(5, 3)]);
        Assert.Contains(actor.Id, threats[new Point(5, 3)]);
        Assert.True(FeatureSystem.IsCellThreatened(threats, new Point(5, 3), actor.Id));
        var sharedSources = threats[new Point(5, 3)];

        actor.AI.NextActionTarget2 = actor.Position.Cell.Value.DifferenceTo(new Point(2, 4)).ToInt32();
        var redirected = room.CellThreats!;
        Assert.Same(threats, redirected);
        Assert.Same(sharedSources, redirected[new Point(5, 3)]);
        Assert.Equal([spike.Id], redirected[new Point(5, 3)]);
        Assert.Equal([actor.Id], redirected[new Point(2, 4)]);
        Assert.False(FeatureSystem.IsCellThreatened(redirected, new Point(2, 4), actor.Id));

        actor.Position.Cell = new Point(1, 4);
        Assert.Null(actor.AI.ThreatenedCells);
        Assert.Null(manager.CellThreatTracker.GetCachedTargetCells(actor, ability.EntityId, new Point(2, 5)));
        var displaced = room.CellThreats!;
        Assert.DoesNotContain(new Point(2, 4), displaced.Keys);
        Assert.Equal([actor.Id], displaced[new Point(2, 5)]);
        Assert.Same(sharedSources, displaced[new Point(5, 3)]);

        actor.AI.NextAction = ActorActionType.Wait;
        Assert.DoesNotContain(new Point(2, 5), room.CellThreats!.Keys);
        Assert.Null(manager.CellThreatTracker.GetCachedTargetCells(actor, ability.EntityId, new Point(2, 5)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Planned_actor_threats_move_between_rooms_and_clear_when_ai_is_removed(bool removePreviousRoom)
    {
        var room = TestHelper.BuildRoom("""
            ....
            ....
            ....
            """);
        var other = TestHelper.BuildRoom("""
            ....
            ....
            ....
            """, room.Game);
        var manager = room.Entity.Manager;
        var actor = CreatureData.Boggart.Instantiate(room, new Point(1, 1));
        var ability = new Ability
        {
            Name = "test traveling planned attack", Type = AbilityType.RangedAttack,
            TargetingShape = TargetingShape.Square, Range = 2, Effects = [new PhysicalDamage { Amount = "1" }]
        }.AddToAffectable(actor);
        manager.Queue.ProcessQueue(manager);
        ability.Slot = 5;
        actor.AI!.NextAction = ActorActionType.UseAbilitySlot;
        actor.AI.NextActionTarget = 5;
        actor.AI.NextActionTarget2 = actor.Position!.Cell!.Value.DifferenceTo(new Point(2, 1)).ToInt32();
        Assert.Contains(actor.Id, room.CellThreats![new Point(2, 1)]);
        Assert.Empty(other.CellThreats!);
        var ai = actor.AI!;

        actor.Position.RoomEntity = other.Entity;
        Assert.Null(ai.ThreatenedCells);
        if (removePreviousRoom)
        {
            room.Entity.Room = null;
            manager.Queue.ProcessQueue(manager);
        }
        else
        {
            Assert.Empty(room.CellThreats!);
        }
        Assert.Contains(actor.Id, other.CellThreats![new Point(2, 1)]);
        actor.RemoveComponent((int)EntityComponent.AI);
        Assert.Empty(other.CellThreats!);
        Assert.Null(ai.ThreatenedCells);
    }

    [Fact]
    public void Planned_actor_threats_follow_position_heading_ability_changes_and_death()
    {
        var room = TestHelper.BuildRoom("""
            .....
            .....
            .....
            .....
            .....
            """);
        var manager = room.Entity.Manager;
        var actor = CreatureData.Boggart.Instantiate(room, new Point(2, 2));
        var ability = new Ability
        {
            Name = "test planned cone attack", Type = AbilityType.RangedAttack,
            TargetingShape = TargetingShape.Cone, Range = 1, Effects = [new PhysicalDamage { Amount = "1" }]
        }.AddToAffectable(actor);
        manager.Queue.ProcessQueue(manager);
        ability.Slot = 5;
        actor.AI!.NextAction = ActorActionType.UseAbilitySlot;
        actor.AI.NextActionTarget = 5;
        actor.AI.NextActionTarget2 = 0;
        actor.Position!.Heading = Direction.East;
        Assert.Contains(new Point(3, 2), room.CellThreats!.Keys);
        actor.Position.Cell = new Point(2, 3);
        Assert.DoesNotContain(new Point(3, 1), room.CellThreats!.Keys);
        actor.Position.Heading = Direction.West;
        Assert.Contains(new Point(1, 3), room.CellThreats!.Keys);
        Assert.DoesNotContain(new Point(3, 3), room.CellThreats!.Keys);
        ability.Range = 2;
        Assert.Contains(new Point(0, 3), room.CellThreats!.Keys);
        actor.Being!.HitPoints = 0;
        Assert.Empty(room.CellThreats!);
        room.Reset();
        Assert.Null(room.CellThreats);
    }

    [Fact]
    public void Plus_launcher_resolves_one_volley_and_stops_each_ray_at_the_first_actor()
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
        var manager = room.Entity.Manager;
        var players = new[] { new Point(1, 3), new Point(0, 3), new Point(3, 1), new Point(6, 3), new Point(3, 6), new Point(1, 1) }
            .Select((cell, index) => PlayerRace.InstantiatePlayer($"Player{index}", Sex.Male, room, cell)).ToArray();
        var definition = new Feature
        {
            Name = "test plus launcher", Behavior = FeatureBehavior.TriggerOnInterval,
            Abilities = [new Ability
            {
                Name = "test plus attack", Type = AbilityType.RangedAttack, TargetingShape = TargetingShape.Plus,
                Range = 4, Delay = "100", Windup = 200, Effects = [new FireDamage { Amount = "1" }]
            }]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var launcher = definition.Instantiate(room, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);
        var initial = players.Select(player => player.Being!.HitPoints).ToArray();
        manager.Game.CurrentTick = 300;

        manager.FeatureSystem.Advance(launcher, manager);
        manager.Queue.ProcessQueue(manager);

        foreach (var index in new[] { 0, 2, 3, 4 })
        {
            Assert.Equal(initial[index] - 1, players[index].Being!.HitPoints);
        }
        Assert.Equal(initial[1], players[1].Being!.HitPoints);
        Assert.Equal(initial[5], players[5].Being!.HitPoints);
        Assert.Equal(600, launcher.Feature!.NextActionTick);
    }

    [Theory]
    [InlineData("40 + 20", 40)]
    [InlineData("0", 100)]
    [InlineData(null, 100)]
    public void Timed_features_use_ability_delay_and_windup_for_each_deadline(string? delay, int windup)
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....
.....
.....");
        PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var definition = new Feature
        {
            Name = "test ability timed trap", Behavior = FeatureBehavior.TriggerOnInterval,
            Abilities = [new Ability
            {
                Name = "test timed attack", Type = AbilityType.RangedAttack, Range = 1,
                Delay = delay, Windup = windup, Effects = [new FireDamage { Amount = "1" }]
            }]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var entity = definition.Instantiate(room, new Point(2, 2));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(100, entity.Feature!.NextActionTick);

        Assert.Single(entity.ActiveEffects!.Abilities).Ability!.Windup += 25;
        room.Game.CurrentTick = 100;
        manager.FeatureSystem.Advance(entity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(225, entity.Feature.NextActionTick);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData(null, 0)]
    [InlineData("-1", 100)]
    [InlineData("100", -1)]
    public void Timed_features_reject_nonpositive_or_negative_ability_timing(string? delay, int windup)
    {
        var room = TestHelper.BuildRoom("...");
        var definition = new Feature
        {
            Name = "test invalid timed trap", Behavior = FeatureBehavior.TriggerOnInterval,
            Abilities = [new Ability { Name = "test invalid timed attack", Type = AbilityType.RangedAttack, Delay = delay, Windup = windup }]
        };

        var error = Assert.Throws<InvalidOperationException>(() => definition.Instantiate(room, new Point(1, 0)));

        Assert.Contains("Delay plus Windup", error.Message);
    }

    [Fact]
    public void Features_without_a_reward_type_do_not_create_rewards()
    {
        var room = TestHelper.BuildRoom("...");
        var definition = new Feature { Name = "test scenery" };
        var feature = definition.Instantiate(room, new Point(1, 0));

        Assert.Equal(0, (int)RewardType.None);
        Assert.Equal(RewardType.None, definition.RewardType);
        Assert.Same(definition, feature.Feature!.Template);
    }

    [Fact]
    public void Features_lookup_their_originating_template_and_refresh_it_when_renamed()
    {
        var room = TestHelper.BuildRoom("...");
        var original = new Feature { Name = "test original feature" };
        var replacement = new Feature { Name = "test replacement feature", Behavior = FeatureBehavior.None };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(replacement);
        var entity = original.Instantiate(room, new Point(1, 0));

        Assert.Same(original, entity.Feature!.Template);
        Assert.True(entity.Feature.BlocksMovement);
        entity.Feature.Name = original.Name;
        Assert.Same(original, entity.Feature.Template);
        entity.Feature.Name = replacement.Name;
        Assert.Same(replacement, entity.Feature.Template);
        Assert.False(entity.Feature.BlocksMovement);
    }

    [Fact]
    public void Features_use_all_eight_random_planar_headings_and_preserve_the_reward_seed()
    {
        var room = TestHelper.BuildRoom(new string('.', 64));
        room.Level.GenerationRandom.Seed = 4321;
        var headings = new HashSet<Direction>();
        for (byte x = 0; x < room.Width; x++)
        {
            var seed = room.Level.GenerationRandom.Seed;
            var feature = new Feature { Name = "test random heading" }.Instantiate(room, new Point(x, 0));

            Assert.InRange((int)feature.Position!.Heading!.Value, 0, Vector.MovementDirections.Length - 1);
            Assert.Equal(seed, feature.Feature!.RewardRandom.Seed);
            Assert.NotEqual(seed, room.Level.GenerationRandom.Seed);
            headings.Add(feature.Position.Heading.Value);
        }

        Assert.Equal(Vector.MovementDirections.Length, headings.Count);
    }

    [Theory]
    [InlineData(FeatureBehavior.None, false, false, null)]
    [InlineData(FeatureBehavior.BlocksMovement, true, false, null)]
    [InlineData(FeatureBehavior.AllowDestroy, false, true, null)]
    [InlineData(FeatureBehavior.TriggerOnEntry, false, false, null)]
    [InlineData(FeatureBehavior.TriggerOnInterval, false, false, 100)]
    [InlineData(FeatureBehavior.BlocksMovement | FeatureBehavior.AllowDestroy
        | FeatureBehavior.TriggerOnEntry | FeatureBehavior.TriggerOnInterval, true, true, 100)]
    public void Behavior_flags_control_collision_rewards_and_scheduling(
        FeatureBehavior behavior, bool blocksMovement, bool allowDestroy, int? nextActionTick)
    {
        var room = TestHelper.BuildRoom("...");
        var definition = new Feature
        {
            Name = "test feature behavior", Behavior = behavior, RewardType = RewardType.Ritual,
            Abilities = [new Ability { Name = "test feature behavior attack", Type = AbilityType.MeleeAttack, Delay = "60", Windup = 40 }]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var entity = definition.Instantiate(room, new Point(1, 0));
        room.Entity.Manager.Queue.ProcessQueue(room.Entity.Manager);

        Assert.Equal(blocksMovement, entity.Feature!.BlocksMovement);
        Assert.Same(definition, entity.Feature.Template);
        Assert.Equal(allowDestroy ? false : (bool?)null, entity.Feature.Destroyed);
        Assert.Equal(nextActionTick, entity.Feature.NextActionTick);
    }

    [Fact]
    public void Attack_selection_uses_available_usable_abilities_and_skips_cooldowns()
    {
        var room = TestHelper.BuildRoom("...");
        var feature = FeatureData.SpikeTrap.Instantiate(room, new Point(1, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var original = Assert.Single(feature.ActiveEffects!.Abilities).Ability!;
        Assert.Same(original, manager.FeatureSystem.SelectAttack(feature));
        var alternative = new Ability
        {
            Name = "test alternative attack", Type = AbilityType.RangedAttack, Effects = []
        }.AddToAffectable(feature);
        manager.Queue.ProcessQueue(manager);
        original.CooldownTick = 100;
        Assert.Same(alternative, manager.FeatureSystem.SelectAttack(feature, tick: 0));
        Assert.Same(original, manager.FeatureSystem.SelectAttack(feature, tick: 100));
        alternative.CooldownXpLeft = 1;
        Assert.Null(manager.FeatureSystem.SelectAttack(feature, tick: 0));
        alternative.CooldownXpLeft = null;
        alternative.IsUsable = false;
        Assert.Null(manager.FeatureSystem.SelectAttack(feature, tick: 0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Entry_trigger_uses_the_selected_ability_range_shape_and_shape_size(bool forced)
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....
.....
.....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(2, 0));
        var manager = room.Entity.Manager;
        var definition = new Feature
        {
            Name = "test alternate geometry trap", Behavior = FeatureBehavior.TriggerOnEntry,
            Abilities =
            [
                new Ability
                {
                    Name = "test small entry attack", Type = AbilityType.MeleeAttack,
                    TargetingShape = TargetingShape.Square, Range = 0, Cooldown = 100,
                    Effects = [new PhysicalDamage { Amount = "1" }]
                },
                new Ability
                {
                    Name = "test cone entry attack", Type = AbilityType.MeleeAttack,
                    TargetingShape = TargetingShape.Cone, TargetingShapeSize = 2, Range = 1, Cooldown = 100,
                    Effects = [new PhysicalDamage { Amount = "2" }]
                }
            ]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);
        var abilities = feature.ActiveEffects!.Abilities;
        Assert.Equal([new Point(2, 2)], GetTriggerCells(feature));
        abilities[0].Ability!.CooldownTick = 100;
        Assert.Same(abilities[1].Ability, manager.FeatureSystem.SelectAttack(feature));
        var cells = GetTriggerCells(feature);
        Assert.Equal(5, cells.Count);
        Assert.Contains(new Point(2, 1), cells);
        Assert.DoesNotContain(new Point(1, 2), cells);
        var initialHp = player.Being!.HitPoints;
        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = player;
        travel.TargetCell = new Point(2, 1);
        travel.TargetHeading = Direction.South;
        travel.Forced = forced;
        manager.Process(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.True(player.Being.HitPoints < initialHp);
        Assert.Equal(100, abilities[0].Ability!.CooldownTick);
        Assert.Empty(GetTriggerCells(feature));
    }

    [Theory]
    [InlineData(TargetingShape.Square, 9, true)]
    [InlineData(TargetingShape.SquareNoCenter, 8, true)]
    [InlineData(TargetingShape.Plus, 5, false)]
    [InlineData(TargetingShape.PlusNoCenter, 4, false)]
    [InlineData(TargetingShape.Line, 1, false)]
    [InlineData(TargetingShape.Cone, 3, false)]
    public void Trigger_shapes_control_entry_damage_and_the_published_footprint(TargetingShape shape, int count, bool diagonalTriggers)
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....
.....
.....");
        var manager = room.Entity.Manager;
        var definition = new Feature
        {
            Name = "test shaped trap", Behavior = FeatureBehavior.TriggerOnEntry,
            Abilities = [new Ability
            {
                Name = "test shaped attack", Type = AbilityType.MeleeAttack, TargetingShape = shape, Range = 1,
                Cooldown = 100, Effects = [new PhysicalDamage { Amount = "15", ArmorPenetration = "2" }]
            }]
        };
        ((TestDataProvider)room.Game.Services.Data).Features.Add(definition);
        var feature = definition.Instantiate(room, new Point(2, 2));
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(count, GetTriggerCells(feature).Count);
        Assert.Equal(diagonalTriggers, room.CellThreats!.ContainsKey(new Point(1, 1)));
        var initialHp = player.Being!.HitPoints;
        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = player;
        travel.TargetCell = new Point(1, 1);
        travel.TargetHeading = Direction.Southeast;
        travel.Forced = true;
        manager.Process(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(diagonalTriggers, player.Being.HitPoints < initialHp);
        Assert.Equal(diagonalTriggers, room.CellThreats!.ContainsKey(new Point(1, 1)));
    }

    [Fact]
    public void Feature_rescheduling_updates_existing_threats_before_another_query()
    {
        var room = TestHelper.BuildRoom("""
            .....
            .....
            .....
            .....
            .....
            """);
        PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(2, 1));
        var manager = room.Entity.Manager;
        var definition = new Feature
        {
            Name = "test rescheduled forecast", Behavior = FeatureBehavior.TriggerOnInterval,
            Abilities =
            [
                new Ability
                {
                    Name = "test cooling primary", Type = AbilityType.RangedAttack,
                    TargetingShape = TargetingShape.Plus, Range = 0, Delay = "100", Cooldown = 300,
                    Effects = [new PhysicalDamage { Amount = "1" }]
                },
                new Ability
                {
                    Name = "test cooling alternate", Type = AbilityType.RangedAttack,
                    TargetingShape = TargetingShape.Plus, Range = 1, Delay = "100", Cooldown = 300,
                    Effects = [new PhysicalDamage { Amount = "1" }]
                }
            ]
        };
        var feature = definition.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);
        feature.ActiveEffects!.Abilities[0].Ability!.CooldownTick = 400;
        var threats = room.CellThreats!;
        Assert.Contains(feature.Id, threats[new Point(2, 1)]);

        manager.Game.CurrentTick = 100;
        manager.FeatureSystem.Advance(feature, manager);

        Assert.Equal(200, feature.Feature!.NextActionTick);
        Assert.Empty(threats);
        Assert.Null(feature.Feature.ThreatenedCells);
    }

    [Fact]
    public void Entry_activation_updates_every_overlapping_source_and_existing_threats()
    {
        var room = TestHelper.BuildRoom("""
            ....
            ....
            ....
            """);
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        var spike = FeatureData.SpikeTrap.Instantiate(room, new Point(1, 1));
        var definition = new Feature
        {
            Name = "test overlapping entry", Behavior = FeatureBehavior.TriggerOnEntry,
            Abilities = [new Ability
            {
                Name = "test overlapping entry attack", Type = AbilityType.MeleeAttack,
                TargetingShape = TargetingShape.Square, Range = 1, Cooldown = 100,
                Effects = [new PhysicalDamage { Amount = "1" }]
            }]
        };
        var feature = definition.Instantiate(room, new Point(2, 1));
        manager.Queue.ProcessQueue(manager);
        var cell = new Point(1, 1);
        var threats = room.CellThreats!;
        Assert.Equal(2, threats[cell].Count);
        var sources = threats[cell];
        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = player;
        travel.TargetCell = cell;
        travel.TargetHeading = Direction.Southeast;
        manager.Process(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(spike.ActiveEffects!.Abilities[0].Ability!.CooldownTick);
        Assert.NotNull(feature.ActiveEffects!.Abilities[0].Ability!.CooldownTick);
        Assert.Same(sources, threats[cell]);
        Assert.Equal(2, sources.Count);
    }

    [Fact]
    public void Actor_movement_and_schedule_changes_keep_the_cached_hazard_geometry()
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var launcher = FeatureData.FireboltLauncher.Instantiate(room, new Point(2, 1));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        var original = room.CellThreats!;
        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = player;
        travel.TargetCell = new Point(1, 0);
        travel.TargetHeading = Direction.East;
        manager.Process(travel);
        manager.Queue.ProcessQueue(manager);
        launcher.Feature!.NextActionTick = 600;

        Assert.Same(original, room.CellThreats);
        TerrainSystem.SetTerrain(MapFeature.StoneWall, new Point(3, 1), room);
        Assert.Same(original, room.CellThreats);
    }

    [Fact]
    public void Authored_rewards_move_to_the_nearest_cell_that_preserves_connectivity()
    {
        var room = TestHelper.BuildRoom(@"
.....
##.##
.....");
        var original = new Point(2, 1);
        var feature = FeatureGenerator.Place(room, original, RoomFeatures.SmallFountain);

        Assert.Equal(1, original.DistanceTo(feature.Position!.Cell!.Value));
        Assert.True(room.Entity.Manager.TravelSystem.CanMoveTo(original, room));
        Assert.NotNull(room.Entity.Manager.TravelSystem.GetShortestPath(room,
            new Point(0, 0), new Point(0, 2), Direction.East));
    }

    [Fact]
    public void Rewards_keep_the_authored_cell_when_it_does_not_block_a_route()
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....");
        var feature = FeatureGenerator.Place(room, new Point(2, 1), RoomFeatures.SmallFountain);
        Assert.Equal(new Point(2, 1), feature.Position!.Cell);
    }

    [Fact]
    public void AI_danger_includes_lanes_that_would_be_exposed_when_a_creature_moves()
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....
.....
.....");
        PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(1, 2));
        var launcher = FeatureData.FireboltLauncher.Instantiate(room, new Point(2, 2));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);

        Assert.Contains(new Point(0, 2), room.CellThreats!.Keys);
        var attackCells = new List<Point>();
        manager.FeatureSystem.GetAttackCells(launcher, attackCells, static (target, cell) => target.Add(cell));
        Assert.DoesNotContain(new Point(0, 2), attackCells);
    }

    [Fact]
    public void Authored_traps_do_not_turn_a_narrow_passage_into_a_solid_barrier()
    {
        for (uint seed = 1; seed <= 8; seed++)
        {
            var room = TestHelper.BuildRoom(".^.", seed: seed);
            var feature = Assert.Single(room.Features).Value;
            Assert.False(feature.Feature!.BlocksMovement);
            Assert.NotNull(room.Entity.Manager.TravelSystem.GetShortestPath(room,
                new Point(0, 0), new Point(2, 0), Direction.East));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Launchers_skip_unattended_processing_resume_and_stop_when_destroyed(bool invalidUnattendedTiming)
    {
        var room = TestHelper.BuildRoom("""
            ...
            ...
            ...
            """);
        var other = TestHelper.BuildRoom("""
            ...
            ...
            ...
            """, room.Game);
        var manager = room.Entity.Manager;
        var launcher = FeatureData.FireboltLauncher.Instantiate(room, new Point(1, 1));
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, other, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var attack = Assert.Single(launcher.ActiveEffects!.Abilities).Ability!;
        var originalWindup = attack.Windup;
        if (invalidUnattendedTiming)
        {
            attack.Windup = -1;
            attack.CooldownTick = 700;
        }
        player.Player!.NextActionTick = 1000;
        Assert.Equal(300, TimeSystem.GetTick(launcher));
        Assert.Same(launcher, manager.TemporalEntitiesIndex.First());

        Assert.Equal(TurnResult.Continue, manager.TimeSystem.AdvanceSingleTurn(manager));

        Assert.Equal(300, manager.Game.CurrentTick);
        Assert.Equal(300 + TimeSystem.DefaultActionDelay, launcher.Feature!.NextActionTick);
        Assert.Equal(invalidUnattendedTiming ? 700 : (int?)null, attack.CooldownTick);
        attack.Windup = originalWindup;
        attack.CooldownTick = null;
        player.Position!.RoomEntity = room.Entity;
        Assert.Equal(400, launcher.Feature.NextActionTick);

        Assert.Equal(TurnResult.Continue, manager.TimeSystem.AdvanceSingleTurn(manager));

        Assert.Equal(400, manager.Game.CurrentTick);
        Assert.Equal(700, launcher.Feature.NextActionTick);
        Assert.Same(launcher, manager.TemporalEntitiesIndex[(700, (int)EntityComponent.Feature, launcher.Id)]);

        launcher.Feature!.Destroyed = true;

        Assert.Null(launcher.Feature.NextActionTick);
        Assert.Null(TimeSystem.GetTick(launcher));
        Assert.Null(manager.TemporalEntitiesIndex[(700, (int)EntityComponent.Feature, launcher.Id)]);
    }

    [Fact]
    public void Launcher_danger_uses_cardinal_attack_lines_and_respects_walls()
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....
.....
.....");
        var manager = room.Entity.Manager;
        FeatureData.FireboltLauncher.Instantiate(room, new Point(2, 2));
        TerrainSystem.SetTerrain(MapFeature.StoneWall, new Point(3, 2), room);
        manager.Queue.ProcessQueue(manager);
        var danger = room.CellThreats!;

        Assert.Contains(new Point(1, 2), danger.Keys);
        Assert.Contains(new Point(2, 1), danger.Keys);
        Assert.DoesNotContain(new Point(4, 2), danger.Keys);
        Assert.DoesNotContain(new Point(0, 0), danger.Keys);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Spike_traps_trigger_on_entry_and_respect_the_ability_cooldown(bool forced)
    {
        var room = TestHelper.BuildRoom("....");
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        var trap = FeatureData.SpikeTrap.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);
        var initialHp = player.Being!.HitPoints;

        Move(new Point(1, 0));
        var firstHp = player.Being.HitPoints;
        Assert.True(firstHp < initialHp);
        Assert.Null(trap.Feature!.NextActionTick);
        Move(new Point(0, 0));
        Move(new Point(1, 0));
        Assert.Equal(firstHp, player.Being.HitPoints);
        Assert.Equal(TurnResult.Continue, manager.TimeSystem.AdvanceSingleTurn(manager));
        Assert.Equal(100, manager.Game.CurrentTick);
        Assert.Contains(trap.Id, room.CellThreats![new Point(1, 0)]);
        Move(new Point(0, 0));
        Move(new Point(1, 0));
        Assert.True(player.Being.HitPoints < firstHp);

        void Move(Point cell)
        {
            var travel = TravelMessage.Create(manager);
            travel.ActorEntity = player;
            travel.TargetCell = cell;
            travel.TargetHeading = cell.X == 0 ? Direction.West : Direction.East;
            travel.Forced = forced;
            manager.Process(travel);
            manager.Queue.ProcessQueue(manager);
        }
    }

    [Fact]
    public void Launchers_fire_cardinal_bolts_at_their_action_tick_and_reschedule_once()
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....
.....
.....");
        var manager = room.Entity.Manager;
        var players = new[] { new Point(0, 2), new Point(4, 2), new Point(2, 0), new Point(2, 4), new Point(0, 0) }
            .Select((cell, index) => PlayerRace.InstantiatePlayer($"Player{index}", Sex.Male, room, cell)).ToArray();
        var launcher = FeatureData.FireboltLauncher.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);
        foreach (var player in players)
        {
            player.Player!.NextActionTick = 1000;
        }

        var initial = players.Select(player => player.Being!.HitPoints).ToArray();
        Assert.Equal(300, launcher.Feature!.NextActionTick);
        Assert.Equal(TurnResult.Continue, manager.TimeSystem.AdvanceSingleTurn(manager));

        Assert.Equal(300, manager.Game.CurrentTick);
        Assert.Equal(600, launcher.Feature.NextActionTick);
        for (var index = 0; index < 4; index++)
        {
            Assert.True(players[index].Being!.HitPoints < initial[index]);
        }

        Assert.Equal(initial[4], players[4].Being!.HitPoints);
        Assert.Null(launcher.Being);
        Assert.Null(launcher.AI);
    }
}