using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;
using Xunit;

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
        var level = TestHelper.BuildLevel(TestMap);

        var manager = level.Entity.Manager;
        var listener = new AbilityActivatedListener();
        manager.Queue.Register(listener, AbilityActivatedMessage.Name, -1);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 0));
        playerEntity.Position.Heading = Direction.West;
        playerEntity.Player.SkillPoints = 5;
        manager.SkillAbilitiesSystem.BuyAbilityLevel(AbilityData.Conjuration, playerEntity);

        manager.Queue.ProcessQueue(manager);

        ItemData.Shortbow.Instantiate(playerEntity);
        var undine = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        var sylph = CreatureData.Sylph.Instantiate(level, new Point(0, 0));

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100, playerEntity.Being.Accuracy);
        Assert.Equal(100, playerEntity.Being.Evasion);
        Assert.Equal(50, sylph.Being.Accuracy);
        Assert.Equal(75, sylph.Being.Evasion);

        var bow = playerEntity.Being.Items.Single();
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = bow;
        equipMessage.Slot = EquipmentSlot.GraspBothRanged;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        var nymphAbility = undine.Being.Abilities
            .First(a => (a.Ability.Activation & ActivationType.Slottable) != 0
                        && a.Ability.Template?.Type != AbilityType.DefaultAttack);
        Assert.Equal(2, nymphAbility.Ability.Slot);

        var attackAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.TwoHandedRangedAttack.Name)];

        var activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = undine.Position.LevelCell;
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
        activateAbilityMessage.TargetCell = sylph.Position.LevelCell;
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
        var iceShardAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)];

        var accuracy = manager.AbilityActivationSystem.GetAccuracy(iceShardAbility.Ability, playerEntity);
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
        activateAbilityMessage.TargetCell = undine.Position.LevelCell;
        activateAbilityMessage.AbilityEntity = iceShardAbility;
        manager.Enqueue(activateAbilityMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, messageCount);
        Assert.Equal(1100, iceShardAbility.Ability.CooldownTick);

        activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = undine.Position.LevelCell;
        activateAbilityMessage.AbilityEntity = iceShardAbility;
        manager.Enqueue(activateAbilityMessage);

        Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));

        Assert.Equal(1, messageCount);
        Assert.Equal(1100, iceShardAbility.Ability.CooldownTick);
    }

    [Fact]
    public void Can_use_toggleable_abilities()
    {
        var level = TestHelper.BuildLevel(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var player = playerEntity.Player;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        manager.XPSystem.AddPlayerXP(player.NextLevelXP * 3, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);
        var toggledAbility = playerEntity.Being.Abilities
            .Single(a => (a.Ability.Activation & ActivationType.WhileToggled) != 0);
        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = toggledAbility;
        setSlotMessage.Slot = 2;

        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.False(toggledAbility.Ability.IsActive);
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

        player.NextAction = ActorAction.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        player.NextAction = ActorAction.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        player.NextAction = ActorAction.Wait;
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
O#.......#
#.#......#
.#.#.....#
....#....#
.........#
....#....#
.....#T..#",
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
O#.......#
#T#......#
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
O#.......#
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
        Direction.Southeast);

    [Fact]
    public void Targeting_line_pattern2_diagonal_near() => TestTargeting(@"
O........#
.T.......#
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
...O.....#
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
        Direction.South);

    [Fact]
    public void Targeting_line_pattern2_vertical_near() => TestTargeting(@"
..O......#
..T......#
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
        Direction heading)
    {
        var (level, rooms) = TestHelper.BuildLevelWithRooms(map);
        var room = rooms.Single();
        var origin = room.PredefinedPoints.Single(p => p.Item2 == 'O').Item1;
        var (target, targetChar) = room.PredefinedPoints.SingleOrDefault(p => p.Item2 == 'T');
        if (targetChar != 'T')
        {
            target = origin;
        }

        var manager = level.Entity.Manager;
        var abilityActivationSystem = manager.AbilityActivationSystem;
        var targetedCells = abilityActivationSystem.GetTargetedCells(
            origin, target, heading, range, targetingShape, targetShapeSize, level);

        var visibleTerrain = new byte[level.TileCount];
        var originDistance = 0;
        foreach (var (point, exposure) in targetedCells)
        {
            visibleTerrain[level.PointToIndex[point.X, point.Y]] += exposure;
            var distance = origin.DistanceTo(point);
            Assert.False(distance < originDistance, $"Point {point} is out of sequence");

            originDistance = distance;
        }

        ((List<(Point, byte)>)targetedCells).Clear();
        manager.PointByteListArrayPool.Return((List<(Point, byte)>)targetedCells);
        TestHelper.AssertVisibility(level, expectedExposure, visibleTerrain);
    }

    private class AbilityActivatedListener : IGameSystem<AbilityActivatedMessage>
    {
        public Action<AbilityActivatedMessage> ProcessMessage
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
