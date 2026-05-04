using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class MultiTurnSequenceTest
{
    [Fact]
    public void FullPlaythrough()
    {
        var map = @"
..........
..........
..........
..........
..........
..........
..........
..........
..........
..........";
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(map, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(5, 5));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        ItemData.PotionOfOgreness.Instantiate(playerEntity);
        ItemData.Dagger.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        CreatureData.Human.Instantiate(level, new Point(5, 3));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);
        var player = playerEntity.Player!;

        // Step 1: Equip sword
        var swordEntity = playerEntity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        // Step 2: Move south
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.South);

        // Step 3: Move north towards NPC
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.North);

        // Step 4: Attack NPC
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.UseAbilitySlot, 0, (int)Direction.North);

        // Step 5: Wait
        WebTestHelper.PerformAndVerify(player, listener, game.Services, ActorActionType.Wait);

        // Step 6: Slot and drink ogre potion
        var ogrePotionAbility = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, "potion of ogreness: Drink")];
        if (ogrePotionAbility != null)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.SetAbilitySlot, ogrePotionAbility.Id, 5);
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.UseAbilitySlot, 5);
        }

        // Step 7: Move east
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.East);

        // Step 8: Drop dagger
        var daggerEntity = playerEntity.Being!.Items.SingleOrDefault(i => i.Item!.TemplateName == "dagger");
        if (daggerEntity != null)
        {
            WebTestHelper.PerformAndVerify(player, listener, game.Services,
                ActorActionType.DropItem, daggerEntity.Id);
        }
    }
}

