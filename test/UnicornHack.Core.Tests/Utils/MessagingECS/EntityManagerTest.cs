using UnicornHack.Systems.Levels;

namespace UnicornHack.Utils.MessagingECS;

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
            entity.AddComponent<ConnectionComponent>((int)EntityComponent.Connection);

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

            Assert.Same(entity, manager.FindEntity(entity.Id));

            RemoveComponentMessage.Enqueue(entity, EntityComponent.Level, manager);

            Assert.NotNull(manager.FindEntity(entity.Id));
            Assert.True(entity.HasComponent(EntityComponent.Level));

            manager.Queue.ProcessQueue(manager);
            Assert.False(entity.HasComponent(EntityComponent.Level));
        }

        Assert.Null(manager.FindEntity(entity.Id));
    }
}
