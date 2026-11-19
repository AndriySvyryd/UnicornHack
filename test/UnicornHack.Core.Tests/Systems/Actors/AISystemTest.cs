using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Data.Features;
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
    [Fact]
    public void AI_rechecks_a_queued_move_that_would_enter_a_trap()
    {
        var room = TestHelper.BuildRoom(@".....
.....
.....");
        var actor = CreatureData.Boggart.Instantiate(room, new Point(1, 1));
        PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(4, 1));
        var manager = room.Entity.Manager;
        FeatureData.SpikeTrap.Instantiate(room, new Point(2, 1));
        manager.Queue.ProcessQueue(manager);
        actor.Position!.Heading = Direction.East;
        actor.AI!.NextAction = ActorActionType.MoveOneCell;
        actor.AI.NextActionTarget = (int)Direction.East;
        var initialHp = actor.Being!.HitPoints;

        TestHelper.PerformTurn(actor, manager);

        Assert.NotEqual(new Point(2, 1), actor.Position.Cell);
        Assert.Equal(initialHp, actor.Being.HitPoints);
    }

    [Fact]
    public void AI_chooses_a_safe_exit_from_a_launcher_attack_lane()
    {
        var room = TestHelper.BuildRoom(@".....
.....
.....
.....
.....");
        var actor = CreatureData.Boggart.Instantiate(room, new Point(1, 2));
        PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        FeatureData.FireboltLauncher.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);
        DecideNextActionMessage.Enqueue(actor, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(actor.AI!.NextAction is ActorActionType.ChangeHeading or ActorActionType.MoveOneCell);
        var target = actor.Position!.Cell!.Value.Translate(((Direction)actor.AI.NextActionTarget!.Value).AsVector());
        Assert.DoesNotContain(target, room.CellThreats!.Keys);
    }

    [Fact]
    public void AI_ability_windup_is_added_once_and_the_queued_attack_resolves()
    {
        var room = TestHelper.BuildRoom("...");
        var actor = CreatureData.ElfScout.Instantiate(room, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(1, 0));
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);
        var shot = manager.AffectableAbilitiesIndex[(actor.Id, "elf-shot")]!.Ability!;
        foreach (var candidate in actor.ActiveEffects!.Abilities)
        {
            if (candidate.Ability!.Activation == ActivationType.Targeted && candidate.Ability != shot)
            {
                candidate.Ability.IsUsable = false;
            }
        }
        manager.Queue.ProcessQueue(manager);
        var previousAttack = new Ability
        {
            Name = "test previous planned attack", Type = AbilityType.RangedAttack,
            TargetingShape = TargetingShape.Square, Range = 2, Effects = []
        }.AddToAffectable(actor);
        manager.Queue.ProcessQueue(manager);
        previousAttack.Slot = 5;
        actor.Position!.Heading = Direction.East;
        actor.AI!.NextAction = ActorActionType.UseAbilitySlot;
        actor.AI.NextActionTarget = previousAttack.Slot;
        actor.AI.NextActionTarget2 = new Vector(1, 0).ToInt32();
        Assert.Contains(player.Position!.Cell!.Value, room.CellThreats!.Keys);
        Assert.NotNull(actor.AI.ThreatenedCells);
        actor.AI.NextActionTick = manager.Game.CurrentTick + 50;
        var initialTick = actor.AI.NextActionTick;
        var initialHp = player.Being!.HitPoints;
        var initialEp = actor.Being!.EnergyPoints;
        DecideNextActionMessage.Enqueue(actor, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.UseAbilitySlot, actor.AI.NextAction);
        Assert.Equal(shot.Slot, actor.AI.NextActionTarget);
        Assert.Equal(new Vector(1, 0).ToInt32(), actor.AI.NextActionTarget2);
        Assert.Equal(initialTick + shot.Template!.Windup, actor.AI.NextActionTick);
        Assert.Equal(initialHp, player.Being.HitPoints);
        Assert.Equal(initialEp, actor.Being.EnergyPoints);
        Assert.NotNull(actor.AI.ThreatenedCells);
        var scheduledTick = actor.AI.NextActionTick!.Value;
        _ = room.CellThreats;
        var cachedCells = actor.AI.ThreatenedCells;
        Assert.NotNull(cachedCells);

        DecideNextActionMessage.Enqueue(actor, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(scheduledTick, actor.AI.NextActionTick);
        Assert.Same(cachedCells, actor.AI.ThreatenedCells);
        var threats = room.CellThreats!;
        var sources = threats[player.Position.Cell.Value];
        DelayMessage.Enqueue(actor, 50, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(scheduledTick + 50, actor.AI.NextActionTick);
        Assert.Same(cachedCells, actor.AI.ThreatenedCells);
        Assert.Same(sources, threats[player.Position.Cell.Value]);
        scheduledTick = actor.AI.NextActionTick!.Value;

        shot.CooldownTick = 100;
        player.Player!.NextActionTick = 1000;
        _ = room.CellThreats;
        cachedCells = actor.AI.ThreatenedCells;
        sources = threats[player.Position.Cell.Value];
        Assert.Equal(TurnResult.Continue, manager.TimeSystem.AdvanceSingleTurn(manager));
        Assert.Null(shot.CooldownTick);
        Assert.Same(cachedCells, actor.AI.ThreatenedCells);
        Assert.Same(sources, threats[player.Position.Cell.Value]);
        Assert.Same(cachedCells, manager.CellThreatTracker.GetCachedTargetCells(actor, shot.EntityId, player.Position.Cell.Value));

        shot.SuccessCondition = AbilitySuccessCondition.Always;

        TestHelper.PerformTurn(actor, manager);

        Assert.Equal(TargetingShape.Line, shot.TargetingShape);
        Assert.Equal(scheduledTick, manager.Game.CurrentTick);
        Assert.True(player.Being.HitPoints < initialHp);
        Assert.True(shot.CooldownTick > manager.Game.CurrentTick);
        Assert.True(actor.AI.NextActionTick > scheduledTick);
    }

    [Fact]
    public void Embedded_creature_ability_templates_resolve_from_the_owner_after_reload()
    {
        var room = TestHelper.BuildRoom(".");
        var actor = CreatureData.Boggart.Instantiate(room, new Point(0, 0));
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);
        manager.AffectableAbilitiesIndex[(actor.Id, "bruising punch")]!.Ability!.Name = "previous bruising punch";
        manager.Queue.ProcessQueue(manager);
        using var reference = manager.CreateEntity();
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = "bruising punch";
        ability.OwnerEntity = actor;
        reference.Referenced.Ability = ability;

        Assert.Same(CreatureData.Boggart.Abilities.Single(template => template.Name == "bruising punch"),
            ability.Template);
    }

    [Theory]
    [InlineData(AIBehavior.Peaceful)]
    [InlineData(AIBehavior.Peaceful | AIBehavior.RangedPeaceful)]
    public void Player_attacks_remove_peaceful_flags_without_changing_other_behavior(AIBehavior peacefulBehavior)
    {
        var room = TestHelper.BuildRoom("..");
        var npc = CreatureData.Boggart.Instantiate(room, new Point(1, 0));
        var otherBehavior = AIBehavior.NonRecruitable | AIBehavior.Stalking;
        npc.AI!.Behavior = peacefulBehavior | otherBehavior;
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.False(AISystem.IsHostileToPlayers(npc));
        Assert.Equal(ActorActionType.Wait, npc.AI.NextAction);

        var ability = new Ability
        {
            Name = "test provocation",
            Type = AbilityType.MeleeAttack,
            Activation = ActivationType.Manual,
            Effects = []
        }.AddToAffectable(player);
        manager.Queue.ProcessQueue(manager);
        var attack = AbilityActivatedMessage.Create(manager);
        attack.ActivatorEntity = player;
        attack.TargetEntity = npc;
        attack.AbilityEntity = ability.Entity;
        attack.EffectsToApply = System.Collections.Immutable.ImmutableList<GameEntity>.Empty;
        attack.Outcome = ApplicationOutcome.Success;
        manager.EffectApplicationSystem.Process(attack, manager);
        manager.ReturnMessage(attack);

        Assert.Equal(otherBehavior, npc.AI.Behavior);
        Assert.True(AISystem.IsHostileToPlayers(npc));
    }

    private const string Map = @"
.....
.....
.....
.....
.....";

    [Fact]
    public void Digestion_attack_uses_its_normal_delay_against_the_engulfed_victim()
    {
        var room = TestHelper.BuildRoom("...");
        var manager = room.Entity.Manager;
        var engulfer = CreatureData.Sylph.Instantiate(room, new Point(0, 0));
        var victim = CreatureData.Manusya.Instantiate(room, new Point(1, 0));
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

        Assert.True(victim.Being!.HasEffectOrAccumulator(EffectType.Deafen));
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
        var npc = CreatureData.Manusya.Instantiate(otherRoom, new Point(2, 2));
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
        var npc = CreatureData.Manusya.Instantiate(otherRoom, new Point(0, 0));
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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(0, 0));
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
        var actor = CreatureData.Manusya.Instantiate(room, new Point(0, 1));
        actor.Position!.Heading = Direction.East;
        CreatureData.Manusya.Instantiate(room, new Point(1, 1));
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
        var creature = CreatureData.Manusya.Instantiate(room, new Point(1, 0));
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
            entry => entry.Message == $"The manusya tries to {actionText}, but fails.");
    }

    [Fact]
    public void Failed_item_equipment_messages_always_add_the_default_delay()
    {
        var room = TestHelper.BuildRoom(".");
        var creature = CreatureData.Manusya.Instantiate(room, new Point(0, 0));
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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(1, 0));
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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(1, 0));
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
        Assert.False(AbilityLevelSystem.GetAbilityLevel(npc, AbilityData.Paralyzed.Name) > 0);
        Assert.False(npc.ActiveEffects.Accumulators.ContainsKey(EffectType.Paralyze));
        Assert.Equal(ActorActionType.UseAbilitySlot, npc.AI.NextAction);

        // The attack is now performed.
        var attacks = 0;
        var listener = new AttackListener(() => attacks++);
        manager.Queue.Register(listener, AbilityActivatedMessage.Name, -1);

        TestHelper.PerformTurn(npc, manager);
        Assert.True(attacks > 0);
    }

    [Fact]
    public void Frightened_level_two_ai_flees_from_a_perceived_source()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(Map, game);
        var source = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 2));
        var actor = CreatureData.Manusya.Instantiate(room, new Point(2, 2));
        actor.Position!.Heading = Direction.East;
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);

        var sourceAbility = source.ActiveEffects!.Abilities.First();
        ApplyCompulsion(actor, EffectType.Frighten, sourceAbility, manager, amount: 200);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(actor, AbilityData.Frightened.Name));
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
        var actor = CreatureData.Manusya.Instantiate(room, new Point(2, 2));
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);

        ApplyCompulsion(actor, EffectType.Frighten, player.ActiveEffects!.Abilities.First(), manager, amount: 300);
        Assert.Equal(3, AbilityLevelSystem.GetAbilityLevel(actor, AbilityData.Frightened.Name));

        DecideNextActionMessage.Enqueue(actor, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ActorActionType.MoveOneCell, actor.AI!.NextAction);
        var targetCell = actor.Position!.Cell!.Value.Translate(
            ((Direction)actor.AI.NextActionTarget!.Value).AsVector());
        Assert.True(targetCell.DistanceTo(player.Position!.Cell!.Value)
                    > actor.Position.Cell.Value.DistanceTo(player.Position.Cell.Value));
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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);
        var basePerception = npc.Being!.Perception;

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        ApplyStatus(npc, EffectType.Sedate, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(npc, AbilityData.Sleepy.Name));
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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        ApplyStatus(npc, EffectType.Sedate, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(npc, AbilityData.Sleepy.Name));

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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Sedate, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(10, AbilityLevelSystem.GetAbilityLevel(npc, AbilityData.Sleepy.Name));

        var hitPointsBefore = npc.Being!.HitPoints;
        ApplyDamage(npc, amount: 5, manager);
        manager.Queue.ProcessQueue(manager);

        // The damage landed and the sleep effect was removed.
        Assert.Equal(hitPointsBefore - 5, npc.Being.HitPoints);
        Assert.False(npc.Being.HasEffectOrAccumulator(EffectType.Sedate));
        Assert.False(AbilityLevelSystem.GetAbilityLevel(npc, AbilityData.Sleepy.Name) > 0);
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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        DecideNextActionMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(ActorActionType.UseAbilitySlot, npc.AI!.NextAction);

        ApplyStatus(npc, EffectType.Fascinate, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(npc, AbilityData.Fascinated.Name));

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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Fascinate, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(10, AbilityLevelSystem.GetAbilityLevel(npc, AbilityData.Fascinated.Name));
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

        var npc = CreatureData.Manusya.Instantiate(room, new Point(2, 2));
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

        var npc = CreatureData.BoggartShaman.Instantiate(room, new Point(2, 2));
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
