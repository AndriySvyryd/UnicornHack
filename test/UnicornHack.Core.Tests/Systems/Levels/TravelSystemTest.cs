using System.Collections.Generic;
using System.Linq;
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
            var undine = CreatureData.Undine.Instantiate(level, new Point(0, 1));
            var manager = undine.Manager;
            PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(new[] { Direction.North, Direction.Southeast },
                manager.TravelSystem.GetPossibleMovementDirections(undine.Position, safe: true));
        }

        [Fact]
        public void Can_travel_to_other_levels()
        {
            var level1 = TestHelper.BuildLevel(@"
.#.
.>.
...");
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level1, new Point(1, 0));
            var manager = player.Manager;

            manager.Queue.ProcessQueue(manager);

            var level2 = TestHelper.BuildLevel(@"
...
..#
..<", game: manager.Game);

            var connection1Entity = level1.Connections.GetValueOrDefault(new Point(1, 1));
            var connection1 = connection1Entity.Connection;

            var connection2Entity = level2.Connections.GetValueOrDefault(new Point(2, 2));

            connection1.TargetLevelId = level2.EntityId;
            connection1.TargetLevelCell = connection2Entity.Position.LevelCell;

            var travelMessage = TravelMessage.Create(manager);
            travelMessage.ActorEntity = player;
            travelMessage.TargetHeading = Direction.South;
            travelMessage.TargetCell = connection1Entity.Position.LevelCell;
            manager.Enqueue(travelMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(level2.EntityId, player.Position.LevelId);
            Assert.Equal(connection1.TargetLevelCell, player.Position.LevelCell);
        }
    }
}
