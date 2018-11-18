using UnicornHack.Data.Creatures;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Levels
{
    public class TravelSystemTest
    {
        [Fact]
        public void GetPossibleMovementDirections_returns_correct_directions()
        {
            var level = TestHelper.BuildLevel(@"
.#.
.>.
...");
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(0, 1));
            var manager = nymph.Manager;
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(new[] { Direction.North, Direction.Southeast },
                manager.TravelSystem.GetPossibleMovementDirections(nymph.Position, safe: true, manager));
        }
    }
}
