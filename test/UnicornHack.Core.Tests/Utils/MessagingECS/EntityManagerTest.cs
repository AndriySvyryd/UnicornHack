using System.Linq;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class EntityManagerTest
    {
        [Fact]
        public void AddEntity()
        {
            var manager = TestHelper.CreateGameManager();
            GameEntity entity;
            using (var entityReference = ((IEntityManager)manager).CreateEntity())
            {
                entity = (GameEntity)entityReference.Referenced;
                entity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

                Assert.Same(entity, ((IEntityManager)manager).FindEntity(entity.Id));
                Assert.Same(entity, manager.GetEntities().Single());
            }

            manager.AddEntity(entity);
            Assert.Same(entity, manager.FindEntity(entity.Id));
            Assert.Same(entity, manager.GetEntities().Single());
        }

        [Fact]
        public void RemoveComponent()
        {
            var manager = TestHelper.CreateGameManager();
            GameEntity entity;
            using (var entityReference = ((IEntityManager)manager).CreateEntity())
            {
                entity = (GameEntity)entityReference.Referenced;
                entity.AddComponent<LevelComponent>((int)EntityComponent.Level);
            }

            Assert.Same(entity, manager.FindEntity(entity.Id));

            using(var message = manager.CreateRemoveComponentMessage())
            {
                message.Entity = entity;
                message.Component = EntityComponent.Level;

                manager.Process(message, manager);

                Assert.False(entity.HasComponent(EntityComponent.Level));
                Assert.Null(manager.FindEntity(entity.Id));
            }
        }
    }
}
