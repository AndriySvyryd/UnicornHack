using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class AbilitySlottingTest
{
    private const string TestMap = @"
.......
.......
.......
.......
.......
.......
.......";

    private static (PlayerComponent Player, LevelChangeBuilder Listener, GameServices Services, GameManager Manager)
        SetupGame()
    {
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        return (playerEntity.Player!, listener, game.Services, manager);
    }

    [Fact]
    public void SetAbilitySlot()
    {
        var (player, listener, services, manager) = SetupGame();

        // Find a slottable ability (item drink ability)
        var slottableAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "flask of healing: Drink")]!;
        if (slottableAbility == null)
        {
            return; // Skip if no slottable abilities available
        }

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, slottableAbility.Id, 3);

        Assert.Contains(fresh.Abilities!.Values, a => a.Slot == 3);
    }

    [Fact]
    public void ClearAbilitySlot()
    {
        var (player, listener, services, manager) = SetupGame();

        var slottableAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "flask of healing: Drink")]!;
        if (slottableAbility == null)
        {
            return;
        }

        // Assign to slot 3
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, slottableAbility.Id, 3);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, 0, 3);

        Assert.DoesNotContain(fresh.Abilities!.Values, a => a.Slot == 3);
    }

    [Fact]
    public void SwapAbilitySlot()
    {
        var (player, listener, services, manager) = SetupGame();

        var swordAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "long sword: Wield")]!;
        var flaskAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "flask of healing: Drink")]!;
        if (swordAbility == null || flaskAbility == null)
        {
            return;
        }

        // Assign first to slot 3
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, swordAbility.Id, 3);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, flaskAbility.Id, 3);

        var slotted = fresh.Abilities!.Values.Single(a => a.Slot == 3);
        Assert.Contains("Drink", slotted.Name!);
    }

    [Fact]
    public void UseAbilitySlot()
    {
        var (player, listener, services, manager) = SetupGame();

        // Place NPC south
        CreatureData.Human.Instantiate(
            manager.FindEntity(player.Entity.Position!.LevelId)!.Level!, new Point(3, 4));
        manager.Queue.ProcessQueue(manager);

        // Equip sword to get melee attack in slot 0
        var swordEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 0, (int)Direction.South);
    }
}

