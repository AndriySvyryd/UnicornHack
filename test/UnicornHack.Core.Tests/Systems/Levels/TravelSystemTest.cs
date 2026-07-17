using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;

namespace UnicornHack.Systems.Levels;

public class TravelSystemTest
{
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

        Assert.Equal(new[] { Direction.North, Direction.Southeast },
            manager.TravelSystem.GetPossibleMovementDirections(undine.Position!, safe: true));
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

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.South;
        travelMessage.TargetCell = connection1Entity.Position!.Cell;
        manager.Enqueue(travelMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(level2.EntityId, player.Position!.RoomId);
        // level1 is a different branch from level2, so travelling there deletes level1 entirely
        // (including connection1); compare against the cell captured before the move.
        Assert.Equal(targetCell, player.Position.Cell);
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

        var npc = CreatureData.Human.Instantiate(room, new Point(1, 1));
        ItemData.GoldCoin.Instantiate(room, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        // Put the gold coin into the NPC's inventory so we can verify loot drops (not deleted).
        var coinEntity = room.Items[new Point(0, 0)];
        var moveIn = MoveItemMessage.Create(manager);
        moveIn.ItemEntity = coinEntity;
        moveIn.TargetContainerEntity = npc;
        manager.Enqueue(moveIn);
        manager.Queue.ProcessQueue(manager);

        var coinId = coinEntity.Id;
        var npcId = npc.Id;
        var owned = new List<int>();
        owned.AddRange(npc.Being!.Abilities.Select(e => e.Id));
        owned.AddRange(npc.Being!.AppliedEffects.Select(e => e.Id));
        if (npc.Being!.PrimaryNaturalWeaponId is { } pw) owned.Add(pw);
        if (npc.Being!.SecondaryNaturalWeaponId is { } sw) owned.Add(sw);

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

        var npc = CreatureData.Human.Instantiate(room, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        var npcId = npc.Id;
        var ownedIds = new List<int>();
        ownedIds.AddRange(npc.Being!.Items.Select(e => e.Id));
        ownedIds.AddRange(npc.Being!.Abilities.Select(e => e.Id));
        ownedIds.AddRange(npc.Being!.AppliedEffects.Select(e => e.Id));
        if (npc.Being!.PrimaryNaturalWeaponId is { } pw) ownedIds.Add(pw);
        if (npc.Being!.SecondaryNaturalWeaponId is { } sw) ownedIds.Add(sw);

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
    public void Deleting_an_actor_entity_keeps_an_effect_it_applied_to_a_different_being()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var manager = room.Game.Manager;

        var attacker = CreatureData.Human.Instantiate(room, new Point(1, 1));
        var victim = CreatureData.Human.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        // An ability owned by the attacker, and an infinite effect it applied to the victim.
        GameEntity sourceAbility;
        using (var abilityReference = manager.CreateEntity())
        {
            sourceAbility = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = "test attack";
            ability.OwnerEntity = attacker;
            sourceAbility.Ability = ability;
        }

        int victimEffectId;
        using (var effectReference = manager.CreateEntity())
        {
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.Duration = EffectDuration.Infinite;
            effect.EffectType = EffectType.Bleed;
            effect.AppliedAmount = 1;
            effect.AffectedEntityId = victim.Id;
            effect.SourceAbilityId = sourceAbility.Id;
            effectEntity.Effect = effect;
            victimEffectId = effectEntity.Id;
        }

        manager.Queue.ProcessQueue(manager);

        DeleteEntityMessage.Enqueue(attacker, manager);
        manager.Queue.ProcessQueue(manager);

        // The attacker is gone, but the effect it applied to the (still-living) victim is not
        // deleted — it merely loses its source link.
        Assert.Null(manager.FindEntity(attacker.Id));
        var survivingEffect = manager.FindEntity(victimEffectId);
        Assert.NotNull(survivingEffect);
        Assert.Equal(victim.Id, survivingEffect!.Effect!.AffectedEntityId);
        Assert.Null(survivingEffect.Effect!.SourceAbilityId);
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

        var npc = CreatureData.Human.Instantiate(roomA, new Point(0, 0));
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
        var strandedNpc = CreatureData.Human.Instantiate(sideRoom, new Point(0, 0));

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

        var exitNpc = CreatureData.Human.Instantiate(exitRoom, new Point(0, 0));
        var deeperNpc = CreatureData.Human.Instantiate(deeperRoom, new Point(0, 0));
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
