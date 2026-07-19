using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Senses;

public class SensorySystemTest
{
    private static readonly string TestMap = @"
...
...
..#";

    [Fact]
    public void Hearing_uses_quietness_fast_range_and_weighted_terrain_attenuation()
    {
        var level = TestHelper.BuildRoom("...............");
        var listener = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(10, 0));
        var manager = listener.Manager;
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(20, listener.Being!.HearingLevel);
        Assert.Equal(200, source.Being!.Quietness);
        Assert.False(manager.SensorySystem.CanHear(listener, source, actionNoise: 0));
        Assert.True(manager.SensorySystem.CanHear(listener, source, actionNoise: 100));
        Assert.False(manager.SensorySystem.CanHear(listener, source, actionNoise: 99));

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            20, nameof(BeingComponent.HearingLevel), listener, manager);
        Assert.True(manager.SensorySystem.CanHear(listener, source, actionNoise: 100));
    }

    [Fact]
    public void Walls_attenuate_sound_more_than_open_floor()
    {
        var openLevel = TestHelper.BuildRoom("...");
        var openListener = CreatureData.HumanWarrior.Instantiate(openLevel, new Point(0, 0));
        var openSource = CreatureData.HumanWarrior.Instantiate(openLevel, new Point(2, 0));
        var openManager = openListener.Manager;
        openManager.Queue.ProcessQueue(openManager);
        Assert.True(openManager.SensorySystem.CanHear(openListener, openSource, actionNoise: 100));

        var wallLevel = TestHelper.BuildRoom(".#.");
        var wallListener = CreatureData.HumanWarrior.Instantiate(wallLevel, new Point(0, 0));
        var wallSource = CreatureData.HumanWarrior.Instantiate(wallLevel, new Point(2, 0));
        var wallManager = wallListener.Manager;
        wallManager.Queue.ProcessQueue(wallManager);
        Assert.False(wallManager.SensorySystem.CanHear(wallListener, wallSource, actionNoise: 100));
    }

    [Fact]
    public void Batched_sound_search_matches_individual_listeners()
    {
        var level = TestHelper.BuildRoom(".....");
        GameEntity[] listeners =
        [
            CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0)),
            CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0)),
            CreatureData.HumanWarrior.Instantiate(level, new Point(3, 0))
        ];
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(4, 0));
        var manager = source.Manager;
        manager.Queue.ProcessQueue(manager);
        var soundSenses = new SenseType[listeners.Length];

        manager.SensorySystem.CalculateSensedSound(
            source, actionNoise: 100, listeners, soundSenses);

        for (var i = 0; i < listeners.Length; i++)
        {
            Assert.Equal(
                manager.SensorySystem.CanHear(listeners[i], source, actionNoise: 100),
                soundSenses[i] != SenseType.None);
        }
    }

    [Fact]
    public void Sound_messages_reveal_unseen_actors_by_sound()
    {
        var level = TestHelper.BuildRoom("...........");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(10, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Blind, player, amount: 400));
        manager.Queue.ProcessQueue(manager);
        Assert.Null(source.Position!.Knowledge);

        SoundMessage.Enqueue(source, actionNoise: 200, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(SenseType.Sound, source.Position.Knowledge!.Knowledge!.SensedType);
        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, source));
        Assert.False(source.Position.Knowledge.Knowledge.IsIdentified);
    }

    [Fact]
    public void Distant_sound_logs_direction_without_revealing_position()
    {
        var level = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Blind, player, amount: 400));
        manager.Queue.ProcessQueue(manager);
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);
        Assert.Null(source.Position!.Knowledge);
        player.Player!.LogEntries.Clear();

        SoundMessage.Enqueue(source, actionNoise: 20, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(source.Position.Knowledge);
        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, source));
        Assert.Contains(player.Player.LogEntries, entry => entry.Message == "You hear a sound from the east.");
    }

    [Fact]
    public void Action_sound_is_not_retained_by_current_sense_queries()
    {
        var level = TestHelper.BuildRoom("...........");
        var listener = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var deafListener = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(10, 0));
        var manager = listener.Manager;
        manager.Queue.ProcessQueue(manager);

        Blind(listener);
        Blind(deafListener);
        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Deafen, deafListener, amount: 400));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, deafListener.Being!.HearingLevel);

        SoundMessage.Enqueue(source, actionNoise: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(listener, source));
        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(deafListener, source));

        void Blind(GameEntity sensor)
        {
            manager.EffectApplicationSystem.ApplyStatus(
                EffectApplicationContext.CreateStatus(EffectType.Blind, sensor, amount: 400));
        }
    }

    [Fact]
    public void Action_sight_is_stored_in_knowledge_but_not_returned_as_current_sight()
    {
        var level = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        player.Position!.Heading = Direction.East;
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        SoundMessage.Enqueue(source, actionNoise: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(SenseType.Sight, manager.SensorySystem.CanSense(player, source));
        Assert.Equal(
            SenseType.Sight | SenseType.Sound,
            source.Position!.Knowledge!.Knowledge!.SensedType);

        SoundMessage.Enqueue(source, actionNoise: 0, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(SenseType.Sight, source.Position.Knowledge.Knowledge.SensedType);

        SoundMessage.Enqueue(source, actionNoise: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            SenseType.Sight | SenseType.Sound,
            source.Position.Knowledge.Knowledge.SensedType);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Blind, player, amount: 400));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, manager.SensorySystem.GetVisibility(player, source.Position!.Cell!.Value));

        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, source));
        Assert.Equal(SenseType.Sound, source.Position.Knowledge!.Knowledge!.SensedType);

        SoundMessage.Enqueue(source, actionNoise: 0, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, source));
        Assert.Equal(SenseType.None, source.Position.Knowledge!.Knowledge!.SensedType);
    }

    [Fact]
    public void Audible_actions_do_not_clear_sleepy_status()
    {
        var level = TestHelper.BuildRoom("......");
        var sleeper = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(5, 0));
        var manager = sleeper.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Sedate, sleeper, amount: 200));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(sleeper, AbilityData.Sleepy.Name));

        SoundMessage.Enqueue(source, actionNoise: 0, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(sleeper, AbilityData.Sleepy.Name));

        SoundMessage.Enqueue(source, actionNoise: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(sleeper, AbilityData.Sleepy.Name));
    }

    [Fact]
    public void Successful_movement_emits_sound()
    {
        var level = TestHelper.BuildRoom("...");
        var sleeper = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        var mover = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        var manager = sleeper.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Sedate, sleeper, amount: 200));
        manager.Queue.ProcessQueue(manager);

        mover.AI!.NextAction = ActorActionType.MoveOneCell;
        mover.AI.NextActionTarget = (int)Direction.West;
        PerformActionMessage.EnqueueAI(mover, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new Point(1, 0), mover.Position!.Cell);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(sleeper, AbilityData.Sleepy.Name));
    }

    [Fact]
    public void Visibility_updated_on_travel()
    {
        var level = TestHelper.BuildRoom(TestMap);
        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 1));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
        player.Position!.Heading = Direction.West;
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.Dagger.Instantiate(undine);

        manager.Queue.ProcessQueue(manager);

        var dagger = undine.Being!.Items.Single();
        Assert.Equal(SenseType.Sight | SenseType.Touch, manager.SensorySystem.CanSense(player, undine));
        Assert.Equal(
            SenseType.Sight | SenseType.Touch,
            manager.SensorySystem.SensedByPlayer(undine, undine.Position!.Cell!.Value));
        Assert.Equal(SenseType.Sight | SenseType.Touch | SenseType.Telepathy,
            manager.SensorySystem.CanSense(player, player));
        Assert.Equal(SenseType.Sight | SenseType.Touch | SenseType.Telepathy,
            manager.SensorySystem.SensedByPlayer(player, player.Position.Cell!.Value));
        Assert.Equal(SenseType.Sight, manager.SensorySystem.CanSense(player, dagger));
        Assert.Equal(
            SenseType.Sight,
            manager.SensorySystem.SensedByPlayer(dagger, undine.Position.Cell!.Value));
        var expectedVisibleMap = @"
.
.
.";
        TestAssert.AssertVisibility(level, expectedVisibleMap, level.VisibleTerrain);
        var expectedKnownMap = @"
.
.
.";
        TestAssert.AssertTerrain(level, expectedKnownMap, level.KnownTerrain);

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.South;
        travelMessage.TargetCell = player.Position.Cell!.Value;
        manager.Enqueue(travelMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(SenseType.Touch, manager.SensorySystem.CanSense(player, undine));
        Assert.Equal(SenseType.Touch, manager.SensorySystem.SensedByPlayer(undine, undine.Position.Cell!.Value));
        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, dagger));
        Assert.Equal(SenseType.None, manager.SensorySystem.SensedByPlayer(dagger, undine.Position.Cell!.Value));
        expectedVisibleMap = @"
 
 
..#";
        TestAssert.AssertVisibility(level, expectedVisibleMap, level.VisibleTerrain);
        expectedKnownMap = @"
.
.
..#";
        TestAssert.AssertTerrain(level, expectedKnownMap, level.KnownTerrain);
    }

    [Fact]
    public void Blinded_progressively_reduces_sight_but_preserves_touch()
    {
        var level = TestHelper.BuildRoom(@"
.....
.....
.....");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 1));
        var adjacent = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 1));
        var far = CreatureData.HumanWarrior.Instantiate(level, new Point(4, 1));
        playerEntity.Position!.Heading = Direction.East;
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var farCell = new Point(4, 1);
        var adjacentCell = new Point(1, 1);
        var primaryRange = playerEntity.Being!.PrimaryVisionRange;
        var secondaryRange = playerEntity.Being.SecondaryVisionRange;

        // Touch supplements ordinary sight for adjacent entities even when the observer is not blinded.
        Assert.True(manager.SensorySystem.GetVisibility(playerEntity, farCell) > 0);
        Assert.True(manager.SensorySystem.GetVisibility(playerEntity, adjacentCell) > 0);
        Assert.Equal(SenseType.Sight | SenseType.Touch, manager.SensorySystem.CanSense(playerEntity, adjacent));
        Assert.Equal(SenseType.Sight, manager.SensorySystem.CanSense(playerEntity, far));

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Blind, playerEntity, amount: 200));
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Blinded.Name));
        Assert.Equal(primaryRange / 2, playerEntity.Being.PrimaryVisionRange);
        Assert.Equal(secondaryRange / 2, playerEntity.Being.SecondaryVisionRange);
        Assert.True(manager.SensorySystem.GetVisibility(playerEntity, farCell) > 0);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Blind, playerEntity, amount: 200));
        manager.Queue.ProcessQueue(manager);

        // At level 2 no tile is visible; only the adjacent entity is still felt.
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Blinded.Name));
        Assert.Equal(0, manager.SensorySystem.GetVisibility(playerEntity, farCell));
        Assert.Equal(0, manager.SensorySystem.GetVisibility(playerEntity, adjacentCell));
        Assert.Equal(SenseType.Touch, manager.SensorySystem.CanSense(playerEntity, adjacent));
        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(playerEntity, far));

        manager.EffectApplicationSystem.RemoveAbility(playerEntity, AbilityData.Blinded.Name, manager);
        manager.Queue.ProcessQueue(manager);

        // Sight restored: the far tile is visible again.
        Assert.Equal(primaryRange, playerEntity.Being.PrimaryVisionRange);
        Assert.Equal(secondaryRange, playerEntity.Being.SecondaryVisionRange);
        Assert.True(manager.SensorySystem.GetVisibility(playerEntity, farCell) > 0);
    }

    [Fact]
    public void Engulfed_has_no_sight_but_senses_the_adjacent_engulfer_by_touch()
    {
        var level = TestHelper.BuildRoom(".....");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(1, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        var primaryRange = player.Being!.PrimaryVisionRange;
        var secondaryRange = player.Being.SecondaryVisionRange;
        var engulfEffect = engulfer.ActiveEffects!.Abilities
            .SelectMany(a => a.Ability!.Effects)
            .Single(e => e.Effect!.EffectType == EffectType.Engulf);
        TestHelper.ApplyEffect(engulfEffect, engulfer, player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, player.Being.PrimaryVisionRange);
        Assert.Equal(0, player.Being.SecondaryVisionRange);
        Assert.Equal(0, manager.SensorySystem.GetVisibility(player, new Point(4, 0)));
        Assert.Equal(SenseType.Touch, manager.SensorySystem.CanSense(player, engulfer));

        engulfer.Being!.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(primaryRange, player.Being.PrimaryVisionRange);
        Assert.Equal(secondaryRange, player.Being.SecondaryVisionRange);
    }
}
