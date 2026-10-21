using UnicornHack.Data.Branches;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Generation.Map;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack.Persistence;

public class RoomPersistenceTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Production_layouts_and_next_floor_generation_repeat_after_reload(bool useAsync)
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave(seed: 17);
        var branches = ((TestDataProvider)persisted.Game.Services.Data).Branches;
        foreach (var template in branches.TestItems)
        {
            template.GenerationWeight = "0";
        }

        var wards = BranchData.OuterWards.Instantiate(persisted.Game);
        BranchData.RuinedStronghold.Instantiate(persisted.Game);
        var level = LevelGenerator.CreateEmpty(wards, 1, 17, persisted.Game.Manager);
        LevelGenerator.EnsureGenerated(level);
        var levelBosses = new HashSet<string>();

        for (var floor = 0; floor < 4; floor++)
        {
            var player = persisted.PlayerEntity;
            var entrance = Assert.Single(level.Rooms, room => room.FeatureTags.HasFlag(RoomFeatures.Entrance));
            player.Position!.RoomEntity = entrance.Entity;
            player.Position.Cell = entrance.Connections.Values.First().Position!.Cell;
            persisted.Game.Manager.Queue.ProcessQueue(persisted.Game.Manager);

            var bosses = level.Rooms.SelectMany(room => room.Actors.Values)
                .Where(actor => Creature.GetTemplate(actor)?.IsBoss == true).ToArray();
            if (floor == 0)
            {
                var boss = Assert.Single(bosses);
                Assert.Equal("Twrch Trwyth", Creature.GetTemplate(boss)!.Name);
                Assert.Equal(1, persisted.Game.GenerationStats.CreatureInstancesByName["Twrch Trwyth"]);
            }
            else
            {
                var boss = Assert.Single(bosses);
                var template = Creature.GetTemplate(boss)!;
                Assert.Equal(1, persisted.Game.GenerationStats.CreatureInstancesByName[template.Name]);
                Assert.Equal(template.XP != 0 ? template.XP : 100 + template.InitialLevel * 10, boss.Being!.ExperiencePoints);
                if (floor < 3)
                {
                    Assert.True(template.GenerationFlags.HasFlag(CreatureGenerationFlags.LevelBoss));
                    Assert.True(levelBosses.Add(template.Name));
                }
                else
                {
                    Assert.True(template.GenerationFlags.HasFlag(CreatureGenerationFlags.BranchBoss));
                    Assert.Contains(template.Name, new[] { "Balor ua Neit", "Rubezahl" });
                }

                var initialPlayerXp = player.Being!.ExperiencePoints;
                var initialReward = boss.Being.ExperiencePoints;
                var adjustment = new Ability
                {
                    Name = "test boss reward adjustment " + floor,
                    Activation = ActivationType.Manual,
                    Effects = [new GainXP { Amount = "-25" }]
                }.AddToAffectable(player);
                persisted.Game.Manager.Queue.ProcessQueue(persisted.Game.Manager);
                TestHelper.ApplyEffect(Assert.Single(adjustment.Effects), player, boss, persisted.Game.Manager);
                persisted.Game.Manager.Queue.ProcessQueue(persisted.Game.Manager);
                Assert.Equal(initialReward - 25, boss.Being.ExperiencePoints);
                Assert.Equal(initialPlayerXp, player.Being.ExperiencePoints);
            }

            var expected = TestHelper.SnapshotLevelLayout(level);
            var expectedPopulation = SnapshotPopulation(level);
            var onward = level.Rooms.Single(room => room.FeatureTags.HasFlag(RoomFeatures.Exit))
                .Connections.Values.SingleOrDefault(connection => connection.Connection!.Orientation == Direction.Down);
            var nextLevel = onward?.Connection!.TargetRoomEntity.Room!.Level;
            var nextSeed = nextLevel?.GenerationRandom.Seed;
            persisted.Context.SaveChanges();

            string? expectedNext = null;
            string? expectedNextPopulation = null;
            if (nextLevel != null)
            {
                Assert.True(LevelGenerator.EnsureGenerated(nextLevel));
                expectedNext = TestHelper.SnapshotLevelLayout(nextLevel);
                expectedNextPopulation = SnapshotPopulation(nextLevel);
            }

            if (useAsync)
            {
                await persisted.ReloadAsync();
            }
            else
            {
                persisted.Reload();
            }

            level = persisted.PlayerEntity.Position!.RoomEntity.Room!.Level;
            Assert.Equal(expected, TestHelper.SnapshotLevelLayout(level));
            Assert.Equal(expectedPopulation, SnapshotPopulation(level));
            Assert.False(LevelGenerator.EnsureGenerated(level));
            Assert.InRange(level.Rooms.Count, floor == 0 ? 10 : 28, floor == 0 ? 14 : 32);
            onward = level.Rooms.Single(room => room.FeatureTags.HasFlag(RoomFeatures.Exit))
                .Connections.Values.SingleOrDefault(connection => connection.Connection!.Orientation == Direction.Down);
            if (floor == 3)
            {
                Assert.Null(onward);
                Assert.Null(expectedNext);
                break;
            }

            Assert.NotNull(onward);
            nextLevel = onward.Connection!.TargetRoomEntity.Room!.Level;
            Assert.Equal(nextSeed, nextLevel.GenerationRandom.Seed);
            Assert.All(nextLevel.Rooms, room => Assert.Equal(0, room.Width));
            Assert.True(LevelGenerator.EnsureGenerated(nextLevel));
            Assert.Equal(expectedNext, TestHelper.SnapshotLevelLayout(nextLevel));
            Assert.Equal(expectedNextPopulation, SnapshotPopulation(nextLevel));
            level = nextLevel;
        }

        Assert.Equal(2, levelBosses.Count);
    }

    private static string SnapshotPopulation(GameLevel level)
        => System.Text.Json.JsonSerializer.Serialize(level.Rooms
            .OrderBy(room => room.GridX).ThenBy(room => room.GridY)
            .Select(room => new
            {
                room.GridX,
                room.GridY,
                Actors = room.Actors.Where(entry => entry.Value.Player == null)
                    .OrderBy(entry => entry.Key.X).ThenBy(entry => entry.Key.Y).Select(entry => new
                {
                    Cell = entry.Key,
                    entry.Value.Id,
                    Name = Creature.GetTemplate(entry.Value)!.Name,
                    entry.Value.Being!.ExperiencePoints
                }),
                Items = room.Items.OrderBy(entry => entry.Key.X).ThenBy(entry => entry.Key.Y).Select(entry => new
                {
                    Cell = entry.Key,
                    entry.Value.Id,
                    entry.Value.Item!.TemplateName,
                    entry.Value.Item.Count
                })
            }));

    [Fact]
    public async Task Visited_source_rooms_remain_loaded_after_multiple_room_transitions()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = persisted.Game.Manager;
        var firstRoom = player.Position!.RoomEntity.Room!;
        var secondRoom = TestHelper.BuildRoom("...", persisted.Game);
        var thirdRoom = TestHelper.BuildRoom("...", persisted.Game);
        secondRoom.Level = firstRoom.Level;
        thirdRoom.Level = firstRoom.Level;
        AddConnection(firstRoom, secondRoom, new Point(4, 0), new Point(0, 0));
        AddConnection(secondRoom, thirdRoom, new Point(1, 0), new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.TravelSystem.TravelToRoom(player, secondRoom.EntityId, manager));
        manager.Queue.ProcessQueue(manager);
        Assert.True(manager.TravelSystem.TravelToRoom(player, thirdRoom.EntityId, manager));
        manager.Queue.ProcessQueue(manager);
        persisted.Context.SaveChanges();

        var expectedRoomIds = new[]
        {
            firstRoom.EntityId,
            secondRoom.EntityId,
            thirdRoom.EntityId
        };
        Assert.All(
            firstRoom.Level.Rooms.Where(room => expectedRoomIds.Contains(room.EntityId)),
            room => Assert.True(room.IsVisited));

        await persisted.ReloadAsync();

        var reloadedLevel = persisted.PlayerEntity.Position!.RoomEntity.Room!.Level;
        Assert.Equal(
            expectedRoomIds.Order(),
            reloadedLevel.Rooms.Select(room => room.EntityId).Where(expectedRoomIds.Contains).Order());
        Assert.All(
            reloadedLevel.Rooms.Where(room => expectedRoomIds.Contains(room.EntityId)),
            room => Assert.True(room.IsVisited));
        var levelMap = GameQueryProcessor.SerializeLevelMap(persisted.PlayerEntity);
        Assert.Equal(
            expectedRoomIds.Order(),
            levelMap.Rooms.Select(room => room.Id).Where(expectedRoomIds.Contains).Order());
    }

    private static void AddConnection(
        RoomComponent source,
        RoomComponent target,
        Point sourceCell,
        Point targetCell)
        => LevelConnection.CreateConnection(source, sourceCell, target.EntityId, targetCell,
            Directionality.Source, Direction.East);
}
