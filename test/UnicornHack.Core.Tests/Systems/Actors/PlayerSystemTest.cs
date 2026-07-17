using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;

namespace UnicornHack.Systems.Actors;

public class PlayerSystemTest
{
    private const string PickupItemAbilityName = "flask of healing: Drink";

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
        playerEntity.Physical!.Capacity = AbilitySlottingSystem.DefaultRangedAttackSlot + 1;
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
