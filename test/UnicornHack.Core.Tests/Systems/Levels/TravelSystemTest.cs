using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Time;

namespace UnicornHack.Systems.Levels;

public class TravelSystemTest
{
    private static void Engulf(GameEntity engulfer, GameEntity victim, GameManager manager)
    {
        var engulfEffect = engulfer.ActiveEffects!.Abilities
                .SelectMany(a => a.Ability!.Effects)
                .Single(e => e.Effect!.EffectType == EffectType.Engulf);

        TestHelper.ApplyEffect(engulfEffect, engulfer, victim, manager);
        manager.Queue.ProcessQueue(manager);
    }

    [Fact]
    public void GetPossibleMovementDirections_returns_correct_directions()
    {
        var level = TestHelper.BuildRoom(@"
.#.
.>.
...");
        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 1));
        var manager = undine.Manager;
        PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));

        manager.Queue.ProcessQueue(manager);

        Direction[] expected = [Direction.North, Direction.Southeast];
        Assert.Equal(
            expected,
            manager.TravelSystem.GetPossibleMovementDirections(undine.Position!, safe: true));

        Span<Direction> directions = stackalloc Direction[8];
        var directionCount = manager.TravelSystem.GetPossibleMovementDirections(
            undine.Position!, safe: true, directions);
        Assert.Equal(expected, directions[..directionCount].ToArray());
    }

    [Fact]
    public void Can_travel_to_other_levels()
    {
        var level1 = TestHelper.BuildRoom(@"
.#.
.>.
...");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level1, new Point(1, 0));
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        var level2 = TestHelper.BuildRoom(@"
...
..#
..<", game: manager.Game, depth: 1);

        var connection1Entity = level1.Connections.GetValueOrDefault(new Point(1, 1))!;
        var connection1 = connection1Entity.Connection!;

        var connection2Entity = level2.Connections.GetValueOrDefault(new Point(2, 2))!;

        var targetCell = connection2Entity.Position!.Cell;
        connection1.TargetRoomId = level2.EntityId;
        connection1.TargetCell = targetCell;

        var roomBound = manager.EffectApplicationSystem.AddAbilityEffect(
            player, new Ability { Name = "room-bound test", Type = AbilityType.Buff });
        roomBound.ExpirationEvent = EffectDuration.UntilNextRoom;
        var levelBound = manager.EffectApplicationSystem.AddAbilityEffect(
            player, new Ability { Name = "level-bound test", Type = AbilityType.Buff });
        levelBound.ExpirationEvent = EffectDuration.UntilNextLevel;

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.South;
        travelMessage.TargetCell = connection1Entity.Position!.Cell!.Value;
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(level2.EntityId, player.Position!.RoomId);
        // level1 is a different branch from level2, so travelling there deletes level1 entirely
        // (including connection1); compare against the cell captured before the move.
        Assert.Equal(targetCell, player.Position.Cell);
        Assert.DoesNotContain(roomBound, player.ActiveEffects!.ComponentEffects);
        Assert.DoesNotContain(levelBound, player.ActiveEffects.ComponentEffects);
    }

    [Fact]
    public void Traveling_to_another_level_updates_room_of_carried_items()
    {
        var level1 = TestHelper.BuildRoom(@"
...
.>.
...");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level1, new Point(0, 0));
        var manager = player.Manager;
        Assert.True(ItemData.Dagger.Instantiate(level1, new Point(1, 0)));

        manager.Queue.ProcessQueue(manager);

        // Walk east onto the dagger's cell to pick it up.
        var pickupMessage = TravelMessage.Create(manager);
        pickupMessage.ActorEntity = player;
        pickupMessage.TargetHeading = Direction.East;
        pickupMessage.TargetCell = new Point(1, 0);
        manager.Enqueue(pickupMessage);

        manager.Queue.ProcessQueue(manager);

        // A carried item keeps its Position with a null cell and its holder's room.
        var daggerEntity = player.Being!.Items.Single();
        Assert.Equal(player.Id, daggerEntity.Item!.ContainerId);
        Assert.Null(daggerEntity.Position!.Cell);
        Assert.Equal(level1.EntityId, daggerEntity.Position!.RoomId);

        var level2 = TestHelper.BuildRoom(@"
...
..#
..<", game: manager.Game, depth: 1);

        var connection1Entity = level1.Connections.GetValueOrDefault(new Point(1, 1))!;
        var connection1 = connection1Entity.Connection!;

        var connection2Entity = level2.Connections.GetValueOrDefault(new Point(2, 2))!;

        connection1.TargetRoomId = level2.EntityId;
        connection1.TargetCell = connection2Entity.Position!.Cell;

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.South;
        travelMessage.TargetCell = connection1Entity.Position!.Cell!.Value;
        manager.Enqueue(travelMessage);

        manager.Queue.ProcessQueue(manager);

        // Traveling deletes level1; the carried dagger's room must have followed the player to
        // level2 (otherwise it would have been destroyed with level1).
        Assert.Equal(level2.EntityId, player.Position!.RoomId);
        Assert.Equal(player.Id, daggerEntity.Item!.ContainerId);
        Assert.Null(daggerEntity.Position!.Cell);
        Assert.Equal(level2.EntityId, daggerEntity.Position!.RoomId);
    }

    [Fact]
    public void Moving_prolongs_bleeding()
    {
        var level = TestHelper.BuildRoom(@"
...
...
...");
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 1));
        var manager = npc.Manager;
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyDamageOverTimeEffect(
                npc, EffectType.BleedDamage, ratePerAction: 5, duration: 1000, manager);

        var bleed = npc.ActiveEffects!.PropertyEffects.Single(
            effect => effect.EffectType == EffectType.BleedDamage);
        var expirationBefore = bleed.ExpirationTarget!.Value;

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = npc;
        travelMessage.TargetHeading = Direction.East;
        travelMessage.TargetCell = npc.Position!.Cell!.Value.Translate(Direction.East.AsVector());
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        // The step reopened the wound, pushing back when the bleeding stops.
        Assert.Equal(expirationBefore + TimeSystem.DefaultActionDelay, bleed.ExpirationTarget);
    }

    [Fact]
    public void Relative_target_is_calculated_after_forced_movement()
    {
        var level = TestHelper.BuildRoom("....");
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        var manager = npc.Manager;
        manager.Queue.ProcessQueue(manager);

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = npc;
        travelMessage.TargetHeading = Direction.East;
        travelMessage.Distance = 2;
        manager.Enqueue(travelMessage);

        npc.Position!.Cell = new Point(1, 0);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new Point(3, 0), npc.Position.Cell);
    }

    [Fact]
    public void Bound_actor_cannot_move_but_can_still_turn()
    {
        var level = TestHelper.BuildRoom(@"
...
...
...");
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 1));
        var manager = npc.Manager;
        manager.Queue.ProcessQueue(manager);

        npc.Position!.Heading = Direction.North;

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Bind, npc, amount: 200));

        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Bound.Name));

        var startCell = npc.Position!.Cell!.Value;
        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = npc;
        travelMessage.TargetHeading = Direction.East;
        travelMessage.TargetCell = startCell.Translate(Direction.East.AsVector());
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        // The bound actor turned to face the target but stayed in place.
        Assert.Equal(startCell, npc.Position!.Cell);
        Assert.Equal(Direction.East, npc.Position.Heading);
    }

    [Fact]
    public void Stuck_level_three_prevents_movement_but_allows_turning()
    {
        var level = TestHelper.BuildRoom(@"
...
...
...");
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 1));
        var manager = npc.Manager;
        manager.Queue.ProcessQueue(manager);

        npc.Position!.Heading = Direction.North;
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Stick, npc, amount: 600));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stuck.Name));

        var startCell = npc.Position.Cell!.Value;
        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = npc;
        travelMessage.TargetHeading = Direction.East;
        travelMessage.TargetCell = startCell.Translate(Direction.East.AsVector());
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(startCell, npc.Position.Cell);
        Assert.Equal(Direction.East, npc.Position.Heading);
    }

    [Theory]
    [InlineData(Direction.North)]
    [InlineData(Direction.Northeast)]
    [InlineData(Direction.East)]
    [InlineData(Direction.Southeast)]
    [InlineData(Direction.South)]
    [InlineData(Direction.Southwest)]
    [InlineData(Direction.West)]
    [InlineData(Direction.Northwest)]
    public void Engulfer_moves_its_victim_by_the_same_vector(Direction direction)
    {
        var level = TestHelper.BuildRoom(@"
.....
.....
.....
.....
.....");
        var manager = level.Entity.Manager;
        var engulferStart = new Point(2, 2);
        var victimStart = new Point(3, 2);
        var engulfer = CreatureData.GelatinousCube.Instantiate(level, engulferStart);
        var victim = CreatureData.HumanWarrior.Instantiate(level, victimStart);
        manager.Queue.ProcessQueue(manager);
        Engulf(engulfer, victim, manager);

        var vector = direction.AsVector();
        var engulferTarget = engulferStart.Translate(vector);
        var victimTarget = victimStart.Translate(vector);
        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = engulfer;
        travel.TargetHeading = direction;
        travel.TargetCell = engulferTarget;

        Assert.True(manager.TravelSystem.CanTravel(travel, manager));
        Assert.Equal(engulferStart, engulfer.Position!.Cell);
        Assert.Equal(victimStart, victim.Position!.Cell);

        manager.Enqueue(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(engulferTarget, engulfer.Position!.Cell);
        Assert.Equal(victimTarget, victim.Position!.Cell);
        Assert.Same(engulfer, level.Actors[engulferTarget]);
        Assert.Same(victim, level.Actors[victimTarget]);
    }

    [Fact]
    public void Engulfer_cannot_move_when_the_victims_paired_destination_is_blocked()
    {
        var level = TestHelper.BuildRoom("...#");
        var manager = level.Entity.Manager;
        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(1, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);
        Engulf(engulfer, victim, manager);

        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = engulfer;
        travel.TargetHeading = Direction.East;
        travel.TargetCell = new Point(2, 0);

        Assert.False(manager.TravelSystem.CanTravel(travel, manager));
        manager.Enqueue(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new Point(1, 0), engulfer.Position!.Cell);
        Assert.Equal(new Point(2, 0), victim.Position!.Cell);
    }

    [Fact]
    public void Engulfer_movement_applies_movement_side_effects_to_its_victim()
    {
        var level = TestHelper.BuildRoom("....");
        var manager = level.Entity.Manager;
        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(1, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);
        Engulf(engulfer, victim, manager);

        var initialExpiration = manager.Game.CurrentTick + 1000;
        var bleeding = new ActivePropertyEffect(0, 0, 0, EffectType.BleedDamage, null)
        {
            MagnitudePrimary = 1,
            ExpirationEvent = EffectDuration.UntilTimeout,
            ExpirationTarget = initialExpiration
        };
        bleeding = victim.ActiveEffects!.AddPropertyEffect(bleeding);

        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = engulfer;
        travel.TargetHeading = Direction.East;
        travel.TargetCell = new Point(2, 0);
        manager.Enqueue(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new Point(3, 0), victim.Position!.Cell);
        Assert.Equal(initialExpiration + TimeSystem.DefaultActionDelay, bleeding.ExpirationTarget);
    }

    [Fact]
    public void Engulfed_victim_cannot_initiate_movement()
    {
        var level = TestHelper.BuildRoom("...");
        var manager = level.Entity.Manager;
        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);
        Engulf(engulfer, victim, manager);

        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = victim;
        travel.TargetHeading = Direction.East;
        travel.TargetCell = new Point(2, 0);
        manager.Enqueue(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new Point(0, 0), engulfer.Position!.Cell);
        Assert.Equal(new Point(1, 0), victim.Position!.Cell);
    }

    [Fact]
    public void Engulfer_cannot_auto_travel_to_another_level()
    {
        var level1 = TestHelper.BuildRoom(@"
...
.>.
...");
        var manager = level1.Entity.Manager;
        var engulfer = CreatureData.GelatinousCube.Instantiate(level1, new Point(1, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level1, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        Engulf(engulfer, victim, manager);

        var level2 = TestHelper.BuildRoom(@"
...
.<.
...", game: manager.Game, depth: 1);
        var sourceConnection = level1.Connections[new Point(1, 1)].Connection!;
        var targetConnection = level2.Connections[new Point(1, 1)];
        sourceConnection.TargetRoomId = level2.EntityId;
        sourceConnection.TargetCell = targetConnection.Position!.Cell;

        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = engulfer;
        travel.TargetHeading = Direction.South;
        travel.TargetCell = new Point(1, 1);
        manager.Enqueue(travel);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(level1.EntityId, engulfer.Position!.RoomId);
        Assert.Equal(new Point(1, 0), engulfer.Position.Cell);
        Assert.Equal(level1.EntityId, victim.Position!.RoomId);
        Assert.Equal(new Point(0, 0), victim.Position.Cell);
    }

    [Fact]
    public void Engulfer_cannot_use_explicit_room_travel()
    {
        var game = TestHelper.CreateGame();
        var room1 = TestHelper.BuildRoom("...", game);
        var room2 = TestHelper.BuildRoom("...", game);
        room2.Level = room1.Level;
        var manager = game.Manager;

        using var connectionReference = manager.CreateEntity();
        var connectionEntity = connectionReference.Referenced;
        var connectionPosition = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        connectionPosition.RoomEntity = room1.Entity;
        connectionPosition.Cell = new Point(1, 0);
        connectionEntity.Position = connectionPosition;
        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomId = room2.EntityId;
        connection.TargetCell = new Point(1, 0);
        connection.Directionality = Directionality.Source;
        connectionEntity.Connection = connection;

        var engulfer = CreatureData.GelatinousCube.Instantiate(room1, new Point(1, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(room1, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);
        Engulf(engulfer, victim, manager);

        Assert.False(manager.TravelSystem.TravelToRoom(engulfer, room2.EntityId, manager));
        Assert.Equal(room1.EntityId, engulfer.Position!.RoomId);
        Assert.Equal(new Point(1, 0), engulfer.Position.Cell);
        Assert.Equal(room1.EntityId, victim.Position!.RoomId);
        Assert.Equal(new Point(2, 0), victim.Position.Cell);
    }

    [Fact]
    public void CanMoveTo_forbids_moving_outside_room_boundary()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var travelSystem = room.Game.Manager.TravelSystem;

        // Inside the room is fine.
        Assert.True(travelSystem.CanMoveTo(new Point(0, 0), room));
        Assert.True(travelSystem.CanMoveTo(new Point(2, 2), room));

        // One past the boundary on each side is forbidden (treated as a wall).
        // X past the right edge.
        Assert.False(travelSystem.CanMoveTo(new Point(3, 0), room));
        // Y past the bottom edge.
        Assert.False(travelSystem.CanMoveTo(new Point(0, 3), room));
        // Negative would underflow to 255 -- still rejected.
        Assert.False(travelSystem.CanMoveTo(new Point(255, 0), room));
        Assert.False(travelSystem.CanMoveTo(new Point(0, 255), room));
        Assert.False(travelSystem.CanMoveTo(new Point(255, 255), room));
    }

    [Fact]
    public void FindCell_visits_each_ring_in_direction_order()
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....
.....
.....");
        var visited = new List<Point>();

        var result = room.Game.Manager.TravelSystem.FindCell(
            room,
            (cell, _) =>
            {
                visited.Add(cell);
                return false;
            },
            origin: new Point(2, 2),
            maxRadius: 2,
            direction: Direction.East);

        Assert.Null(result);
        Assert.Equal(
            [
                new(2, 2),
                new(3, 2), new(3, 1), new(2, 1), new(1, 1),
                new(1, 2), new(1, 3), new(2, 3), new(3, 3),
                new(4, 2), new(4, 1), new(4, 0), new(3, 0),
                new(2, 0), new(1, 0), new(0, 0), new(0, 1),
                new(0, 2), new(0, 3), new(0, 4), new(1, 4),
                new(2, 4), new(3, 4), new(4, 4), new(4, 3)
            ],
            visited);
    }

    [Fact]
    public void FindCell_preserves_direction_order_when_cells_are_out_of_bounds()
    {
        var room = TestHelper.BuildRoom(@"
.....
.....
.....");
        var visited = new List<Point>();

        var result = room.Game.Manager.TravelSystem.FindCell(
            room,
            (cell, _) =>
            {
                visited.Add(cell);
                return false;
            },
            origin: new Point(2, 0),
            maxRadius: 2,
            direction: Direction.Northeast);

        Assert.Null(result);
        Assert.Equal(
            [
                new(2, 0),
                new(1, 0), new(1, 1), new(2, 1), new(3, 1), new(3, 0),
                new(0, 0), new(0, 1), new(0, 2), new(1, 2), new(2, 2),
                new(3, 2), new(4, 2), new(4, 1), new(4, 0)
            ],
            visited);
    }

    [Fact]
    public void Player_blocked_by_the_room_boundary_does_not_crash_revealing_terrain()
    {
        // Regression: a player attempting to move off the edge of the room produces an
        // unsuccessful TraveledMessage whose TargetCell lies outside the room. KnowledgeSystem
        // used to index room.PointToIndex with that out-of-bounds cell while trying to reveal
        // the "bumped" tile, throwing IndexOutOfRangeException. Standing on an edge connection
        // is the common way to end up flush against the boundary, but any edge cell triggers it.
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var manager = room.Game.Manager;

        // Player at the east-edge column (x = Width - 1).
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 1));
        manager.Queue.ProcessQueue(manager);

        // Move east, off the room boundary -> blocked move with an out-of-bounds target.
        var offEdge = TravelMessage.Create(manager);
        offEdge.ActorEntity = player;
        offEdge.TargetHeading = Direction.East;
        offEdge.TargetCell = new Point(3, 1);
        offEdge.MoveOffConflicting = true;
        manager.Enqueue(offEdge);

        // Must not throw.
        manager.Queue.ProcessQueue(manager);

        // The player stayed put.
        Assert.Equal(new Point(2, 1), player.Position!.Cell);
    }

    [Fact]
    public void A_dead_non_player_being_is_fully_deleted_and_drops_its_loot()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var manager = room.Game.Manager;

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 1));
        ItemData.GoldCoin.Instantiate(room, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        // Put the gold coin into the NPC's inventory so we can verify loot drops (not deleted).
        var coinEntity = room.Items[new Point(0, 0)];
        var moveIn = MoveItemMessage.Create(manager);
        moveIn.ItemEntity = coinEntity;
        moveIn.TargetContainerEntity = npc;
        manager.Enqueue(moveIn);
        manager.Queue.ProcessQueue(manager);

        var coinId = npc.Being!.Items.Single(
            item => item.Item!.TemplateName == ItemData.GoldCoin.Name).Id;
        var npcId = npc.Id;
        var owned = new List<int>();
        owned.AddRange(npc.ActiveEffects!.Abilities.Select(e => e.Id));
        if (npc.Being.PrimaryNaturalWeaponId is { } pw)
            owned.Add(pw);
        if (npc.Being!.SecondaryNaturalWeaponId is { } sw)
            owned.Add(sw);

        // Kill the NPC.
        npc.Being!.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);

        // The dead being and everything it owned (abilities, applied effects, natural weapons) are
        // permanently deleted — not left as an inert husk.
        Assert.Null(manager.FindEntity(npcId));
        Assert.False(room.Actors.ContainsKey(new Point(1, 1)));
        var lingering = owned.Where(id => manager.FindEntity(id) != null).ToList();
        Assert.True(lingering.Count == 0, $"Lingering owned entities: {string.Join(",", lingering)}");

        // Its carried loot is dropped to the floor (where it died), not deleted.
        Assert.NotNull(manager.FindEntity(coinId));
        Assert.Contains(room.Items.Values, e => e.Id == coinId);
    }

    [Fact]
    public void Deleting_an_actor_entity_removes_its_inventory_abilities_and_effects()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var manager = room.Game.Manager;

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        var npcId = npc.Id;
        var ownedIds = new List<int>();
        ownedIds.AddRange(npc.Being!.Items.Select(e => e.Id));
        ownedIds.AddRange(npc.ActiveEffects!.Abilities.Select(e => e.Id));
        if (npc.Being!.PrimaryNaturalWeaponId is { } pw)
            ownedIds.Add(pw);
        if (npc.Being!.SecondaryNaturalWeaponId is { } sw)
            ownedIds.Add(sw);

        DeleteEntityMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);

        // The actor entity is gone: removed from the room and from the manager (which, for a
        // tracked game, deletes its row). A deleted entity is no longer in any group or temporal
        // index, so it is never processed again.
        Assert.Null(manager.FindEntity(npcId));
        Assert.False(room.Actors.ContainsKey(new Point(1, 1)));

        // Everything the actor owned — carried items, natural weapons, abilities and the effects
        // applied to it — is cascade-deleted with it.
        var lingering = ownedIds.Where(id => manager.FindEntity(id) != null).ToList();
        Assert.True(lingering.Count == 0, $"Lingering owned entities: {string.Join(",", lingering)}");
    }

    [Fact]
    public void Deleting_an_actor_entity_removes_its_race_carrier()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var manager = room.Game.Manager;

        var npc = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        // A creature carries its race on a separate carrier entity (being.Races), distinct from its
        // main being.
        var raceCarrierId = npc.Being!.Races.Single().Id;
        Assert.NotEqual(npc.Id, raceCarrierId);

        DeleteEntityMessage.Enqueue(npc, manager);
        manager.Queue.ProcessQueue(manager);

        // The carrier is cascade-deleted with its owning being, leaving nothing behind.
        Assert.Null(manager.FindEntity(npc.Id));
        Assert.Null(manager.FindEntity(raceCarrierId));
    }

    [Fact]
    public void Intra_level_travel_deletes_the_abandoned_rooms_actors_and_items()
    {
        var game = TestHelper.CreateGame();
        var roomA = TestHelper.BuildRoom(@"
...
...
...", game);
        var roomB = TestHelper.BuildRoom(@"
...
...
...", game);
        var manager = game.Manager;

        // Both rooms on the same GameLevel -> intra-level move.
        roomB.Level = roomA.Level;

        // One-way intra-level connection roomA(1,2) -> roomB(1,0).
        using var connectionReference = manager.CreateEntity();
        var connectionEntity = connectionReference.Referenced;
        var connPosition = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        connPosition.RoomEntity = roomA.Entity;
        connPosition.Cell = new Point(1, 2);
        connectionEntity.Position = connPosition;
        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomId = roomB.EntityId;
        connection.TargetCell = new Point(1, 0);
        connection.Directionality = Directionality.Source;
        connection.Orientation = Direction.South;
        connectionEntity.Connection = connection;

        var npc = CreatureData.HumanWarrior.Instantiate(roomA, new Point(0, 0));
        ItemData.GoldCoin.Instantiate(roomA, new Point(2, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, roomA, new Point(1, 2));
        manager.Queue.ProcessQueue(manager);

        var npcId = npc.Id;
        var itemId = roomA.Items.Values.Single().Id;

        Assert.True(manager.TravelSystem.TravelToRoom(player, roomB.EntityId, manager));
        manager.Queue.ProcessQueue(manager);

        // Player crossed into roomB.
        Assert.Equal(roomB.EntityId, player.Position!.RoomId);

        // roomA's actor and item are gone; the room itself survives (it's part of the level graph).
        Assert.Null(manager.FindEntity(npcId));
        Assert.Null(manager.FindEntity(itemId));
        Assert.Empty(roomA.Actors);
        Assert.Empty(roomA.Items);
        Assert.Contains(roomA, roomA.Level.Rooms);
    }

    [Fact]
    public void Inter_level_travel_deletes_the_whole_abandoned_level()
    {
        var game = TestHelper.CreateGame();

        // Level 1: an exit room with down-stairs, plus a side room.
        var exitRoom = TestHelper.BuildRoom(@"
...
.>.
...", game);
        var sideRoom = TestHelper.BuildRoom(@"
...
...
...", game);
        var manager = game.Manager;
        sideRoom.Level = exitRoom.Level;

        // Level 2: the destination.
        var level2Room = TestHelper.BuildRoom(@"
...
.<.
...", game: game, depth: 1);

        var down = exitRoom.Connections.GetValueOrDefault(new Point(1, 1))!;
        var up = level2Room.Connections.GetValueOrDefault(new Point(1, 1))!;
        down.Connection!.TargetRoomId = level2Room.EntityId;
        down.Connection!.TargetCell = up.Position!.Cell;
        down.Connection!.Orientation = Direction.Down;

        // Each BuildRoom makes its own GameLevel, so move the side room into level 1 explicitly:
        // DeleteLevelContents iterates GameLevel.Rooms, which only setting RoomComponent.Level
        // (above) does not update.
        var level1 = exitRoom.Level;
        level1.Rooms.Add(sideRoom);

        // level2Room is built in its own branch; move its level into level 1's branch so the
        // transition is an inter-level (same-branch) move rather than an inter-branch one.
        level2Room.Level.Branch = level1.Branch;
        level2Room.Level.BranchName = level1.BranchName;

        // A creature stranded in the side room of level 1.
        var strandedNpc = CreatureData.HumanWarrior.Instantiate(sideRoom, new Point(0, 0));

        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, exitRoom, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var strandedNpcId = strandedNpc.Id;
        var sideRoomId = sideRoom.EntityId;
        var exitRoomId = exitRoom.EntityId;

        // Step south onto the down-stairs -> inter-level transition to level 2.
        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.South;
        travelMessage.TargetCell = new Point(1, 1);
        travelMessage.MoveOffConflicting = true;
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(level2Room.EntityId, player.Position!.RoomId);

        // The side room and its creature are gone.
        Assert.Null(manager.FindEntity(strandedNpcId));
        Assert.Null(manager.FindEntity(sideRoomId));
        Assert.DoesNotContain(sideRoom, level1.Rooms);

        // The exit room (the room with the inter-level connection the player used) is deleted too.
        Assert.Null(manager.FindEntity(exitRoomId));
        Assert.DoesNotContain(exitRoom, level1.Rooms);
        Assert.Empty(level1.Rooms);
    }

    [Fact]
    public void Inter_branch_travel_deletes_every_level_in_the_previous_branch()
    {
        var game = TestHelper.CreateGame();

        // Branch A, depth 0: the room the player is in, with a branch-exit down-stair.
        var exitRoom = TestHelper.BuildRoom(@"
...
.>.
...", game);
        var manager = game.Manager;
        var branchA = exitRoom.Level.Branch;

        // Branch A, depth 1: a deeper, already-visited level. TestHelper.BuildRoom makes its own
        // branch, so move its level into branch A.
        var deeperRoom = TestHelper.BuildRoom(@"
...
...
...", game, depth: 1);
        deeperRoom.Level.Branch = branchA;
        deeperRoom.Level.BranchName = branchA.Name;
        branchA.Levels.Add(deeperRoom.Level);

        // Branch B: the destination, in its own (different) branch.
        var branchBRoom = TestHelper.BuildRoom(@"
...
.<.
...", game, depth: 1);

        var down = exitRoom.Connections.GetValueOrDefault(new Point(1, 1))!;
        var up = branchBRoom.Connections.GetValueOrDefault(new Point(1, 1))!;
        down.Connection!.TargetRoomId = branchBRoom.EntityId;
        down.Connection!.TargetCell = up.Position!.Cell;
        down.Connection!.Orientation = Direction.Down;

        var exitNpc = CreatureData.HumanWarrior.Instantiate(exitRoom, new Point(0, 0));
        var deeperNpc = CreatureData.HumanWarrior.Instantiate(deeperRoom, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, exitRoom, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var exitRoomId = exitRoom.EntityId;
        var deeperRoomId = deeperRoom.EntityId;
        var exitNpcId = exitNpc.Id;
        var deeperNpcId = deeperNpc.Id;
        var level0 = exitRoom.Level;
        var level1 = deeperRoom.Level;

        // Step south onto the down-stair -> inter-branch transition to branch B.
        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.South;
        travelMessage.TargetCell = new Point(1, 1);
        travelMessage.MoveOffConflicting = true;
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(branchBRoom.EntityId, player.Position!.RoomId);

        // Every level in branch A is deleted — even the exit room, whose inter-level stairs would
        // have been kept on a same-branch transition.
        Assert.Null(manager.FindEntity(exitRoomId));
        Assert.Null(manager.FindEntity(deeperRoomId));
        Assert.Null(manager.FindEntity(exitNpcId));
        Assert.Null(manager.FindEntity(deeperNpcId));
        Assert.Empty(level0.Rooms);
        Assert.Empty(level1.Rooms);

        // The destination branch is untouched.
        Assert.NotNull(manager.FindEntity(branchBRoom.EntityId));
    }
}
