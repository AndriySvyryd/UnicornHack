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
            entity.AddComponent<RoomComponent>((int)EntityComponent.Room);

            Assert.Same(entity, manager.FindEntity(entity.Id));

            RemoveComponentMessage.Enqueue(entity, EntityComponent.Room, manager);

            Assert.NotNull(manager.FindEntity(entity.Id));
            Assert.True(entity.HasComponent(EntityComponent.Room));

            manager.Queue.ProcessQueue(manager);
            Assert.False(entity.HasComponent(EntityComponent.Room));
        }

        Assert.Null(manager.FindEntity(entity.Id));
    }
}
