using System.Collections.Generic;
using UnicornHack.Data.Creatures;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyCollectionAccessorTest
    {
        [Fact]
        public void Can_get_and_set_dependent()
        {
            var level = TestHelper.BuildLevel(".");
            var manager = level.Entity.Manager;

            var creature = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));

            manager.Queue.ProcessQueue(manager);

            var accessor =
                new PropertyCollectionAccessor<GameEntity, Dictionary<int, GameEntity>, KeyValuePair<int, GameEntity>>(e
                    => (Dictionary<int, GameEntity>)e.Being.SlottedAbilities);
            accessor.SetDefaultFactory(() => new Dictionary<int, GameEntity>());

            Assert.Null(accessor.GetDependents(creature));
            Assert.Null(creature.Being.SlottedAbilities);

            Assert.Empty(accessor.GetOrCreateDependents(creature));
            Assert.Empty(creature.Being.SlottedAbilities!);
        }
    }
}
