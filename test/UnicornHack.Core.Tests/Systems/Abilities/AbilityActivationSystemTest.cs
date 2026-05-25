using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class AbilityActivationSystemTest
{
    private static readonly string TestMap = @"
.#.
.#.
...";

    [Fact]
    public void Can_use_projectile_abilities_on_targets_in_LOS()
    {
        var level = TestHelper.BuildRoom(TestMap);

        var manager = level.Entity.Manager;
        var listener = new AbilityActivatedListener();
        manager.Queue.Register(listener, AbilityActivatedMessage.Name, -1);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 0));
        playerEntity.Position!.Heading = Direction.West;
        playerEntity.Player!.SkillPoints = 5;
        manager.SkillAbilitiesSystem.BuyAbilityLevel(AbilityData.Conjuration, playerEntity);

        manager.Queue.ProcessQueue(manager);

        ItemData.Shortbow.Instantiate(playerEntity);
        var undine = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        var sylph = CreatureData.Sylph.Instantiate(level, new Point(0, 0));

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100, playerEntity.Being!.Accuracy);
        Assert.Equal(100, playerEntity.Being.Evasion);
        Assert.Equal(50, sylph.Being!.Accuracy);
        Assert.Equal(75, sylph.Being.Evasion);

        var bow = playerEntity.Being.Items.Single();
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = bow;
        equipMessage.Slot = EquipmentSlot.GraspBothRanged;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        var nymphAbility = undine.Being!.Abilities
            .First(a => (a.Ability!.Activation & ActivationType.Slottable) != 0
                        && a.Ability.Template?.Type != AbilityType.DefaultAttack);
        Assert.Equal(2, nymphAbility.Ability!.Slot);

        var attackAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.TwoHandedRangedAttack.Name)]!;

        var activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = undine.Position!.Cell;
        activateAbilityMessage.AbilityEntity = attackAbility;

        manager.Enqueue(activateAbilityMessage);

        var messageCount = 0;
        listener.ProcessMessage = m =>
        {
            if (m.TargetEntity == null)
            {
                return;
            }

            messageCount++;
            Assert.Same(playerEntity, m.ActivatorEntity);
            Assert.Same(undine, m.TargetEntity);
            Assert.Equal(new Point(2, 2), m.TargetCell);
        };

        Assert.Equal(5, playerEntity.Player.NextActionTick);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, messageCount);
        Assert.Equal(155, playerEntity.Player.NextActionTick);

        activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = sylph.Position!.Cell;
        activateAbilityMessage.AbilityEntity = attackAbility;

        manager.Enqueue(activateAbilityMessage);

        messageCount = 0;
        listener.ProcessMessage = m =>
        {
            if (m.TargetEntity == null)
            {
                return;
            }

            messageCount++;
        };

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, messageCount);

        undine.Being.HitPoints = undine.Being.HitPointMaximum;
        var iceShardAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!;

        var accuracy = manager.AbilityActivationSystem.GetAccuracy(iceShardAbility.Ability!, playerEntity);
        Assert.Equal(105, accuracy);

        Assert.Equal(21,
            manager.AbilityActivationSystem.GetEvasionProbability(iceShardAbility, playerEntity, undine, null,
                accuracy));

        Assert.Equal(100,
            manager.AbilityActivationSystem.GetEvasionProbability(iceShardAbility, playerEntity, sylph, null,
                accuracy));

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = iceShardAbility;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);

        activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = undine.Position.Cell;
        activateAbilityMessage.AbilityEntity = iceShardAbility;
        manager.Enqueue(activateAbilityMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, messageCount);
        Assert.Equal(1100, iceShardAbility.Ability!.CooldownTick);

        activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = undine.Position.Cell;
        activateAbilityMessage.AbilityEntity = iceShardAbility;
        manager.Enqueue(activateAbilityMessage);

        Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));

        Assert.Equal(1, messageCount);
        Assert.Equal(1100, iceShardAbility.Ability.CooldownTick);
    }

    [Fact]
    public void Can_use_toggleable_abilities()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        manager.XPSystem.AddPlayerXP(player.NextLevelXP * 3, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, playerEntity.Being!.ReservedEnergyPoints);
        var toggledAbility = playerEntity.Being.Abilities
            .Single(a => (a.Ability!.Activation & ActivationType.WhileToggled) != 0);
        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = toggledAbility;
        setSlotMessage.Slot = 2;

        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.False(toggledAbility.Ability!.IsActive);
        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        Assert.True(toggledAbility.Ability.IsActive);
        Assert.Equal(2, toggledAbility.Ability.Slot);
        Assert.Null(toggledAbility.Ability.CooldownTick);
        Assert.Equal(50, playerEntity.Being.ReservedEnergyPoints);

        setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = toggledAbility;

        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.False(toggledAbility.Ability.IsActive);
        Assert.Equal(2, toggledAbility.Ability.Slot);
        Assert.Equal(200, toggledAbility.Ability.CooldownTick);
        Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);

        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(toggledAbility.Ability.CooldownTick);
        Assert.Equal(200, manager.Game.CurrentTick);

        setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = toggledAbility;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(toggledAbility.Ability.Slot);

        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        Assert.True(toggledAbility.Ability.IsActive);
        Assert.Equal(2, toggledAbility.Ability.Slot);
        Assert.Null(toggledAbility.Ability.CooldownTick);
        Assert.Equal(50, playerEntity.Being.ReservedEnergyPoints);
    }

    [Fact]
    public void Targeting_line_pattern1_diagonal() => TestTargeting(@"
<#.......#
#.#......#
.#.#.....#
....#....#
.........#
....#....#
.....#2..#",
        new byte[]
        {
            0, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 76, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 140, 254, 0, 0, 0, 0, 0, 0,
            0, 0, 22, 190, 254, 0, 0, 0, 0, 0, 0, 0, 0, 43, 183, 45, 0, 0, 0, 0, 0, 0, 0, 0, 254, 181, 68, 0, 0, 0,
            0, 0, 0, 0, 0, 254, 189, 0, 0, 0,
        },
        6,
        TargetingShape.Line,
        1,
        Direction.West);

    [Fact]
    public void Targeting_line_pattern1_diagonal_near() => TestTargeting(@"
<#.......#
#2#......#
.#.#.....#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 190, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 158, 254, 0, 0, 0, 0, 0, 0,
            0, 0, 66, 198, 254, 0, 0, 0, 0, 0, 0, 0, 0, 127, 198, 45, 0, 0, 0, 0, 0, 0, 0, 2, 254, 181, 68, 0, 0, 0,
            0, 0, 0, 0, 0, 254, 189, 0, 0, 0,
        },
        6,
        TargetingShape.Line,
        1,
        Direction.West);

    [Fact]
    public void Targeting_line_pattern1_diagonal_self() => TestTargeting(@"
<#.......#
#.#......#
.#.#.....#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 190, 254, 0, 0, 0, 0, 0, 0, 0, 0, 254, 158, 254, 0, 0, 0, 0, 0, 0,
            0, 0, 66, 198, 254, 0, 0, 0, 0, 0, 0, 0, 0, 127, 198, 45, 0, 0, 0, 0, 0, 0, 0, 2, 254, 181, 68, 0, 0, 0,
            0, 0, 0, 0, 0, 254, 189, 0, 0, 0,
        },
        6,
        TargetingShape.Line,
        1,
        Direction.Southeast,
        targetGlyph: '<');

    [Fact]
    public void Targeting_line_pattern2_diagonal_near() => TestTargeting(@"
<........#
.2.......#
..#......#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0, 42, 0, 0, 0, 0, 0, 0, 0, 0, 42, 254, 211, 127, 42, 0, 0, 0, 0, 0, 0, 211, 254, 187, 254, 211, 126, 0,
            0, 0, 0, 127, 187, 4, 254, 158, 253, 0, 0, 0, 0, 42, 254, 127, 0, 0, 64, 0, 0, 0, 0, 0, 211, 251, 254,
            0, 0, 0, 0, 0, 0, 0, 126, 254, 177, 254, 0, 0, 0, 0,
        },
        6,
        TargetingShape.Line,
        1,
        Direction.West);


    [Fact]
    public void Targeting_line_pattern2_vertical_self() => TestTargeting(@"
...<.....#
.........#
..#......#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 127, 254, 127, 0, 0, 0, 0, 0, 0, 79, 254, 254, 254, 127, 0, 0, 0, 0,
            124, 66, 63, 254, 254, 222, 127, 0, 0, 0, 127, 0, 127, 254, 42, 127, 254, 127, 0, 0, 2, 0, 190, 254,
            254, 15, 238, 254, 127, 0, 0, 7, 246, 254, 25, 0, 127, 254, 254, 254,
        },
        6,
        TargetingShape.Line,
        1,
        Direction.South,
        targetGlyph: '<');

    [Fact]
    public void Targeting_line_pattern2_vertical_near() => TestTargeting(@"
..<......#
..2......#
..#......#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 127, 254, 127, 0, 0, 0, 0, 0, 0, 127, 238, 254, 238, 127, 0, 0, 0, 0,
            0, 254, 190, 0, 190, 254, 74, 0, 0, 0, 0, 254, 127, 0, 127, 127, 51, 117, 0, 0, 0, 254, 63, 0, 63, 254,
            0, 85, 125, 0, 0, 246, 7, 0, 7, 68, 0, 0, 127, 127, 0,
        },
        6,
        TargetingShape.Line,
        1,
        Direction.North);

    private static void TestTargeting(
        string map,
        byte[] expectedExposure,
        int range,
        TargetingShape targetingShape,
        int targetShapeSize,
        Direction heading,
        char targetGlyph = '2')
    {
        var (level, _) = TestHelper.BuildRoomWithChunks(map);
        var origin = level.Connections.Single().Key;
        var target = targetGlyph == '<' ? origin : FindTargetPoint(map, targetGlyph, origin);

        var manager = level.Entity.Manager;
        var abilityActivationSystem = manager.AbilityActivationSystem;
        var targetedCells = abilityActivationSystem.GetTargetedCells(
            origin, target, heading, range, targetingShape, targetShapeSize, level);

        var visibleTerrain = new byte[level.TileCount];
        var originDistance = 0;
        foreach (var (point, exposure) in targetedCells)
        {
            visibleTerrain[level.PointToIndex![point.X, point.Y]] += exposure;
            var distance = origin.DistanceTo(point);
            Assert.False(distance < originDistance, $"Point {point} is out of sequence");

            originDistance = distance;
        }

        ((List<(Point, byte)>)targetedCells).Clear();
        manager.PointByteListArrayPool.Return((List<(Point, byte)>)targetedCells);
        TestHelper.AssertVisibility(level, expectedExposure, visibleTerrain);
    }

    /// <summary>
    ///     Locates <paramref name="glyph" /> in the raw test map and converts its
    ///     position to a world-space <see cref="Point" />. The fragment may be placed
    ///     at a non-zero offset within the level, so the map→world translation is
    ///     derived from the '&lt;' anchor in the map literal compared to the supplied
    ///     <paramref name="origin" /> (the connection's world position).
    /// </summary>
    private static Point FindTargetPoint(string map, char glyph, Point origin)
    {
        var lines = map.Split('\n');
        Point? originMap = null;
        Point? targetMap = null;
        for (var row = 0; row < lines.Length && (originMap is null || targetMap is null); row++)
        {
            var line = lines[row];
            for (var col = 0; col < line.Length; col++)
            {
                var c = line[col];
                if (originMap is null && c == '<')
                {
                    originMap = new Point((byte)col, (byte)row);
                }
                else if (targetMap is null && c == glyph)
                {
                    targetMap = new Point((byte)col, (byte)row);
                }
            }
        }

        if (targetMap is null)
        {
            throw new ArgumentException($"Glyph '{glyph}' not found in map.", nameof(glyph));
        }

        if (originMap is null)
        {
            throw new ArgumentException("Origin glyph '<' not found in map.", nameof(map));
        }

        return new Point(
            (byte)(origin.X + targetMap.Value.X - originMap.Value.X),
            (byte)(origin.Y + targetMap.Value.Y - originMap.Value.Y));
    }

    private class AbilityActivatedListener : IGameSystem<AbilityActivatedMessage>
    {
        public Action<AbilityActivatedMessage>? ProcessMessage
        {
            get;
            set;
        }

        public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager state)
        {
            ProcessMessage?.Invoke(message);

            return MessageProcessingResult.ContinueProcessing;
        }
    }
}
