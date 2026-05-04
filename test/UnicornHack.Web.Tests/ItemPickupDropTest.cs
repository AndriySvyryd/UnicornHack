using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class ItemPickupDropTest
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
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        return (playerEntity.Player!, listener, game.Services, manager);
    }

    [Fact]
    public void DropItem()
    {
        var (player, listener, services, manager) = SetupGame();
        var swordEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.DropItem, swordEntity.Id);

        // Dropped item should appear on the level
        Assert.Contains(fresh.Level!.Items!.Values, i => i.BaseName == "long sword");
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

        // Dropped item should appear on the level
        Assert.Contains(fresh.Level!.Items!.Values, i => i.BaseName == "long sword");
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

        // Verify both items ended up on the level
        Assert.True(fresh.Level!.Items!.Count >= 2);
    }
}
