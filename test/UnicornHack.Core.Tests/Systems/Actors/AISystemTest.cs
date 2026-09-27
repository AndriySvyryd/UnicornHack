using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;
using static UnicornHack.Systems.Effects.EffectTestHelper;

namespace UnicornHack.Systems.Actors;

public class AISystemTest
{
    private const string Map = @"
.....
.....
.....
.....
.....";

    [Theory]
    [InlineData(false, EffectType.ToxinDamage)]
    [InlineData(true, EffectType.Deafen)]
    public void Digestion_attack_uses_its_normal_delay_against_the_engulfed_victim(
        bool useSylph, EffectType expectedEffect)
    {
        var room = TestHelper.BuildRoom("...");
        var manager = room.Entity.Manager;
        var engulfer = (useSylph ? CreatureData.Sylph : CreatureData.GelatinousCube)
            .Instantiate(room, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 0));
        PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);

        var engulfEffect = engulfer.ActiveEffects!.Abilities
            .SelectMany(ability => ability.Ability!.Effects)
            .Single(effect => effect.Effect!.EffectType == EffectType.Engulf);
        TestHelper.ApplyEffect(engulfEffect, engulfer, victim, manager);
        manager.Queue.ProcessQueue(manager);

        var digestion = engulfer.ActiveEffects.Abilities.Single(
            ability => ability.Ability!.Action == AbilityAction.Digestion);
        var digestionAbility = digestion.Ability!;
        var delay = manager.AbilityActivationSystem.GetDelay(digestionAbility, engulfer);
        var actionTick = engulfer.AI!.NextActionTick!.Value;
        engulfer.Position!.Heading = Direction.East;
        engulfer.AI.NextAction = ActorActionType.UseAbilitySlot;
        engulfer.AI.NextActionTarget = digestionAbility.Slot;
        engulfer.AI.NextActionTarget2 = engulfer.Position.Cell!.Value
            .DifferenceTo(victim.Position!.Cell!.Value).ToInt32();

        TestHelper.PerformTurn(engulfer, manager);

        Assert.True(victim.Being!.HasEffectOrAccumulator(expectedEffect));
        Assert.Equal(actionTick + delay, engulfer.AI.NextActionTick);
        Assert.Equal(new Point(0, 0), engulfer.Position.Cell);
        Assert.Equal(new Point(1, 0), victim.Position!.Cell);
    }

    /// <summary>
    ///     An NPC in a room no living player occupies must just pass its turn instead of
    ///     wandering.
    /// </summary>
    [Fact]
    public void Actor_in_room_without_a_player_waits()
    {
        var game = TestHelper.CreateGame(seed: 1u);

        // Player is in its own room.
        var playerRoom = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, playerRoom, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        // NPC is alone in a different room.
        var otherRoom = TestHelper.BuildRoom(Map, game, depth: 1);
        var npc = CreatureData.HumanWarrior.Instantiate(otherRoom, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var ai = npc.AI!;
        ai.NextAction = null;
        ai.NextActionTarget = null;
        ai.NextActionTarget2 = null;

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.Wait, ai.NextAction);
    }

    /// <summary>
    ///     An NPC alone in a room (no living player) must just pass its turn — it must not
    ///     wander, even toward an adjacent item. With no player observing the room the actor
    ///     never moves, so it never walks onto (and picks up) floor items off-screen.
    /// </summary>
    [Fact]
    public void Actor_with_an_adjacent_item_and_no_player_waits_instead_of_moving()
    {
        var game = TestHelper.CreateGame(seed: 1u);

        var playerRoom = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, playerRoom, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        // NPC alone in a different room, with an item right next to it.
        var otherRoom = TestHelper.BuildRoom(Map, game, depth: 1);
        var npc = CreatureData.HumanWarrior.Instantiate(otherRoom, new Point(0, 0));
        npc.Position!.Heading = Direction.East;
        ItemData.GoldCoin.Instantiate(otherRoom, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var goldEntity = otherRoom.Items[new Point(1, 0)];

        var ai = npc.AI!;
        ai.NextAction = null;
        ai.NextActionTarget = null;
        ai.NextActionTarget2 = null;

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);

        // It decided to wait rather than move, so it stayed put and never picked the item up.
        Assert.Equal(ActorActionType.Wait, ai.NextAction);
        Assert.Equal(new Point(0, 0), npc.Position!.Cell);
        Assert.Null(goldEntity.Item!.ContainerId);
        Assert.Same(goldEntity, otherRoom.Items[new Point(1, 0)]);
    }

    /// <summary>
    ///     Guard against over-restriction: an NPC moving over an item in a room a player
    ///     occupies still picks it up.
    /// </summary>
    [Fact]
    public void Actor_in_room_with_a_player_picks_up_items()
    {
        var game = TestHelper.CreateGame(seed: 1u);

        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(4, 4));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(0, 0));
        npc.Position!.Heading = Direction.East;
        ItemData.GoldCoin.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var goldEntity = room.Items[new Point(1, 0)];

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = npc;
        travelMessage.TargetCell = new Point(1, 0);
        travelMessage.TargetHeading = Direction.East;
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new Point(1, 0), npc.Position!.Cell);
        Assert.Equal(npc.Id, goldEntity.Item!.ContainerId);
        Assert.False(room.Items.ContainsKey(new Point(1, 0)));
    }

    [Fact]
    public void Actor_paths_around_another_creature_to_reach_the_player()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(@"
....
....
....", game);
        var actor = CreatureData.HumanWarrior.Instantiate(room, new Point(0, 1));
        actor.Position!.Heading = Direction.East;
        CreatureData.HumanWarrior.Instantiate(room, new Point(1, 1));
        PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(3, 1));
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);

        // If pathing gives up on the occupied East cell, wandering chooses Northwest.
        ((TestRandom)manager.Game.Random).EnqueueNext(2);
        DecideNextActionMessage.Enqueue(actor, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.ChangeHeading, actor.AI!.NextAction);
        Assert.Equal((int)Direction.Northeast, actor.AI.NextActionTarget);

        TestHelper.PerformTurn(actor, manager);
        Assert.Equal(ActorActionType.MoveOneCell, actor.AI.NextAction);
        Assert.Equal((int)Direction.Northeast, actor.AI.NextActionTarget);

        TestHelper.PerformTurn(actor, manager);
        Assert.Equal(new Point(1, 0), actor.Position.Cell);
    }

    [Fact]
    public void Creature_attacks_whoever_moves_into_the_aimed_cell()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(@"
..
..", game);
        var firefly = CreatureData.Firefly.Instantiate(room, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 0));
        var spider = CreatureData.GiantSpider.Instantiate(room, new Point(1, 1));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);

        DecideNextActionMessage.Enqueue(firefly, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.UseAbilitySlot, firefly.AI!.NextAction);
        var targetVector = Vector.Unpack(firefly.AI.NextActionTarget2!.Value);
        Assert.Equal(new Vector(1, 0), targetVector);

        var playerMove = TravelMessage.Create(manager);
        playerMove.ActorEntity = player;
        playerMove.TargetCell = new Point(0, 1);
        playerMove.TargetHeading = Direction.Southwest;
        manager.Enqueue(playerMove);
        manager.Queue.ProcessQueue(manager);

        var spiderMove = TravelMessage.Create(manager);
        spiderMove.ActorEntity = spider;
        spiderMove.TargetCell = new Point(1, 0);
        spiderMove.TargetHeading = Direction.North;
        manager.Enqueue(spiderMove);
        manager.Queue.ProcessQueue(manager);

        var playerHitPoints = player.Being!.HitPoints;
        var spiderHitPoints = spider.Being!.HitPoints;

        TestHelper.PerformTurn(firefly, manager);

        Assert.Equal(playerHitPoints, player.Being.HitPoints);
        Assert.True(spider.Being.HitPoints < spiderHitPoints);
    }

    [Fact]
    public void Creature_does_not_bump_attack_an_actor_blocking_its_planned_move()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom("...", game);
        var firefly = CreatureData.Firefly.Instantiate(room, new Point(0, 0));
        var spider = CreatureData.GiantSpider.Instantiate(room, new Point(1, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 0));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);

        var fireSting = firefly.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "fire sting").Ability!;
        fireSting.Slot = AbilitySlottingSystem.DefaultMeleeAttackSlot;
        manager.Queue.ProcessQueue(manager);
        firefly.Position!.Heading = Direction.East;
        firefly.AI!.NextAction = ActorActionType.MoveOneCell;
        firefly.AI.NextActionTarget = (int)Direction.East;
        var actionTick = firefly.AI.NextActionTick;
        var spiderHitPoints = spider.Being!.HitPoints;
        player.Player!.LogEntries.Clear();

        TestHelper.PerformTurn(firefly, manager);

        Assert.Equal(new Point(0, 0), firefly.Position.Cell);
        Assert.Equal(spiderHitPoints, spider.Being.HitPoints);
        Assert.Null(fireSting.CooldownTick);
        Assert.True(firefly.AI.NextActionTick > actionTick);
        Assert.Contains(
            player.Player.LogEntries,
            entry => entry.Message == "The firefly tries to move east, but fails.");
    }

    [Fact]
    public void Creature_spends_its_turn_when_its_attack_target_becomes_invalid()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom("..", game);
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var firefly = CreatureData.Firefly.Instantiate(room, new Point(1, 0));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);

        DecideNextActionMessage.Enqueue(firefly, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(ActorActionType.UseAbilitySlot, firefly.AI!.NextAction);

        var actionTick = firefly.AI.NextActionTick;
        firefly.AI.NextActionTarget2 = new Vector(1, 0).ToInt32();
        player.Player!.LogEntries.Clear();

        TestHelper.PerformTurn(firefly, manager);

        Assert.True(firefly.AI.NextActionTick > actionTick);
        Assert.Equal(player.Being!.HitPointMaximum, player.Being.HitPoints);
        Assert.Contains(
            player.Player.LogEntries,
            entry => entry.Message == "The firefly tries to use Fire sting, but fails.");
    }

    [Theory]
    [InlineData(ActorActionType.DropItem)]
    [InlineData(ActorActionType.EquipItem)]
    [InlineData(ActorActionType.UnequipItem)]
    public void Creature_spends_its_turn_when_an_item_action_fails(ActorActionType action)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom("..", game);
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var creature = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 0));
        var manager = game.Manager;
        ItemData.LongSword.Instantiate(creature);
        manager.Queue.ProcessQueue(manager);

        var sword = creature.Being!.Items.Single(item => item.Item!.TemplateName == ItemData.LongSword.Name);
        if (action != ActorActionType.EquipItem)
        {
            var equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = creature;
            equipMessage.ItemEntity = sword;
            equipMessage.Slot = EquipmentSlot.GraspBothMelee;
            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);
        }

        ApplyStatus(creature, EffectType.Stick, duration: 100, manager, amount: 1000);
        manager.Queue.ProcessQueue(manager);
        Assert.False(creature.Being.CanMoveItems);

        var ai = creature.AI!;
        ai.NextAction = action;
        ai.NextActionTarget = sword.Id;
        ai.NextActionTarget2 = action == ActorActionType.EquipItem
            ? (int)EquipmentSlot.GraspBothMelee
            : null;
        var actionTick = ai.NextActionTick;
        player.Player!.LogEntries.Clear();

        TestHelper.PerformTurn(creature, manager);

        Assert.Equal(actionTick + TimeSystem.DefaultActionDelay, ai.NextActionTick);
        Assert.Equal(creature.Id, sword.Item!.ContainerId);
        Assert.Equal(player.Being!.HitPointMaximum, player.Being.HitPoints);
        var actionText = action switch
        {
            ActorActionType.DropItem => "drop an item",
            ActorActionType.EquipItem => "equip an item",
            ActorActionType.UnequipItem => "unequip an item",
            _ => throw new InvalidOperationException()
        };
        Assert.Contains(
            player.Player.LogEntries,
            entry => entry.Message == $"The human warrior tries to {actionText}, but fails.");
    }

    [Fact]
    public void Failed_item_equipment_messages_always_add_the_default_delay()
    {
        var room = TestHelper.BuildRoom(".");
        var creature = CreatureData.HumanWarrior.Instantiate(room, new Point(0, 0));
        var manager = creature.Manager;
        ItemData.LongSword.Instantiate(creature);
        manager.Queue.ProcessQueue(manager);

        var sword = creature.Being!.Items.Single(item => item.Item!.TemplateName == ItemData.LongSword.Name);
        var ai = creature.AI!;
        ai.NextActionTick = manager.Game.CurrentTick + 5;

        var message = ItemEquippedMessage.Create(manager);
        message.ActorEntity = creature;
        message.ItemEntity = sword;
        manager.Enqueue(message);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(manager.Game.CurrentTick + 5 + TimeSystem.DefaultActionDelay, ai.NextActionTick);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void Confused_creature_avoids_item_actions_that_would_fail(int itemActionIndex)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom("...", game);
        var creature = CreatureData.Bugbear.Instantiate(room, new Point(0, 0));
        PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 0));
        var manager = game.Manager;
        ItemData.FieryAegis.Instantiate(creature);
        manager.Queue.ProcessQueue(manager);

        var shield = creature.Being!.Items.Single();
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = creature;
        equipMessage.ItemEntity = shield;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;
        equipMessage.Force = true;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(EquipmentSlot.GraspBothMelee, shield.Item!.EquippedSlot);

        ApplyStatus(creature, EffectType.Stick, duration: 100, manager, amount: 1000);
        manager.Queue.ProcessQueue(manager);
        Assert.False(creature.Being!.CanMoveItems);

        var dropMessage = MoveItemMessage.Create(manager);
        dropMessage.ItemEntity = shield;
        dropMessage.TargetRoomEntity = room.Entity;
        dropMessage.TargetCell = creature.Position!.Cell;
        Assert.False(manager.ItemMovingSystem.CanMoveItem(dropMessage, manager));
        manager.ReturnMessage(dropMessage);

        var unequipMessage = EquipItemMessage.Create(manager);
        unequipMessage.ActorEntity = creature;
        unequipMessage.ItemEntity = shield;
        unequipMessage.Slot = EquipmentSlot.None;
        Assert.False(manager.ItemUsageSystem.CanEquipItem(unequipMessage, manager));
        manager.ReturnMessage(unequipMessage);

        ApplyStatus(creature, EffectType.Confuse, duration: 100, manager, amount: 1000);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(100, creature.Being.RandomActionPercent);

        var random = (TestRandom)manager.Game.Random;
        random.EnqueueNext(itemActionIndex);
        random.EnqueueNext(0);
        random.EnqueueNext(0);

        var randomizedAction = TestActorSystem.RandomizeAction(
            creature, ActorActionType.PickupItem, manager);

        Assert.Equal(ActorActionType.ChangeHeading, randomizedAction);
        Assert.Equal(EquipmentSlot.GraspBothMelee, shield.Item!.EquippedSlot);
    }

    [Fact]
    public void Creature_attack_target_remains_relative_after_displacement()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(@"
...
...
...", game);
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        var firefly = CreatureData.Firefly.Instantiate(room, new Point(2, 1));
        var spider = CreatureData.GiantSpider.Instantiate(room, new Point(1, 2));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);

        DecideNextActionMessage.Enqueue(firefly, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.UseAbilitySlot, firefly.AI!.NextAction);

        var displacement = TravelMessage.Create(manager);
        displacement.ActorEntity = firefly;
        displacement.TargetCell = new Point(2, 2);
        displacement.TargetHeading = Direction.South;
        displacement.SuppressDelay = true;
        manager.Enqueue(displacement);
        manager.Queue.ProcessQueue(manager);

        var playerHitPoints = player.Being!.HitPoints;
        var spiderHitPoints = spider.Being!.HitPoints;

        TestHelper.PerformTurn(firefly, manager);

        Assert.Equal(playerHitPoints, player.Being.HitPoints);
        Assert.True(spider.Being.HitPoints < spiderHitPoints);
    }

    /// <summary>
    ///     A paralyzed creature can't perform its decided attack; its planned action becomes a wait
    ///     and it's rescheduled to retry when the paralysis is expected to end.
    /// </summary>
    [Fact]
    public void Paralyzed_creature_plans_to_wait()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        // The creature decides to attack the adjacent player.
        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(ActorActionType.UseAbilitySlot, npc.AI!.NextAction);

        ApplyStatus(npc, EffectType.Paralyze, duration: 100, manager, amount: 200);

        var playerHitPoints = playerEntity.Being!.HitPoints;
        var tickBefore = npc.AI.NextActionTick;

        TestHelper.PerformTurn(npc, manager);

        Assert.Equal(playerHitPoints, playerEntity.Being.HitPoints);
        Assert.Equal(ActorActionType.Wait, npc.AI.NextAction);
        Assert.Equal(tickBefore + TimeSystem.DefaultActionDelay, npc.AI.NextActionTick);
    }

    /// <summary>
    ///     Once the paralysis wears off, the creature resumes acting and attacks the adjacent player.
    /// </summary>
    [Fact]
    public void Creature_resumes_acting_once_the_incapacitation_ends()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(ActorActionType.UseAbilitySlot, npc.AI!.NextAction);

        ApplyStatus(npc, EffectType.Paralyze, duration: 100, manager, amount: 200);
        var records = npc.ActiveEffects!.ComponentEffects
            .Where(effect => effect.TargetName == AbilityData.Paralyzed.Name)
            .ToList();
        Assert.Equal(2, records.Count);
        Assert.All(records, record => Assert.Equal(1, record.Level));
        npc.AI!.PreviousActionTick = manager.Game.CurrentTick - TimeSystem.DefaultActionDelay;

        // The active paralysis costs it one turn while its residual buildup decays.
        TestHelper.PerformTurn(npc, manager);
        Assert.Equal(ActorActionType.Wait, npc.AI.NextAction);

        manager.Game.CurrentTick = records.Max(record => record.ExpirationTarget)!.Value;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(npc, manager);
        manager.Queue.ProcessQueue(manager);

        // The timed carrier lapses with sub-threshold buildup still banked. The creature wakes, performs its
        // planned wait and decides to attack again.
        TestHelper.PerformTurn(npc, manager);
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Paralyzed.Name) > 0);
        Assert.False(npc.ActiveEffects.Accumulators.ContainsKey(EffectType.Paralyze));
        Assert.Equal(ActorActionType.UseAbilitySlot, npc.AI.NextAction);

        // The attack is now performed.
        var attacks = 0;
        var listener = new AttackListener(() => attacks++);
        manager.Queue.Register(listener, AbilityActivatedMessage.Name, -1);

        TestHelper.PerformTurn(npc, manager);
        Assert.True(attacks > 0);
    }

    /// <summary>
    ///     A stunned player can't act: their queued move is dropped, they don't move, and the
    ///     server advances their clock rather than waiting for input.
    /// </summary>
    [Fact]
    public void Stunned_player_cannot_move()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager, amount: 300);

        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        var cellBefore = playerEntity.Position!.Cell!.Value;
        var tickBefore = player.NextActionTick;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(cellBefore, playerEntity.Position.Cell);
        Assert.Null(player.NextAction);
        Assert.Equal(tickBefore + TimeSystem.DefaultActionDelay, player.NextActionTick);
    }

    [Theory]
    [InlineData(100, 24, false)]
    [InlineData(100, 25, true)]
    [InlineData(200, 49, false)]
    [InlineData(200, 50, true)]
    public void Lower_stunned_levels_replace_actions_at_their_probability_boundary(
        int buildup, int roll, bool moves)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager, amount: buildup);
        Assert.Equal(buildup / 4, playerEntity.Being!.UnableToActPercent);
        ((TestRandom)manager.Game.Random).EnqueueNext(roll);

        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        var cellBefore = playerEntity.Position!.Cell!.Value;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(moves ? new Point(3, 2) : cellBefore, playerEntity.Position.Cell);
        Assert.Null(player.NextAction);
    }

    /// <summary>
    ///     A paralysis shorter than the time between the actor's actions doesn't take hold at once;
    ///     it banks and the actor keeps acting until successive applications accumulate to a full
    ///     action interval.
    /// </summary>
    [Fact]
    public void A_short_incapacitation_builds_up_before_it_takes_hold()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        var player = playerEntity.Player!;

        // A single application shorter than the actor's action interval only banks; it keeps acting. A
        // fresh actor's threshold is its default action interval (it hasn't acted yet), so half an interval
        // is safely sub-threshold and merely banks.
        var freshInterval = Effects.EffectTestHelper.GetStatusEffectThreshold(playerEntity);
        var partial = freshInterval / 2;

        ApplyStatus(playerEntity, EffectType.Paralyze, duration: partial, manager);
        manager.Queue.ProcessQueue(manager);

        var cellBefore = playerEntity.Position!.Cell!.Value;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, manager);

        // Still able to act: the move goes through.
        Assert.Equal(cellBefore.Translate(Direction.East.AsVector()), playerEntity.Position!.Cell);

        // The move also turns, lengthening the actor's action interval; the threshold is now that longer
        // interval. Top the banked buildup up to two fixed thresholds so level 2 incapacitates.
        var banked = playerEntity.ActiveEffects!.Accumulators[EffectType.Paralyze].Value;
        ApplyStatus(
            playerEntity, EffectType.Paralyze,
            duration: (2 * EffectApplicationSystem.BuildUpThreshold) - banked, manager);
        manager.Queue.ProcessQueue(manager);

        var cellBefore2 = playerEntity.Position!.Cell!.Value;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, manager);

        // Now it takes hold: the move is dropped.
        Assert.Equal(cellBefore2, playerEntity.Position.Cell);
        Assert.Null(player.NextAction);
    }

    [Fact]
    public void Paralyzed_level_two_keeps_residual_buildup_banked_while_active()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        var player = playerEntity.Player!;

        ApplyStatus(playerEntity, EffectType.Paralyze, duration: 100, manager, amount: 250);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Paralyzed.Name));
        Assert.Equal(50, playerEntity.ActiveEffects!.Accumulators[EffectType.Paralyze].Value);

        var cellBefore = playerEntity.Position!.Cell!.Value;
        player.PreviousActionTick = manager.Game.CurrentTick - TimeSystem.DefaultActionDelay;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(cellBefore, playerEntity.Position.Cell);
        Assert.Equal(20, playerEntity.ActiveEffects.Accumulators[EffectType.Paralyze].Value);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Paralyzed.Name));
    }

    /// <summary>
    ///     Applying a status ability imposes its condition, and removing it lifts the condition again.
    /// </summary>
    [Fact]
    public void Status_ability_imposes_and_lifts_its_condition()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        Assert.False(EffectApplicationSystem.GetAbilityLevel(playerEntity, "stunned") > 0);

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager);

        Assert.True(EffectApplicationSystem.GetAbilityLevel(playerEntity, "stunned") > 0);

        manager.EffectApplicationSystem.RemoveAbility(playerEntity, "stunned", manager);
        manager.Queue.ProcessQueue(manager);

        Assert.False(EffectApplicationSystem.GetAbilityLevel(playerEntity, "stunned") > 0);
    }

    [Fact]
    public void Confused_level_three_moves_in_a_random_direction()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        Assert.IsType<LeveledAbility>(AbilityData.Confused);
        Assert.IsType<LeveledAbility>(manager.Game.Services.Data.Abilities.Get(AbilityData.Confused.Name));
        ApplyStatus(playerEntity, EffectType.Confuse, duration: 100, manager, amount: 350);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(50, playerEntity.ActiveEffects!.Accumulators[EffectType.Confuse].Value);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Confused.Name));

        // East is excluded, leaving Northeast then North as the first two candidates.
        ((TestRandom)manager.Game.Random).EnqueueNext(1);

        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        var cellBefore = playerEntity.Position!.Cell!.Value;

        TestHelper.PerformTurn(playerEntity, manager);

        // The player lurched North instead of stepping East as directed.
        Assert.Equal(cellBefore.Translate(Direction.North.AsVector()), playerEntity.Position.Cell);
        Assert.NotEqual(cellBefore.Translate(Direction.East.AsVector()), playerEntity.Position.Cell);
    }

    [Fact]
    public void Confused_level_one_preserves_the_intended_direction()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(playerEntity, EffectType.Confuse, duration: 100, manager, amount: 150);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(50, playerEntity.ActiveEffects!.Accumulators[EffectType.Confuse].Value);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Confused.Name));

        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        var cellBefore = playerEntity.Position!.Cell!.Value;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(cellBefore.Translate(Direction.East.AsVector()), playerEntity.Position.Cell);
    }

    [Theory]
    [InlineData(49, Direction.North)]
    [InlineData(50, Direction.East)]
    public void Confused_level_two_randomizes_half_of_movement_attempts(
        int roll, Direction expectedDirection)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(playerEntity, EffectType.Confuse, duration: 100, manager, amount: 250);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(50, playerEntity.ActiveEffects!.Accumulators[EffectType.Confuse].Value);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Confused.Name));

        var random = (TestRandom)manager.Game.Random;
        random.EnqueueNext(roll);
        if (expectedDirection == Direction.North)
        {
            // East is excluded, leaving Northeast then North as the first two candidates.
            random.EnqueueNext(1);
        }

        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        var cellBefore = playerEntity.Position!.Cell!.Value;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(
            cellBefore.Translate(expectedDirection.AsVector()),
            playerEntity.Position.Cell);
    }

    [Fact]
    public void Staggered_level_two_moves_in_a_random_direction_after_the_action()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(playerEntity, EffectType.Stagger, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Staggered.Name));
        Assert.Equal(100, playerEntity.Being!.RandomMoveAfterActionPercent);

        // The requested East move resolves first, then the post-action lurch starts from that new cell.
        ((TestRandom)manager.Game.Random).EnqueueNext((int)Direction.North);

        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        var cellBefore = playerEntity.Position!.Cell!.Value;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(
            cellBefore.Translate(Direction.East.AsVector()).Translate(Direction.North.AsVector()),
            playerEntity.Position.Cell);
    }

    [Theory]
    [InlineData(49, true)]
    [InlineData(50, false)]
    public void Staggered_level_one_moves_after_half_of_actions(int roll, bool shouldMove)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(playerEntity, EffectType.Stagger, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Staggered.Name));
        Assert.Equal(50, playerEntity.Being!.RandomMoveAfterActionPercent);

        var random = (TestRandom)manager.Game.Random;
        random.EnqueueNext(roll);
        if (shouldMove)
        {
            random.EnqueueNext((int)Direction.North);
        }

        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.Wait;
        var cellBefore = playerEntity.Position!.Cell!.Value;
        var tickBefore = player.NextActionTick;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(
            shouldMove ? cellBefore.Translate(Direction.North.AsVector()) : cellBefore,
            playerEntity.Position.Cell);
            Assert.Equal(tickBefore + TimeSystem.DefaultActionDelay, player.NextActionTick);
    }

    /// <summary>
    ///     A frightened actor can't bring itself to step toward the source of its fear, though it may
    ///     still step away.
    /// </summary>
    [Fact]
    public void Frightened_actor_cannot_step_toward_the_source_of_its_fear()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        var ghost = CreatureData.Ghost.Instantiate(room, new Point(0, 2));
        manager.Queue.ProcessQueue(manager);

        var ghostGaze = ghost.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Gaze);
        ApplyCompulsion(playerEntity, EffectType.Frighten, ghostGaze, manager, amount: 200);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Frightened.Name));

        var player = playerEntity.Player!;
        var startCell = playerEntity.Position!.Cell!.Value;

        // Stepping toward the fear source (West) is refused - the player stays put.
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.West;
        TestHelper.PerformTurn(playerEntity, manager);
        Assert.Equal(startCell, playerEntity.Position.Cell);

        // Stepping away from it (East) is allowed.
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, manager);
        Assert.Equal(startCell.Translate(Direction.East.AsVector()), playerEntity.Position.Cell);
    }

    [Fact]
    public void Frightened_level_one_can_still_approach_the_source()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        var ghost = CreatureData.Ghost.Instantiate(room, new Point(0, 2));
        manager.Queue.ProcessQueue(manager);

        var ghostGaze = ghost.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Gaze);
        ApplyCompulsion(playerEntity, EffectType.Frighten, ghostGaze, manager, amount: 100);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Frightened.Name));

        var player = playerEntity.Player!;
        var startCell = playerEntity.Position!.Cell!.Value;
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.West;
        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(startCell.Translate(Direction.West.AsVector()), playerEntity.Position.Cell);
    }

    [Fact]
    public void Frightened_level_two_ai_flees_from_a_perceived_source()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var source = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 2));
        var actor = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 2));
        actor.Position!.Heading = Direction.East;
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);

        var sourceAbility = source.ActiveEffects!.Abilities.First();
        ApplyCompulsion(actor, EffectType.Frighten, sourceAbility, manager, amount: 200);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(actor, AbilityData.Frightened.Name));
        Assert.NotEqual(SenseType.None, manager.SensorySystem.CanSense(actor, source) ?? SenseType.None);

        DecideNextActionMessage.Enqueue(actor, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.MoveOneCell, actor.AI!.NextAction);
        Assert.Equal((int)Direction.East, actor.AI.NextActionTarget);
    }

    [Fact]
    public void Frightened_level_three_forces_a_random_escape_action()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 2));
        var actor = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 2));
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);

        ApplyCompulsion(actor, EffectType.Frighten, player.ActiveEffects!.Abilities.First(), manager, amount: 300);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(actor, AbilityData.Frightened.Name));

        DecideNextActionMessage.Enqueue(actor, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.MoveOneCell, actor.AI!.NextAction);
        var targetCell = actor.Position!.Cell!.Value.Translate(
            ((Direction)actor.AI.NextActionTarget!.Value).AsVector());
        Assert.True(targetCell.DistanceTo(player.Position!.Cell!.Value)
                    > actor.Position.Cell.Value.DistanceTo(player.Position.Cell.Value));
    }

    /// <summary>
    ///     A taunted actor can't bring itself to step away from the taunter, though it may still
    ///     close in.
    /// </summary>
    [Fact]
    public void Taunted_actor_cannot_step_away_from_the_taunter()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        var bugbear = CreatureData.Bugbear.Instantiate(room, new Point(0, 2));
        manager.Queue.ProcessQueue(manager);

        var taunt = bugbear.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Scream);
        ApplyCompulsion(playerEntity, EffectType.Taunt, taunt, manager, amount: 100);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Taunted.Name));

        var player = playerEntity.Player!;
        var startCell = playerEntity.Position!.Cell!.Value;

        // Stepping away from the taunter (East) is refused.
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, manager);
        Assert.Equal(startCell, playerEntity.Position.Cell);

        // Closing in on it (West) is allowed.
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.West;
        TestHelper.PerformTurn(playerEntity, manager);
        Assert.Equal(startCell.Translate(Direction.West.AsVector()), playerEntity.Position.Cell);
    }

    [Fact]
    public void Taunted_level_three_allows_only_steps_toward_the_source()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        var manager = playerEntity.Manager;
        var bugbear = CreatureData.Bugbear.Instantiate(room, new Point(0, 2));
        manager.Queue.ProcessQueue(manager);

        var taunt = bugbear.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Scream);
        ApplyCompulsion(playerEntity, EffectType.Taunt, taunt, manager, amount: 300);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Taunted.Name));

        var player = playerEntity.Player!;
        var startCell = playerEntity.Position!.Cell!.Value;

        // A lateral step (North) is refused because it does not reduce distance to the taunter.
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.North;
        TestHelper.PerformTurn(playerEntity, manager);
        Assert.Equal(startCell, playerEntity.Position.Cell);

        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.West;
        TestHelper.PerformTurn(playerEntity, manager);
        Assert.Equal(startCell.Translate(Direction.West.AsVector()), playerEntity.Position.Cell);
    }

    /// <summary>
    ///     A creature can't be compelled by a fear source it can't perceive - a wall blocking line of
    ///     sight lets the frightened actor step toward the source it can no longer see.
    /// </summary>
    [Fact]
    public void A_compulsion_does_not_apply_while_its_source_is_unperceived()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(@"
.#..
....", game);
        var manager = game.Manager;
        var ghost = CreatureData.Ghost.Instantiate(room, new Point(0, 0));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(3, 0));
        manager.Queue.ProcessQueue(manager);

        // The wall at (1,0) blocks line of sight, so the player can't perceive the ghost.
        Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(playerEntity, ghost) ?? SenseType.None);

        var ghostGaze = ghost.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Gaze);
        ApplyCompulsion(playerEntity, EffectType.Frighten, ghostGaze, manager);

        var player = playerEntity.Player!;
        var startCell = playerEntity.Position!.Cell!.Value;

        // Stepping toward the unseen fear source (West) is allowed.
        player.NextAction = ActorActionType.MoveOneCell;
        player.NextActionTarget = (int)Direction.West;
        TestHelper.PerformTurn(playerEntity, manager);
        Assert.Equal(startCell.Translate(Direction.West.AsVector()), playerEntity.Position.Cell);
    }

    private static void ApplyCompulsion(
        GameEntity target, EffectType effectType, GameEntity sourceAbility, GameManager manager,
        int amount = 1000)
    {
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            effectType, target, amount * 2, sourceAbility.Id));

        manager.Queue.ProcessQueue(manager);
    }

    [Fact]
    public void Sleepy_level_one_reduces_perception_without_preventing_an_attack()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);
        var basePerception = npc.Being!.Perception;

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        ApplyStatus(npc, EffectType.Sedate, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Sleepy.Name));
        Assert.Equal(basePerception - 3, npc.Being.Perception);

        var attacks = 0;
        var listener = new AttackListener(() => attacks++);
        manager.Queue.Register(listener, AbilityActivatedMessage.Name, -1);
        TestHelper.PerformTurn(npc, manager);

        Assert.True(attacks > 0);
    }

    [Fact]
    public void Sleepy_level_two_prevents_the_planned_attack()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        ApplyStatus(npc, EffectType.Sedate, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Sleepy.Name));

        var attacks = 0;
        var listener = new AttackListener(() => attacks++);
        manager.Queue.Register(listener, AbilityActivatedMessage.Name, -1);
        TestHelper.PerformTurn(npc, manager);

        Assert.Equal(0, attacks);
        Assert.Equal(ActorActionType.Wait, npc.AI!.NextAction);
    }

    [Fact]
    public void Sleepy_creature_wakes_when_damaged()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Sedate, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(10, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Sleepy.Name));

        var hitPointsBefore = npc.Being!.HitPoints;
        ApplyDamage(npc, amount: 5, manager);
        manager.Queue.ProcessQueue(manager);

        // The damage landed and the sleep effect was removed.
        Assert.Equal(hitPointsBefore - 5, npc.Being.HitPoints);
        Assert.False(npc.Being.HasEffectOrAccumulator(EffectType.Sedate));
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Sleepy.Name) > 0);
    }

    /// <summary>
    ///     A fascinated creature takes no action, planning to wait until the entrancement ends.
    /// </summary>
    [Fact]
    public void Fascinated_creature_plans_to_wait()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(ActorActionType.UseAbilitySlot, npc.AI!.NextAction);

        ApplyStatus(npc, EffectType.Fascinate, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Fascinated.Name));

        var playerHitPoints = playerEntity.Being!.HitPoints;

        TestHelper.PerformTurn(npc, manager);

        Assert.Equal(playerHitPoints, playerEntity.Being.HitPoints);
        Assert.Equal(ActorActionType.Wait, npc.AI.NextAction);
    }

    /// <summary>
    ///     A fascinated creature snaps out of the entrancement the instant it takes damage.
    /// </summary>
    [Fact]
    public void Fascinated_creature_snaps_out_when_damaged()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Fascinate, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(10, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Fascinated.Name));
        Assert.True(npc.Being!.HasEffectOrAccumulator(EffectType.Fascinate));

        var hitPointsBefore = npc.Being!.HitPoints;
        ApplyDamage(npc, amount: 5, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(hitPointsBefore - 5, npc.Being.HitPoints);
        Assert.False(npc.Being.HasEffectOrAccumulator(EffectType.Fascinate));
    }

    [Fact]
    public void Doomed_creature_dies_when_the_doom_exceeds_its_health()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);
        var npcId = npc.Id;

        ApplyStatus(npc, EffectType.Doom, duration: 100000, manager, amount: npc.Being!.HitPoints + 10);
        manager.Queue.ProcessQueue(manager);

        // The doom is only realized when the creature next acts.
        Assert.True(npc.Being.IsAlive);

        TestHelper.PerformTurn(npc, manager);

        Assert.False(manager.FindEntity(npcId)?.Being?.IsAlive == true);
    }

    [Fact]
    public void Overcharged_detonates_at_threshold_burning_energy_into_damage()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.GnomeWizard.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var being = npc.Being!;
        being.EnergyPoints = 5;
        being.HitPoints = being.HitPointMaximum;
        var energy = being.EnergyPoints;
        Assert.True(energy > 0);
        var hitPointsBefore = being.HitPoints;

        ApplyStatus(npc, EffectType.Overcharge, duration: 100000, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        // The charge only detonates when the creature next acts.
        Assert.Equal(energy, being.EnergyPoints);

        TestHelper.PerformTurn(npc, manager);

        // All energy is burnt away and turned into an equal amount of damage.
        Assert.Equal(0, being.EnergyPoints);
        Assert.Equal(hitPointsBefore - energy, being.HitPoints);
    }

    private static void ApplyDamage(GameEntity target, int amount, GameManager manager)
    {
        ApplyInstantDamageEffect(target, EffectType.PhysicalDamage, amount, manager);
    }

    private sealed class TestActorSystem : ActorSystemBase
    {
        public static ActorActionType RandomizeAction(
            GameEntity actorEntity, ActorActionType action, GameManager manager)
        {
            int? target = null;
            int? target2 = null;
            RandomizeActionTargetIfConfused(actorEntity, ref action, ref target, ref target2, manager);
            return action;
        }
    }

    private sealed class AttackListener : IGameSystem<AbilityActivatedMessage>
    {
        private readonly Action _onActivated;

        public AttackListener(Action onActivated) => _onActivated = onActivated;

        public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager state)
        {
            if (message.TargetEntity != null)
            {
                _onActivated();
            }

            return MessageProcessingResult.ContinueProcessing;
        }
    }
}
