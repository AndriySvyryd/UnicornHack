using UnicornHack.Systems.Items;

namespace UnicornHack.Utils.MessagingECS;

public class ComponentTest
{
    [Fact]
    public void ComponentId_is_set()
    {
        var manager = TestHelper.CreateGameManager();
        var component = manager.CreateComponent<ItemComponent>(EntityComponent.Item);

        Assert.Equal((int)EntityComponent.Item, component.ComponentId);
    }

    [Fact]
    public void Updating_properties_fires_events()
    {
        var manager = TestHelper.CreateGameManager();
        var component = manager.CreateComponent<ItemComponent>(EntityComponent.Item);

        var propertyChangingCalled = false;
        component.PropertyChanging += (s, e) =>
        {
            propertyChangingCalled = true;

            Assert.Equal(nameof(ItemComponent.Name), e.PropertyName);
            Assert.Null(((ItemComponent)s!).Name);
        };

        var propertyChangedCalled = false;
        component.PropertyChanged += (s, e) =>
        {
            propertyChangedCalled = true;

            Assert.Equal(nameof(ItemComponent.Name), e.PropertyName);
            Assert.Equal("foo", ((ItemComponent)s!).Name);
        };

        component.Name = null;

        Assert.False(propertyChangingCalled);
        Assert.False(propertyChangedCalled);

        component.Name = "foo";

        Assert.True(propertyChangingCalled);
        Assert.True(propertyChangedCalled);
    }

    [Fact]
    public void Removing_from_entity_returns_to_pool()
    {
        var manager = TestHelper.CreateGameManager();
        var component = manager.CreateComponent<ItemComponent>(EntityComponent.Item);
        var entity = manager.CreateEntity().Referenced;
        ((Component)component).Entity = entity;

        Assert.Same(entity, component.Entity);

        ((IReferenceable)component).RemoveReference(entity);

        Assert.Null(component.Entity);

        Assert.True(PoolContainsComponent(manager, component));
    }

    [Fact]
    public void Removing_from_entity_returns_to_pool_when_tracked()
    {
        var manager = TestHelper.CreateGameManager();
        var component = manager.CreateComponent<ItemComponent>(EntityComponent.Item);
        manager.Game.Repository.Add(component);
        component.Count = 1;

        var entity = manager.CreateEntity().Referenced;
        ((IReferenceable)component).AddReference(manager);
        ((Component)component).Entity = entity;
        ((IReferenceable)component).RemoveReference(entity);
        ((IReferenceable)component).RemoveReference(manager);

        Assert.Null(component.Count);

        Assert.True(PoolContainsComponent(manager, component));
    }

    [Fact]
    public void Removing_from_tracker_does_not_return_to_pool_until_removed_from_entity()
    {
        var manager = TestHelper.CreateGameManager();
        var component = manager.CreateComponent<ItemComponent>(EntityComponent.Item);
        var entity = manager.CreateEntity().Referenced;
        ((Component)component).Entity = entity;
        manager.Game.Repository.Add(component);
        manager.Game.Repository.RemoveTracked(component);

        var anotherComponent = manager.CreateComponent<ItemComponent>(EntityComponent.Item);

        Assert.NotSame(component, anotherComponent);

        ((IReferenceable)component).RemoveReference(entity);

        Assert.True(PoolContainsComponent(manager, component));
    }

    // The component pool is FIFO, so a just-returned instance goes to the back of the queue and
    // isn't handed back on the very next rent when other instances are already pooled. Rent until
    // the instance reappears (bounded by an ample cap) to assert it was returned to the pool.
    private static bool PoolContainsComponent(GameManager manager, GameComponent target)
    {
        for (var i = 0; i < 1024; i++)
        {
            if (ReferenceEquals(manager.CreateComponent<ItemComponent>(EntityComponent.Item), target))
            {
                return true;
            }
        }

        return false;
    }
}
