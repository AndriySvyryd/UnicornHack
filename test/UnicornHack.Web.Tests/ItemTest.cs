using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class ItemTest
{
    private const string TestMap = @"
.....
.....
.....
.....
.....";

    private static (PlayerComponent Player, RoomChangeBuilder Listener, GameServices Services, GameManager Manager)
        SetupGame()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.Dagger.Instantiate(playerEntity);
        ItemData.MailArmor.Instantiate(playerEntity);
        ItemData.Shortbow.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        return (playerEntity.Player!, listener, game.Services, manager);
    }

    private static int GetEquipAbilitySlot(GameEntity playerEntity, string itemTemplateName)
    {
        var ability = playerEntity.Being!.SlottedAbilities.Values
            .Select(e => e.Ability!)
            .Single(a => a.Type == AbilityType.Item
                         && a.Name == itemTemplateName + ": Equip");
        return ability.Slot!.Value;
    }

    [Fact]
    public void EquipMeleeWeapon()
    {
        var (player, listener, services, manager) = SetupGame();
        var slot = GetEquipAbilitySlot(player.Entity, "long sword");

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, slot);

        // Default melee attack ability should appear in slot 0
        Assert.Contains(fresh.Abilities!.Values, a => a.Slot == 0);
    }

    [Fact]
    public void EquipRangedWeapon()
    {
        var (player, listener, services, manager) = SetupGame();
        var slot = GetEquipAbilitySlot(player.Entity, "shortbow");

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, slot);

        // Default ranged attack ability should appear in slot 1
        Assert.Contains(fresh.Abilities!.Values, a => a.Slot == 1);
    }

    [Fact]
    public void EquipArmor()
    {
        var (player, listener, services, manager) = SetupGame();
        var slot = GetEquipAbilitySlot(player.Entity, "mail armor");

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, slot);

        // Armor shouldn't add attack abilities
        Assert.DoesNotContain(fresh.Abilities!.Values, a => a.Name?.Contains("mail armor") == true);
    }

    [Fact]
    public void UnequipItem()
    {
        var (player, listener, services, manager) = SetupGame();
        var slot = GetEquipAbilitySlot(player.Entity, "mail armor");

        // First equip the armor
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, slot);

        var armorEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "mail armor");
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UnequipItem, armorEntity.Id);

        Assert.DoesNotContain(fresh.Abilities!.Values, a => a.Name?.Contains("mail armor") == true);
    }

    [Fact]
    public void EquipReplaceExisting()
    {
        var (player, listener, services, manager) = SetupGame();
        var swordSlot = GetEquipAbilitySlot(player.Entity, "long sword");

        // Equip sword first
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, swordSlot);

        var daggerSlot = GetEquipAbilitySlot(player.Entity, "dagger");
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, daggerSlot);

        // Slot 0 should now have the dagger attack ability
        Assert.Contains(fresh.Abilities!.Values, a => a.Slot == 0);
    }

    [Fact]
    public void DropItem()
    {
        var (player, listener, services, manager) = SetupGame();
        var swordEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.DropItem, swordEntity.Id);

        // Dropped item should appear on the level and leave the player's inventory.
        Assert.Contains(fresh.Room!.Items!.Values, i => i.BaseName == "long sword");
        Assert.DoesNotContain(player.Entity.Being!.Items, i => i.Item!.TemplateName == "long sword");
    }

    [Fact]
    public void DropEquippedItem()
    {
        var (player, listener, services, manager) = SetupGame();
        var swordEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        // Equip first
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.DropItem, swordEntity.Id);

        // Dropping an equipped item unequips it, drops it on the level, and removes it from inventory.
        Assert.Contains(fresh.Room!.Items!.Values, i => i.BaseName == "long sword");
        Assert.DoesNotContain(player.Entity.Being!.Items, i => i.Item!.TemplateName == "long sword");
        Assert.Equal(EquipmentSlot.None, swordEntity.Item!.EquippedSlot);
    }

    [Fact]
    public void DropMultipleItems()
    {
        var (player, listener, services, manager) = SetupGame();
        var swordEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");
        var daggerEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "dagger");

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.DropItem, swordEntity.Id);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.DropItem, daggerEntity.Id);

        // Both dropped items should be on the level and gone from the player's inventory.
        Assert.Contains(fresh.Room!.Items!.Values, i => i.BaseName == "long sword");
        Assert.Contains(fresh.Room!.Items!.Values, i => i.BaseName == "dagger");
        Assert.DoesNotContain(player.Entity.Being!.Items,
            i => i.Item!.TemplateName is "long sword" or "dagger");
    }

    [Fact]
    public void UseFlaskOfHealing()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);

        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        // Place NPC adjacent to damage the player
        CreatureData.HumanWarrior.Instantiate(level, new Point(3, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        var beforeDamageHp = playerEntity.Being!.HitPoints;

        // Wait for NPC to attack and damage the player
        var afterDamage = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services, ActorActionType.Wait);
        var damagedHp = afterDamage.Hp;

        Assert.True(damagedHp < beforeDamageHp, "Player should have taken damage from NPC");

        // Use flask of healing — find the drink ability
        var flaskAbility = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, "flask of healing: Drink")]!;

        WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.SetAbilitySlot, flaskAbility.Id, 5);
        var afterHealing = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services, ActorActionType.UseAbilitySlot, 5);
        var healedHp = afterHealing.Hp;

        Assert.True(healedHp > damagedHp, "Player HP should increase after using the flask of healing");
    }

    [Fact]
    public void HealAtFullHP()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);

        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        // Use flask at full HP
        var flaskAbility = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, "flask of healing: Drink")]!;

        WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.SetAbilitySlot, flaskAbility.Id, 5);

        WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.UseAbilitySlot, 5);
    }

    /// <summary>
    ///     Regression: equipping mail armor via ability then moving causes change tracking
    ///     inconsistencies (WallNeighbors, actor deletions, item modifications sent for
    ///     entities the client never received).
    /// </summary>
    [Fact]
    public void EquipArmorThenMoveAwayFromCreature()
    {
        var map = @"
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
############.############
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................";
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(map, game);
        var manager = game.Manager;

        // Place a creature just south of the wall gap
        CreatureData.HumanWarrior.Instantiate(level, new Point(12, 14));
        manager.Queue.ProcessQueue(manager);

        // Player starts north of the wall, near the gap
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(12, 11));
        manager.Queue.ProcessQueue(manager);

        ItemData.MailArmor.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        var player = playerEntity.Player!;

        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.South);

        // Step 1: Equip mail armor via its ability
        var armorSlot = GetEquipAbilitySlot(playerEntity, "mail armor");
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.UseAbilitySlot, armorSlot);

        // Step 2: Move north — creature should leave visibility
        for (var i = 0; i < 5; i++)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.MoveOneCell, (int)Direction.North);
        }
    }

    /// <summary>
    ///     Regression: equipping armor on a turn where many actors/items transition
    ///     in and out of the player's 180° vision cone (driven by heading changes from
    ///     movement) emits change-tracking deltas referring to entities the client
    ///     was never told about. Reproduces "Actor X not deleted" / "Item X not found".
    /// </summary>
    [Fact]
    public void EquipArmorWithSurroundingActorsAndItems()
    {
        var map = @"
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................";
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(map, game);
        var manager = game.Manager;

        // Surround the eventual player position (12,12) with creatures and items
        // arranged so they overlap on cells (an actor and a loose item sharing a cell).
        // This is what triggers the cross-relationship cleanup bug when a knowledge
        // entity at one of those cells is removed.
        var creaturePositions = new[]
        {
            new Point(12, 8),  // far north
            new Point(12, 16), // far south
            new Point(8, 12),  // far west
            new Point(16, 12), // far east
            new Point(10, 10), // NW
            new Point(14, 10), // NE
            new Point(10, 14), // SW
            new Point(14, 14), // SE
        };
        foreach (var pos in creaturePositions)
        {
            CreatureData.HumanWarrior.Instantiate(level, pos);
        }

        manager.Queue.ProcessQueue(manager);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(12, 12));
        manager.Queue.ProcessQueue(manager);

        // Place loose items on the SAME cells as some creatures so a knowledge entity
        // at the cell can be either an actor-knowledge or an item-knowledge — exposing
        // the secondary-key collision in LookupEntityRelationship.TryRemoveEntity.
        var itemPositions = new[]
        {
            new Point(12, 8),
            new Point(12, 16),
            new Point(10, 10),
            new Point(14, 10),
            new Point(10, 14),
            new Point(14, 14),
            new Point(11, 11),
            new Point(13, 13)
        };
        foreach (var pos in itemPositions)
        {
            ItemData.Dagger.Instantiate(level, pos);
        }

        // Inventory item to equip
        ItemData.MailArmor.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        var player = playerEntity.Player!;

        // Walk a route that flips heading to swap which creatures/items enter perception,
        // and equip the armor mid-walk. The 8 free neighbors at (12,12) let us
        // step in any cardinal/diagonal direction.
        var moves = new[]
        {
            Direction.Northwest,
            Direction.Northeast,
            Direction.Southeast,
            Direction.Southwest,
            Direction.Northwest
        };
        var armorSlot = GetEquipAbilitySlot(playerEntity, "mail armor");
        for (var i = 0; i < moves.Length; i++)
        {
            if (i == 2)
            {
                WebTestHelper.PerformAndVerify(player, listener, game.Services,
                    ActorActionType.UseAbilitySlot, armorSlot);
            }

            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.MoveOneCell, (int)moves[i]);
        }
    }

    /// <summary>
    ///     Minimal repro: a single creature standing where it leaves perception
    ///     when the player turns east. Equipping armor + heading change causes
    ///     a Deleted sentinel to be emitted for an actor the client never received.
    /// </summary>
    [Fact]
    public void EquipArmorWhileTurning()
    {
        var map = @"
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................
.........................";
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(map, game);
        var manager = game.Manager;

        // Creature directly north of the player. Player faces South initially,
        // so this creature is OUTSIDE perception. After turning to East, still outside.
        // After turning to North, INSIDE perception.
        CreatureData.HumanWarrior.Instantiate(level, new Point(12, 8));
        manager.Queue.ProcessQueue(manager);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(12, 12));
        manager.Queue.ProcessQueue(manager);

        ItemData.MailArmor.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        var player = playerEntity.Player!;

        // First, let the player perceive the creature (turn north)
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);

        var armorSlot = GetEquipAbilitySlot(playerEntity, "mail armor");
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.UseAbilitySlot, armorSlot);

        // Move south and east to lose perception of the creature
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.East);
    }
}
