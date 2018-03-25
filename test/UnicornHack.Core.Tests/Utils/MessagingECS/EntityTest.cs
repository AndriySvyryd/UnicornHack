using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class EntityTest
    {
        [Fact]
        public void AddComponent()
        {
            var manager = TestHelper.CreateGameManager();
            using (var entityReference = manager.CreateEntity())
            {
                var component =
                    entityReference.Referenced.AddComponent<PositionComponent>((int)EntityComponent.Position);

                Assert.Same(component, entityReference.Referenced.FindComponent((int)EntityComponent.Position));
            }
        }
    }
}
