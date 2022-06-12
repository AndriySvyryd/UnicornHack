using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Beings
{
    public class LivingSystemTest
    {
        [Fact]
        public void Hp_and_ep_depend_on_might_and_focus_and_regenerate_with_xp()
        {
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));

            var manager = playerEntity.Manager;
            manager.Queue.ProcessQueue(manager);

            ItemData.PotionOfOgreness.Instantiate(playerEntity);

            manager.Queue.ProcessQueue(manager);

            var being = playerEntity.Being;
            Assert.Equal(100, being.EnergyPointMaximum);
            Assert.Equal(100, being.EnergyPoints);
            Assert.Equal(100, being.HitPointMaximum);
            Assert.Equal(100, being.HitPoints);

            being.EnergyPoints -= 50;
            being.HitPoints -= 50;
            manager.XPSystem.AddPlayerXP(25, manager);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(60, being.EnergyPoints);
            Assert.Equal(60, being.HitPoints);

            TestHelper.ActivateAbility(ItemData.PotionOfOgreness.Name + ": Drink", playerEntity, manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(90, being.EnergyPointMaximum);
            Assert.Equal(54, being.EnergyPoints);
            Assert.Equal(120, being.HitPointMaximum);
            Assert.Equal(72, being.HitPoints);
        }

        [Fact]
        public void Reserved_ep_does_not_replenish()
        {
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
            var manager = playerEntity.Manager;

            manager.Queue.ProcessQueue(manager);

            var being = playerEntity.Being;
            Assert.Equal(100, being.EnergyPointMaximum);
            Assert.Equal(100, being.EnergyPoints);
            Assert.Equal(0, being.ReservedEnergyPoints);

            being.EnergyPoints += 50;

            Assert.Equal(100, being.EnergyPoints);

            being.EnergyPoints -= 120;

            Assert.Equal(0, being.EnergyPoints);

            being.ReservedEnergyPoints = -50;

            Assert.Equal(0, being.ReservedEnergyPoints);

            being.EnergyPoints = 100;
            being.ReservedEnergyPoints = 60;

            Assert.Equal(40, being.EnergyPoints);

            being.EnergyPoints = 100;

            Assert.Equal(40, being.EnergyPoints);

            being.ReservedEnergyPoints = 150;
            being.EnergyPointMaximum = 200;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(100, being.ReservedEnergyPoints);
            Assert.Equal(0, being.EnergyPoints);
            Assert.Equal(200, being.EnergyPointMaximum);

            being.ReservedEnergyPoints = 0;
            being.EnergyPoints = 50;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, being.ReservedEnergyPoints);
            Assert.Equal(50, being.EnergyPoints);
        }
    }
}
