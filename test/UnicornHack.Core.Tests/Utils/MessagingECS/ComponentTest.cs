using UnicornHack.Systems.Items;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class ComponentTest
    {
        [Fact]
        public void ComponentId_is_set()
        {
            var manager = TestHelper.CreateGameManager();
            var component = manager.CreateComponent<ItemComponent>(EntityComponent.Item);

            Assert.Equal(component.ComponentId, (int)EntityComponent.Item);
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
                Assert.Null(((ItemComponent)s).Name);
            };

            var propertyChangedCalled = false;
            component.PropertyChanged += (s, e) =>
            {
                propertyChangedCalled = true;

                Assert.Equal(nameof(ItemComponent.Name), e.PropertyName);
                Assert.Equal("foo", ((ItemComponent)s).Name);
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

            ((IOwnerReferenceable)component).RemoveReference(entity);

            Assert.Null(component.Entity);

            var anotherComponent = manager.CreateComponent<ItemComponent>(EntityComponent.Item);

            Assert.Same(component, anotherComponent);
        }

        [Fact]
        public void Removing_from_entity_returns_to_pool_when_tracked()
        {
            var manager = TestHelper.CreateGameManager();
            var component = manager.CreateComponent<ItemComponent>(EntityComponent.Item);
            manager.Game.Repository.Add(component);
            component.Count = 1;

            var entity = manager.CreateEntity().Referenced;
            ((IOwnerReferenceable)component).AddReference(manager);
            ((Component)component).Entity = entity;
            ((IOwnerReferenceable)component).RemoveReference(entity);
            ((IOwnerReferenceable)component).RemoveReference(manager);

            Assert.Null(component.Count);

            var anotherComponent = manager.CreateComponent<ItemComponent>(EntityComponent.Item);

            Assert.Same(component, anotherComponent);
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

            ((IOwnerReferenceable)component).RemoveReference(entity);

            anotherComponent = manager.CreateComponent<ItemComponent>(EntityComponent.Item);

            Assert.Same(component, anotherComponent);
        }
    }
}
