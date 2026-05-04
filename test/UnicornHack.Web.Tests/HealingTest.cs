using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class HealingTest
{
    private const string TestMap = @"
.......
.......
.......
.......
.......
.......
.......";

    [Fact]
    public void UseFlaskOfHealing()
    {
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);

        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        // Place NPC adjacent to damage the player
        CreatureData.Human.Instantiate(level, new Point(3, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

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
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);

        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        // Use flask at full HP
        var flaskAbility = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, "flask of healing: Drink")]!;

        WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.SetAbilitySlot, flaskAbility.Id, 5);

        WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.UseAbilitySlot, 5);
    }
}
