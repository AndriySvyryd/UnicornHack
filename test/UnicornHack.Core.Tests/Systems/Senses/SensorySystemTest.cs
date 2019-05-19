using System.Linq;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Senses
{
    public class SensorySystemTest
    {
        private static readonly string TestMap = @"
...
...
..#";

        [Fact]
        public void Visibility_updated_on_travel()
        {
            var level = TestHelper.BuildLevel(TestMap);
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(0, 1));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            player.Position.Heading = Direction.West;
            var manager = player.Manager;
            ItemData.Dagger.Instantiate(nymph);

            manager.Queue.ProcessQueue(manager);

            var dagger = manager.EntityItemsToContainerRelationship[nymph.Id].Single();
            Assert.Equal(SenseType.Sight, manager.SensorySystem.CanSense(player, nymph));
            Assert.Equal(SenseType.Sight, manager.SensorySystem.SensedByPlayer(nymph, nymph.Position.LevelCell));
            Assert.Equal(SenseType.Sight | SenseType.Touch | SenseType.Telepathy, manager.SensorySystem.CanSense(player, player));
            Assert.Equal(SenseType.Sight | SenseType.Touch | SenseType.Telepathy,
                manager.SensorySystem.SensedByPlayer(player, player.Position.LevelCell));
            Assert.Equal(SenseType.Sight, manager.SensorySystem.CanSense(player, dagger));
            Assert.Equal(SenseType.Sight, manager.SensorySystem.SensedByPlayer(dagger, nymph.Position.LevelCell));
            var expectedVisibleMap = @"
.
.
.";
            TestHelper.AssertVisibility(level, expectedVisibleMap, level.VisibleTerrain);
            var expectedKnownMap = @"
.
.
.";
            TestHelper.AssertTerrain(level, expectedKnownMap, level.KnownTerrain);

            var travelMessage = TravelMessage.Create(manager);
            travelMessage.Entity = player;
            travelMessage.TargetHeading = Direction.South;
            travelMessage.TargetCell = player.Position.LevelCell;
            manager.Enqueue(travelMessage);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, nymph));
            Assert.Equal(SenseType.None, manager.SensorySystem.SensedByPlayer(nymph, nymph.Position.LevelCell));
            Assert.Equal(SenseType.None, manager.SensorySystem.CanSense(player, dagger));
            Assert.Equal(SenseType.None, manager.SensorySystem.SensedByPlayer(dagger, nymph.Position.LevelCell));
            expectedVisibleMap = @"
 
 
..#";
            TestHelper.AssertVisibility(level, expectedVisibleMap, level.VisibleTerrain);
            expectedKnownMap = @"
.
.
..#";
            TestHelper.AssertTerrain(level, expectedKnownMap, level.KnownTerrain);
        }
    }
}
