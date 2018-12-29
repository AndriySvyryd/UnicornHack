using System.Linq;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Faculties
{
    public class SkillAbilitiesSystemTest
    {
        [Fact]
        public void Skills_add_abilities()
        {
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            var player = playerEntity.Player;
            ItemData.SkillbookOfWaterSourcery.Instantiate(playerEntity);
            var manager = playerEntity.Manager;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, player.WaterSourcery);
            Assert.Null(manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                   .FirstOrDefault(a => a.Ability.Name == "ice shard"));

            var firstWaterAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                   .First(a => a.Ability.Name == ItemData.SkillbookOfWaterSourcery.Name + ": Consult");
            ItemData.SkillbookOfWaterSourcery.Instantiate(playerEntity);
            ItemData.SkillbookOfConjuration.Instantiate(playerEntity);
            ActivateAbility(firstWaterAbility, 0, manager);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, player.WaterSourcery);
            Assert.Equal(0, player.Conjuration);

            var iceShardAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                   .First(a => a.Ability.Name == "ice shard");
            Assert.True(iceShardAbility.Ability.IsUsable);

            var otherWaterAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                   .First(a => a.Ability.Name == ItemData.SkillbookOfWaterSourcery.Name + ": Consult" && a != firstWaterAbility);
            ActivateAbility(otherWaterAbility, 1, manager);

            var conjurationAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                   .First(a => a.Ability.Name == ItemData.SkillbookOfConjuration.Name + ": Consult");
            ActivateAbility(conjurationAbility, 2, manager);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(2, player.WaterSourcery);
            Assert.Equal(1, player.Conjuration);
            Assert.True(iceShardAbility.Ability.IsUsable);

            DeactivateAbility(firstWaterAbility, manager);
            DeactivateAbility(otherWaterAbility, manager);
            DeactivateAbility(conjurationAbility, manager);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, player.WaterSourcery);
            Assert.False(iceShardAbility.Ability.IsUsable);
        }

        private static GameEntity ActivateAbility(GameEntity abilityEntity, int slot, GameManager manager)
        {
            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = abilityEntity;
            setSlotMessage.Slot = slot;

            manager.Enqueue(setSlotMessage);

            return abilityEntity;
        }

        private static void DeactivateAbility(GameEntity abilityEntity, GameManager manager)
        {
            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = abilityEntity;

            manager.Enqueue(setSlotMessage);
        }
    }
}
