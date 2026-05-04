using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class ItemEquipmentTest
{
    private const string TestMap = @"
.....
.....
.....
.....
.....";

    private static (PlayerComponent Player, LevelChangeBuilder Listener, GameServices Services, GameManager Manager)
        SetupGame()
    {
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.Dagger.Instantiate(playerEntity);
        ItemData.MailArmor.Instantiate(playerEntity);
        ItemData.Shortbow.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

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
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        // Place a creature just south of the wall gap
        CreatureData.Human.Instantiate(level, new Point(12, 14));
        manager.Queue.ProcessQueue(manager);

        // Player starts north of the wall, near the gap
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(12, 11));
        manager.Queue.ProcessQueue(manager);

        ItemData.MailArmor.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

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
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
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
            CreatureData.Human.Instantiate(level, pos);
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
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

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
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        // Creature directly north of the player. Player faces South initially,
        // so this creature is OUTSIDE perception. After turning to East, still outside.
        // After turning to North, INSIDE perception.
        CreatureData.Human.Instantiate(level, new Point(12, 8));
        manager.Queue.ProcessQueue(manager);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(12, 12));
        manager.Queue.ProcessQueue(manager);

        ItemData.MailArmor.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

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
