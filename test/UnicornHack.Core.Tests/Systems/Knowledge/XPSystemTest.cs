using System.Linq;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Knowledge
{
    public class XPSystemTest
    {
        [Fact]
        public void Xp_gained_on_exploration()
        {
            var level = TestHelper.BuildLevel(@"
...
>..
..#");
            level.Difficulty = 3;

            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            player.Position.Heading = Direction.West;
            var manager = player.Manager;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(9, player.Being.ExperiencePoints);

            var travelMessage = manager.TravelSystem.CreateTravelMessage(manager);
            travelMessage.Entity = player;
            travelMessage.TargetHeading = Direction.East;
            travelMessage.TargetCell = new Point(0, 0);
            manager.Enqueue(travelMessage);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(27, player.Being.ExperiencePoints);
        }

        [Fact]
        public void Xp_gained_on_enemy_death()
        {
            var level = TestHelper.BuildLevel("..");
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(0, 0));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(1, 0));
            player.Position.Heading = Direction.West;
            var manager = player.Manager;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(2, player.Being.ExperiencePoints);

            nymph.Being.HitPoints = 0;
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(302, player.Being.ExperiencePoints);
        }

        [Fact]
        public void Races_level_up()
        {
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            var player = playerEntity.Player;
            ItemData.PotionOfElfness.Instantiate(playerEntity);
            var manager = playerEntity.Manager;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, player.SkillPoints);
            Assert.Equal(0, player.TraitPoints);
            Assert.Equal(0, player.MutationPoints);
            Assert.Equal(1000, player.NextLevelXP);

            manager.XPSystem.AddPlayerXP(player.NextLevelXP, manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(2, manager.XPSystem.GetXPLevel(playerEntity, manager));
            Assert.Equal(3, player.SkillPoints);
            Assert.Equal(3, player.TraitPoints);
            Assert.Equal(0, player.MutationPoints);
            Assert.Equal(2000, player.NextLevelXP);

            var humanEntity = manager.RacesToBeingRelationship[playerEntity.Id].Single().Value;
            var elfnessPotion = manager.EntityItemsToContainerRelationship[playerEntity.Id].Single();
            var activateItemMessage = manager.ItemUsageSystem.CreateActivateItemMessage(manager);
            activateItemMessage.ActivatorEntity = playerEntity;
            activateItemMessage.TargetEntity = playerEntity;
            activateItemMessage.ItemEntity = elfnessPotion;
            activateItemMessage.ActivationType = ActivationType.ManualActivation;

            manager.Enqueue(activateItemMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(2, manager.RacesToBeingRelationship[playerEntity.Id].Count());
            var elfEntity = manager.RacesToBeingRelationship[playerEntity.Id].Values.Single(r => r != humanEntity);
            Assert.Equal(2, humanEntity.Race.Level);
            Assert.Equal(1, elfEntity.Race.Level);
            Assert.Equal(3, manager.XPSystem.GetXPLevel(playerEntity, manager));
            Assert.Equal(3, player.SkillPoints);
            Assert.Equal(3, player.TraitPoints);
            Assert.Equal(0, player.MutationPoints);
            Assert.Equal(3500, player.NextLevelXP);

            ItemData.PotionOfInhumanity.Instantiate(playerEntity);
            manager.XPSystem.AddPlayerXP(player.NextLevelXP, manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(4, manager.XPSystem.GetXPLevel(playerEntity, manager));
            Assert.Equal(16, manager.AbilitiesToAffectableRelationship[playerEntity.Id].Count());
            Assert.Equal(2, humanEntity.Race.Level);
            Assert.Equal(2, elfEntity.Race.Level);
            Assert.Equal(5, player.SkillPoints);
            Assert.Equal(6, player.TraitPoints);
            Assert.Equal(1, player.MutationPoints);
            Assert.Equal(4500, player.NextLevelXP);

            manager.XPSystem.AddPlayerXP(player.NextLevelXP, manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(3, humanEntity.Race.Level);
            Assert.Equal(17, manager.AbilitiesToAffectableRelationship[playerEntity.Id].Count());

            var inhumanityPotion = manager.EntityItemsToContainerRelationship[playerEntity.Id].Single();
            activateItemMessage = manager.ItemUsageSystem.CreateActivateItemMessage(manager);
            activateItemMessage.ActivatorEntity = playerEntity;
            activateItemMessage.TargetEntity = playerEntity;
            activateItemMessage.ItemEntity = inhumanityPotion;
            activateItemMessage.ActivationType = ActivationType.ManualActivation;

            manager.Enqueue(activateItemMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, manager.RacesToBeingRelationship[playerEntity.Id].Count());
            Assert.Equal(2, manager.XPSystem.GetXPLevel(playerEntity, manager));
            Assert.Equal(2, elfEntity.Race.Level);
            Assert.Equal(2, player.SkillPoints);
            Assert.Equal(3, player.TraitPoints);
            Assert.Equal(1, player.MutationPoints);
            Assert.Equal(3500, player.NextLevelXP);

            manager.XPSystem.AddPlayerXP(player.NextLevelXP, manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(3, manager.XPSystem.GetXPLevel(playerEntity, manager));
            Assert.Equal(2000, player.NextLevelXP);
        }
    }
}
