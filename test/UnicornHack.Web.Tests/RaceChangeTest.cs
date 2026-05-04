using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class RaceChangeTest
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

        ItemData.PotionOfOgreness.Instantiate(playerEntity);
        ItemData.PotionOfElfness.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        return (playerEntity.Player!, listener, game.Services, manager);
    }

    [Fact]
    public void DrinkPotionOfOgreness()
    {
        var (player, listener, services, manager) = SetupGame();

        var potionAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "potion of ogreness: Drink")]!;

        // Slot the potion drink ability
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, potionAbility.Id, 5);

        // Use the slotted ability
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5);

        // Verify race changed — should now have an ogre race
        Assert.Contains(fresh.Races!.Values, r => r.Name?.Contains("ogre", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void DrinkPotionOfElfness()
    {
        var (player, listener, services, manager) = SetupGame();

        var potionAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "potion of elfness: Drink")]!;

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, potionAbility.Id, 5);

        var fresh2 = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5);

        Assert.Contains(fresh2.Races!.Values, r => r.Name?.Contains("elf", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void MultipleRaceChanges()
    {
        var (player, listener, services, manager) = SetupGame();

        // Drink ogre potion
        var ogrePotionAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "potion of ogreness: Drink")]!;

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, ogrePotionAbility.Id, 5);
        var afterOgre = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5);

        Assert.Contains(afterOgre.Races!.Values, r => r.Name?.Contains("ogre", StringComparison.OrdinalIgnoreCase) == true);

        // Drink elf potion
        var elfPotionAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "potion of elfness: Drink")]!;

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, elfPotionAbility.Id, 5);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5);

        Assert.Contains(fresh.Races!.Values, r => r.Name?.Contains("elf", StringComparison.OrdinalIgnoreCase) == true);
    }
}
