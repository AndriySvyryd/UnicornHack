using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class EntityMatcherTest
    {
        [Fact]
        public void Matches()
        {
            var manager = TestHelper.CreateGameManager();
            using (var entityReference = manager.CreateEntity())
            {
                var entity = entityReference.Referenced;
                entity.AddComponent<AIComponent>((int)EntityComponent.AI);
                entity.AddComponent<BeingComponent>((int)EntityComponent.Being);

                Assert.True(new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.AI).AllOf((int)EntityComponent.Being)
                    .Matches(entity));
                Assert.False(new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.AI).AllOf((int)EntityComponent.Player)
                    .Matches(entity));

                Assert.True(new EntityMatcher<GameEntity>().AnyOf((int)EntityComponent.AI).AnyOf((int)EntityComponent.Player)
                    .Matches(entity));
                Assert.False(new EntityMatcher<GameEntity>().AnyOf((int)EntityComponent.Physical).AnyOf((int)EntityComponent.Player)
                    .Matches(entity));

                Assert.True(new EntityMatcher<GameEntity>().NoneOf((int)EntityComponent.Physical).NoneOf((int)EntityComponent.Player)
                    .Matches(entity));
                Assert.False(new EntityMatcher<GameEntity>().NoneOf((int)EntityComponent.AI).NoneOf((int)EntityComponent.Player)
                    .Matches(entity));

                Assert.True(new EntityMatcher<GameEntity>()
                    .AllOf((int)EntityComponent.AI)
                    .AnyOf((int)EntityComponent.Being)
                    .NoneOf((int)EntityComponent.Player)
                    .Matches(entity));
                Assert.False(new EntityMatcher<GameEntity>()
                    .AllOf((int)EntityComponent.AI)
                    .AnyOf((int)EntityComponent.Physical)
                    .NoneOf((int)EntityComponent.Player)
                    .Matches(entity));
            }
        }
    }
}
