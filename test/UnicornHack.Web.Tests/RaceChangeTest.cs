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

    private static (PlayerComponent Player, RoomChangeBuilder Listener, GameServices Services, GameManager Manager)
        SetupGame()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ItemData.PotionOfOgreness.Instantiate(playerEntity);
        ItemData.PotionOfElfness.Instantiate(playerEntity);
        ItemData.PotionOfInhumanity.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        return (playerEntity.Player!, listener, game.Services, manager);
    }

    [Fact]
    public void DrinkPotionOfOgreness()
    {
        var (player, listener, services, manager) = SetupGame();

        var potionAbility = GetPotionAbility(player.Entity, ItemData.PotionOfOgreness.Name);

        // Slot the potion drink ability
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, potionAbility.Id, 5);

        // Use the slotted ability
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5, new Vector(0, 0).ToInt32());

        // Verify the new race is delivered through generic ability transport.
        Assert.Contains(
            fresh.Abilities!.Values,
            ability => ability.Type == (int)AbilityType.Race
                       && ability.Name == "ogre"
                       && ability.Level == 1);
    }

    [Fact]
    public void Repeated_race_change_updates_authoritative_level_without_an_over_max_carrier()
    {
        var (player, listener, services, manager) = SetupGame();

        var firstPotionAbility = GetPotionAbility(player.Entity, ItemData.PotionOfOgreness.Name);
        WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.SetAbilitySlot, firstPotionAbility.Id, 5);
        WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.UseAbilitySlot, 5,
            new Vector(0, 0).ToInt32());

        ItemData.PotionOfOgreness.Instantiate(player.Entity);
        manager.Queue.ProcessQueue(manager);
        listener.RegisterPlayerBuilder(player.Entity, manager);
        var secondPotionAbility = GetPotionAbility(player.Entity, ItemData.PotionOfOgreness.Name);
        WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.SetAbilitySlot, secondPotionAbility.Id, 5);
        var fresh = WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.UseAbilitySlot, 5,
            new Vector(0, 0).ToInt32());

        var ogreAbilities = fresh.Abilities!.Values.Where(
            ability => ability.Type == (int)AbilityType.Race
                       && ability.Name == "ogre");
        var ogreAbility = Assert.Single(ogreAbilities);
        Assert.Equal(2, ogreAbility.Level);

        Assert.Null(manager.AffectableAbilitiesIndex[(player.Entity.Id, "ogre race#2")]);
    }

    [Fact]
    public void DrinkPotionOfElfness()
    {
        var (player, listener, services, manager) = SetupGame();

        var potionAbility = GetPotionAbility(player.Entity, ItemData.PotionOfElfness.Name);

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, potionAbility.Id, 5);

        var fresh2 = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5, new Vector(0, 0).ToInt32());

        Assert.Contains(
            fresh2.Abilities!.Values,
            ability => ability.Type == (int)AbilityType.Race
                       && ability.Name == "elf");
    }

    [Fact]
    public void DrinkPotionOfInhumanity_removes_the_human_race_ability()
    {
        var (player, listener, services, manager) = SetupGame();
        var potionAbility = GetPotionAbility(player.Entity, ItemData.PotionOfInhumanity.Name);

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, potionAbility.Id, 5);
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5, new Vector(0, 0).ToInt32());

        Assert.DoesNotContain(
            fresh.Abilities!.Values,
            ability => ability.Type == (int)AbilityType.Race
                       && ability.Name == "human");
    }

    [Fact]
    public void MultipleRaceChanges()
    {
        var (player, listener, services, manager) = SetupGame();

        // Drink ogre potion
        var ogrePotionAbility = GetPotionAbility(player.Entity, ItemData.PotionOfOgreness.Name);

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, ogrePotionAbility.Id, 5);
        var afterOgre = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5, new Vector(0, 0).ToInt32());

        Assert.Contains(
            afterOgre.Abilities!.Values,
            ability => ability.Type == (int)AbilityType.Race
                       && ability.Name == "ogre");

        // Drink elf potion
        var elfPotionAbility = GetPotionAbility(player.Entity, ItemData.PotionOfElfness.Name);

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, elfPotionAbility.Id, 5);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 5, new Vector(0, 0).ToInt32());

        Assert.Contains(
            fresh.Abilities!.Values,
            ability => ability.Type == (int)AbilityType.Race
                       && ability.Name == "elf");
    }

    private static GameEntity GetPotionAbility(GameEntity playerEntity, string itemTemplateName)
        => playerEntity.Being!.Items.Single(item => item.Item!.TemplateName == itemTemplateName)
            .ActiveEffects!.Abilities.Single(ability => ability.Ability!.Name == "Drink");
}
