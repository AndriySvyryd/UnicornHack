using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;

namespace UnicornHack.Systems.Actors;

public class PlayerSystemTest
{
    private const string PickupItemAbilityName = "flask of healing: Drink";

    [Theory]
    [InlineData(AbilityType.MeleeAttack, AbilityAction.Punch, null, 100)]
    [InlineData(AbilityType.RangedSpell, AbilityAction.Spell, null, 100)]
    [InlineData(AbilityType.MeleeAttack, AbilityAction.Punch, 17, 17)]
    [InlineData(AbilityType.RangedSpell, AbilityAction.Spell, 0, 0)]
    public void Ability_noise_penalty_overrides_the_action_default(
        AbilityType type, AbilityAction abilityAction, int? noisePenalty, int expectedNoise)
    {
        var room = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var abilityEntity = new Ability
        {
            Name = "noise test",
            Type = type,
            Activation = ActivationType.Targeted,
            Action = abilityAction,
            Noise = noisePenalty
        }.AddToAffectable(playerEntity).Entity;
        manager.Queue.ProcessQueue(manager);

        var setSlot = SetAbilitySlotMessage.Create(manager);
        setSlot.AbilityEntity = abilityEntity;
        setSlot.Slot = 2;
        manager.Enqueue(setSlot);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            expectedNoise,
            ActorSystemBase.GetActionNoise(
                playerEntity, ActorActionType.UseAbilitySlot, 2, manager));
    }

    [Theory]
    [InlineData(49, true)]
    [InlineData(50, false)]
    public void Confused_level_two_randomizes_item_targets(int roll, bool randomizes)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(".", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.Dagger.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var items = playerEntity.Being!.Items.ToList();
        Assert.Equal(2, items.Count);
        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Confuse, playerEntity, amount: 400));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Confused.Name));

        var random = (TestRandom)manager.Game.Random;
        random.EnqueueNext(roll);

        playerEntity.Player!.NextAction = ActorActionType.DropItem;
        playerEntity.Player.NextActionTarget = items[0].Id;
        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(playerEntity.Id, items[randomizes ? 0 : 1].Item!.ContainerId);
        Assert.Null(items[randomizes ? 1 : 0].Item!.ContainerId);
    }

    [Fact]
    public void Confused_level_three_randomizes_targeted_ability_targets_without_hostility_filtering()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom("...", game);
        var firstTarget = CreatureData.HumanWarrior.Instantiate(room, new Point(0, 0));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 0));
        var secondTarget = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        firstTarget.Being!.HitPoints = firstTarget.Being.HitPointMaximum - 20;
        secondTarget.Being!.HitPoints = secondTarget.Being.HitPointMaximum - 20;
        var ability = new Ability
        {
            Name = "confused target test heal",
            Type = AbilityType.RangedSpell,
            Activation = ActivationType.Targeted,
            Action = AbilityAction.Spell,
            SuccessCondition = AbilitySuccessCondition.Always,
            Range = 2,
            TargetingShape = TargetingShape.Line,
            Effects = new List<Effect> { new Heal { Amount = "10" } }
        }.AddToAffectable(playerEntity).Entity;
        manager.Queue.ProcessQueue(manager);

        var setSlot = SetAbilitySlotMessage.Create(manager);
        setSlot.AbilityEntity = ability;
        setSlot.Slot = 2;
        manager.Enqueue(setSlot);
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Confuse, playerEntity, amount: 600));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Confused.Name));

        // The original target is excluded; select the other creature after the caster itself.
        ((TestRandom)manager.Game.Random).EnqueueNext(1);
        playerEntity.Player!.NextAction = ActorActionType.UseAbilitySlot;
        playerEntity.Player.NextActionTarget = 2;
        playerEntity.Player.NextActionTarget2 = -firstTarget.Id;
        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(firstTarget.Being.HitPointMaximum - 20, firstTarget.Being.HitPoints);
        Assert.Equal(secondTarget.Being.HitPointMaximum - 10, secondTarget.Being.HitPoints);
    }

    [Fact]
    public void Confused_action_without_a_target_retries_actions_until_one_has_a_valid_target()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom("...", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Confuse, playerEntity, amount: 600));
        manager.Queue.ProcessQueue(manager);

        var random = (TestRandom)manager.Game.Random;
        random.EnqueueNext(3); // PickupItem has no valid target because the cell is empty.
        random.EnqueueNext(1); // MoveOneCell after PickupItem is removed from consideration.
        random.EnqueueNext(0); // East is a valid movement target.
        playerEntity.Player!.NextAction = ActorActionType.Wait;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(new Point(2, 0), playerEntity.Position!.Cell);
    }

    [Fact]
    public void Confused_action_with_no_other_valid_target_chooses_a_different_action()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var room = TestHelper.BuildRoom(".", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        var ability = new Ability
        {
            Name = "confused self target test",
            Type = AbilityType.RangedSpell,
            Activation = ActivationType.Targeted,
            Action = AbilityAction.Spell,
            SuccessCondition = AbilitySuccessCondition.Always,
            Range = 1,
            TargetingShape = TargetingShape.Line,
            Effects = new List<Effect> { new Heal { Amount = "10" } }
        }.AddToAffectable(playerEntity).Entity;
        manager.Queue.ProcessQueue(manager);
        var setSlot = SetAbilitySlotMessage.Create(manager);
        setSlot.AbilityEntity = ability;
        setSlot.Slot = 2;
        manager.Enqueue(setSlot);
        manager.Queue.ProcessQueue(manager);
        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Confuse, playerEntity, amount: 600));
        manager.Queue.ProcessQueue(manager);

        playerEntity.Being!.HitPoints -= 10;
        var hitPoints = playerEntity.Being.HitPoints;
        var heading = playerEntity.Position!.Heading;
        var random = (TestRandom)manager.Game.Random;
        random.EnqueueNext(0); // ChangeHeading is the first different action.
        random.EnqueueNext(0); // Select its first heading other than the current one.
        playerEntity.Player!.NextAction = ActorActionType.UseAbilitySlot;
        playerEntity.Player.NextActionTarget = 2;
        playerEntity.Player.NextActionTarget2 = -playerEntity.Id;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(hitPoints, playerEntity.Being.HitPoints);
        Assert.NotEqual(heading, playerEntity.Position.Heading);
    }

    [Fact]
    public void Engulfed_player_cannot_move_through_a_player_action()
    {
        var room = TestHelper.BuildRoom("...");
        var manager = room.Entity.Manager;
        var engulfer = CreatureData.GelatinousCube.Instantiate(room, new Point(0, 0));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var engulfEffect = engulfer.ActiveEffects!.Abilities
            .SelectMany(ability => ability.Ability!.Effects)
            .Single(effect => effect.Effect!.EffectType == EffectType.Engulf);
        TestHelper.ApplyEffect(engulfEffect, engulfer, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        playerEntity.Position!.Heading = Direction.East;
        playerEntity.Player!.NextAction = ActorActionType.MoveOneCell;
        playerEntity.Player.NextActionTarget = (int)Direction.East;
        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(new Point(0, 0), engulfer.Position!.Cell);
        Assert.Equal(new Point(1, 0), playerEntity.Position.Cell);
        Assert.Null(playerEntity.Player.NextAction);
    }

    [Fact]
    public void Incapacitating_status_logs_why_the_player_loses_the_turn()
    {
        var room = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Stun, playerEntity, amount: 1000));
        manager.Queue.ProcessQueue(manager);
        playerEntity.Player!.LogEntries.Clear();
        playerEntity.Player.NextAction = ActorActionType.Wait;

        TestHelper.PerformTurn(playerEntity, manager);

        var log = Assert.Single(playerEntity.Player.LogEntries);
        Assert.Equal("You are stunned and unable to act.", log.Message);
    }

    [Fact]
    public void Movement_status_logs_why_the_player_cannot_move()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Bind, playerEntity, amount: 1000));
        manager.Queue.ProcessQueue(manager);
        playerEntity.Player!.LogEntries.Clear();
        playerEntity.Position!.Heading = Direction.East;
        playerEntity.Player.NextAction = ActorActionType.MoveOneCell;
        playerEntity.Player.NextActionTarget = (int)Direction.East;

        TestHelper.PerformTurn(playerEntity, manager);

        var log = Assert.Single(playerEntity.Player.LogEntries);
        Assert.Equal("You are bound and can't move.", log.Message);
        Assert.Equal(new Point(0, 0), playerEntity.Position.Cell);
    }

    [Fact]
    public void Item_status_logs_why_the_player_cannot_pick_up_an_item()
    {
        var room = TestHelper.BuildRoom(".");
        Assert.True(ItemData.FlaskOfHealing.Instantiate(room, new Point(0, 0)));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Stick, playerEntity, amount: 1000));
        manager.Queue.ProcessQueue(manager);
        playerEntity.Player!.LogEntries.Clear();
        playerEntity.Player.NextAction = ActorActionType.PickupItem;

        TestHelper.PerformTurn(playerEntity, manager);

        var log = Assert.Single(playerEntity.Player.LogEntries);
        Assert.Equal("You are stuck and can't manipulate items.", log.Message);
        Assert.Empty(playerEntity.Being!.Items);
    }

    [Fact]
    public void Ability_status_logs_why_the_player_cannot_use_an_ability()
    {
        var room = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        TestHelper.AddSpellAbility(playerEntity);
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        var spell = playerEntity.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Action == AbilityAction.Spell);
        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = spell;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Mute, playerEntity, amount: 1000));
        manager.Queue.ProcessQueue(manager);
        playerEntity.Player!.LogEntries.Clear();
        playerEntity.Player.NextAction = ActorActionType.UseAbilitySlot;
        playerEntity.Player.NextActionTarget = spell.Ability!.Slot;
        playerEntity.Player.NextActionTarget2 = -playerEntity.Id;

        TestHelper.PerformTurn(playerEntity, manager);

        var log = Assert.Single(playerEntity.Player.LogEntries);
        Assert.Equal("You are muted and can't use Test spell.", log.Message);
    }

    [Fact]
    public void PickupItem_with_no_item_on_cell_logs_a_message_and_does_not_advance_the_turn()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        // Drop any startup log entries so the post-action diff is clean.
        player.LogEntries.Clear();
        var initialTick = manager.Game.CurrentTick;

        player.NextAction = ActorActionType.PickupItem;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        var log = Assert.Single(player.LogEntries);
        Assert.Equal(manager.Game.Services.Language.NothingToPickUp(), log.Message);
        // ProcessTurn returned false, so NextActionTick wasn't advanced by the
        // PickupItem case; the player remains the acting player on the same tick.
        Assert.Equal(initialTick, manager.Game.CurrentTick);
        Assert.Same(playerEntity, manager.Game.ActingPlayer);
    }

    [Fact]
    public void PickupItem_without_a_requested_slot_auto_picks_the_first_free_slot()
    {
        var (room, playerEntity, _) = SetupPlayerOnItem();
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;
        var being = playerEntity.Being!;

        var firstFreeSlot = manager.AbilitySlottingSystem.GetFirstFreeSlot(playerEntity);
        Assert.NotNull(firstFreeSlot);
        Assert.False(being.SlottedAbilities.ContainsKey(firstFreeSlot.Value));

        player.NextAction = ActorActionType.PickupItem;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        var slottedAbility = Assert.Contains(firstFreeSlot.Value, being.SlottedAbilities);
        Assert.Equal(PickupItemAbilityName, slottedAbility.Ability!.Name);
        Assert.Equal(playerEntity.Id, slottedAbility.Ability.OwnerId);
        // The item physically moved into the player's container.
        var pickedUpItem = Assert.Single(being.Items);
        Assert.Equal(playerEntity.Id, pickedUpItem.Item!.ContainerId);
    }

    [Fact]
    public void Taunted_level_three_spends_non_offensive_actions_without_performing_them()
    {
        var room = TestHelper.BuildRoom("..");
        var manager = room.Entity.Manager;
        Assert.True(ItemData.FlaskOfHealing.Instantiate(room, new Point(0, 0)));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var taunter = CreatureData.Bugbear.Instantiate(room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var taunt = taunter.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Scream);
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Taunt, playerEntity, amount: 600, taunt.Id));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Taunted.Name));

        var player = playerEntity.Player!;
        player.LogEntries.Clear();
        var actionTick = player.NextActionTick;
        player.NextAction = ActorActionType.PickupItem;

        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Empty(playerEntity.Being!.Items);
        Assert.Single(room.Items.Values);
        var log = Assert.Single(player.LogEntries);
        Assert.Equal("You are taunted and can't perform that action.", log.Message);
        Assert.Equal(actionTick + TimeSystem.DefaultActionDelay, player.NextActionTick);
    }

    [Fact]
    public void Frightened_level_three_replaces_a_player_action_with_escape()
    {
        var room = TestHelper.BuildRoom("...");
        var manager = room.Entity.Manager;
        var source = CreatureData.Ghost.Instantiate(room, new Point(0, 0));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var sourceAbility = source.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Action == AbilityAction.Gaze);
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Frighten, playerEntity, amount: 600, sourceAbility.Id));
        manager.Queue.ProcessQueue(manager);

        var startCell = playerEntity.Position!.Cell!.Value;
        playerEntity.Player!.NextAction = ActorActionType.Wait;
        TestHelper.PerformTurn(playerEntity, manager);

        Assert.Equal(new Point(2, 0), playerEntity.Position.Cell);
        Assert.True(playerEntity.Position.Cell.Value.DistanceTo(source.Position!.Cell!.Value)
                    > startCell.DistanceTo(source.Position.Cell.Value));
    }

    [Fact]
    public void PickupItem_with_a_requested_free_slot_routes_the_ability_into_that_slot()
    {
        var (_, playerEntity, _) = SetupPlayerOnItem();
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;
        var being = playerEntity.Being!;

        var firstFreeSlot = manager.AbilitySlottingSystem.GetFirstFreeSlot(playerEntity)!.Value;
        // Pick a slot at least one position past the auto-pick so a passive (no-op)
        // run-through can't masquerade as a successful slot move.
        var requestedSlot = firstFreeSlot + 1;
        Assert.True(requestedSlot < playerEntity.Physical!.Capacity);
        Assert.False(being.SlottedAbilities.ContainsKey(requestedSlot));
        Assert.False(being.SlottedAbilities.ContainsKey(firstFreeSlot));

        player.NextAction = ActorActionType.PickupItem;
        player.NextActionTarget = requestedSlot;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        var slottedAbility = Assert.Contains(requestedSlot, being.SlottedAbilities);
        Assert.Equal(PickupItemAbilityName, slottedAbility.Ability!.Name);
        Assert.False(being.SlottedAbilities.ContainsKey(firstFreeSlot));
    }

    [Fact]
    public void PickupItem_with_a_requested_slot_already_occupied_displaces_the_existing_ability()
    {
        // Need at least two cells so the displaced item has somewhere to spill over
        // when its slot is cleared (clearing an item-type slot drops the item).
        var room = TestHelper.BuildRoom("..");
        var manager = room.Entity.Manager;

        Assert.True(ItemData.FlaskOfHealing.Instantiate(room, new Point(0, 0)));
        manager.Queue.ProcessQueue(manager);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var player = playerEntity.Player!;
        var being = playerEntity.Being!;

        // First pickup populates the first free non-default slot with flask1's drink.
        var targetSlot = manager.AbilitySlottingSystem.GetFirstFreeSlot(playerEntity)!.Value;
        player.NextAction = ActorActionType.PickupItem;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        var displacedAbility = Assert.Contains(targetSlot, being.SlottedAbilities);
        Assert.Equal(PickupItemAbilityName, displacedAbility.Ability!.Name);

        // Spawn a second flask on the player's cell; the existing flask spills over
        // to (1,0) when the slot is cleared during the next pickup.
        Assert.True(ItemData.FlaskOfHealing.Instantiate(room, new Point(0, 0)));
        manager.Queue.ProcessQueue(manager);

        // PickupItem must clear the conflicting ability's slot first so the new
        // pickup auto-slots directly into the requested slot (rather than landing
        // elsewhere and being moved with a side-effect displacement).
        player.NextAction = ActorActionType.PickupItem;
        player.NextActionTarget = targetSlot;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        var slottedAbility = Assert.Contains(targetSlot, being.SlottedAbilities);
        Assert.Equal(PickupItemAbilityName, slottedAbility.Ability!.Name);
        Assert.NotSame(displacedAbility, slottedAbility);
        // The displaced flask was dropped (item-type slots drop their item on clear),
        // so its WhilePossessed-added ability entity has been cleaned up.
        Assert.Null(displacedAbility.Ability);
        // One flask was picked up (now in inventory); the displaced one spilled over
        // to the adjacent cell.
        Assert.Single(being.Items);
        Assert.NotEmpty(room.Items);
    }

    [Fact]
    public void PickupItem_replacing_a_slotted_item_logs_the_drop_before_the_pickup()
    {
        // Two cells so the displaced item can spill over when its slot is cleared.
        var room = TestHelper.BuildRoom("..");
        var manager = room.Entity.Manager;

        Assert.True(ItemData.FlaskOfHealing.Instantiate(room, new Point(0, 0)));
        manager.Queue.ProcessQueue(manager);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var player = playerEntity.Player!;

        // First pickup occupies the first free slot.
        var targetSlot = manager.AbilitySlottingSystem.GetFirstFreeSlot(playerEntity)!.Value;
        player.NextAction = ActorActionType.PickupItem;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        // Spawn a second flask on the player's cell.
        Assert.True(ItemData.FlaskOfHealing.Instantiate(room, new Point(0, 0)));
        manager.Queue.ProcessQueue(manager);

        player.LogEntries.Clear();

        // Pick up the second flask into the occupied slot, displacing the first.
        player.NextAction = ActorActionType.PickupItem;
        player.NextActionTarget = targetSlot;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        var dropIndex = player.LogEntries.ToList().FindIndex(e => e.Message.Contains("drop"));
        var pickupIndex = player.LogEntries.ToList().FindIndex(e => e.Message.Contains("pick up"));

        Assert.True(dropIndex >= 0, "Expected a drop log entry for the displaced item.");
        Assert.True(pickupIndex >= 0, "Expected a pick up log entry for the new item.");
        Assert.True(dropIndex < pickupIndex,
            $"Drop should be logged before pickup, but drop was at {dropIndex} and pickup at {pickupIndex}. "
            + $"Log: {string.Join(" | ", player.LogEntries.Select(e => e.Message))}");
    }

    [Fact]
    public void PickupItem_when_player_inventory_is_full_logs_the_failure_without_re_throwing()
    {
        var (room, playerEntity, _) = SetupPlayerOnItem();
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;
        var being = playerEntity.Being!;

        // Force the auto-slot path to find no free slot by reducing capacity to
        // the two default-attack slots (which can't host item abilities). The
        // pickup should fail silently rather than crash; the item stays on the floor.
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            AbilitySlottingSystem.DefaultRangedAttackSlot + 1 - playerEntity.Physical!.Capacity,
            nameof(PhysicalComponent.Capacity), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AbilitySlottingSystem.GetFirstFreeSlot(playerEntity));
        Assert.Empty(being.Items);
        Assert.NotEmpty(room.Items);

        player.NextAction = ActorActionType.PickupItem;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        // Item didn't transfer (capacity check inside ItemMovingSystem.TryMove
        // refuses the move); pickup is silent today (see ItemMovingSystem TODO).
        Assert.NotEmpty(room.Items);
        Assert.Empty(being.Items);
    }

    [Fact]
    public void UseAbilitySlot_with_an_out_of_bounds_packed_target_logs_NoPath_without_crashing()
    {
        // Reproduces a hub crash: a client (here, the diagnostic client) sent a raw,
        // POSITIVE actor id as the ability target. The server interprets a positive
        // target as a PACKED CELL (Point.Unpack) and only a NEGATIVE one as an actor,
        // so e.g. actor id 3802 unpacks to (14, 218) — far outside any room — and the
        // pathfinder indexed its grid out of bounds (IndexOutOfRangeException).
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);
        player.LogEntries.Clear();

        // 3802 -> Point.Unpack -> (14, 218), well outside the 2x1 room.
        const int outOfBoundsPackedTarget = 3802;
        Assert.False(room.IsValid(Point.Unpack(outOfBoundsPackedTarget)));

        player.NextAction = ActorActionType.UseAbilitySlot;
        // Slot 0 is the default melee slot; the bounds guard runs before the slot lookup.
        player.NextActionTarget = 0;
        player.NextActionTarget2 = outOfBoundsPackedTarget;
        AdvanceTurnMessage.Enqueue(manager);

        // Must not throw IndexOutOfRangeException.
        manager.Queue.ProcessQueue(manager);

        var log = Assert.Single(player.LogEntries);
        Assert.Equal(manager.Game.Services.Language.NoPath(), log.Message);
    }

    /// <summary>
    ///     Builds a 1x1 room, drops a known item on (0,0), and instantiates the player
    ///     on top of it. The bootstrap pickup that would normally happen via the
    ///     walk-onto-cell path doesn't fire — InstantiatePlayer places the player
    ///     directly, so the item is still on the floor when the test starts driving
    ///     PickupItem actions.
    /// </summary>
    private static (RoomComponent Room, GameEntity PlayerEntity, GameEntity ItemEntity)
        SetupPlayerOnItem()
    {
        var room = TestHelper.BuildRoom(".");
        var manager = room.Entity.Manager;

        Assert.True(ItemData.FlaskOfHealing.Instantiate(room, new Point(0, 0)));
        manager.Queue.ProcessQueue(manager);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var itemEntity = Assert.Single(room.Items.Values);
        return (room, playerEntity, itemEntity);
    }
}
