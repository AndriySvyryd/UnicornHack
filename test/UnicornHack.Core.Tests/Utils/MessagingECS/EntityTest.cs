using System;
using System.Collections.Generic;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
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
                var entity = entityReference.Referenced;
                var component = entity.AddComponent<PositionComponent>((int)EntityComponent.Position);

                Assert.Same(component, entityReference.Referenced.FindComponent((int)EntityComponent.Position));

                using (var anotherEntityReference = manager.CreateEntity())
                {
                    Assert.Throws<InvalidOperationException>(() =>
                        anotherEntityReference.Referenced.RemoveComponent(component));
                }

                entity.RemoveComponent(component);

                Assert.False(entity.HasComponent(EntityComponent.Position));

                entity.RemoveComponent(component);
            }
        }

        [Fact]
        public void Setting_component_property_fires_events()
        {
            var manager = TestHelper.CreateGameManager();
            using (var entityReference = manager.CreateEntity())
            {
                var component = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);

                AbilityComponent expectedCurrentValue = null;
                var expectedNewValue = component;

                var entity = entityReference.Referenced;
                var propertyChangingCalled = false;
                entity.PropertyChanging += (s, e) =>
                {
                    propertyChangingCalled = true;

                    Assert.Equal(nameof(GameEntity.Ability), e.PropertyName);
                    Assert.Equal(expectedCurrentValue, ((GameEntity)s).Ability);
                };

                var propertyChangedCalled = false;
                entity.PropertyChanged += (s, e) =>
                {
                    propertyChangedCalled = true;

                    Assert.Equal(nameof(GameEntity.Ability), e.PropertyName);
                    Assert.Equal(expectedNewValue, ((GameEntity)s).Ability);
                };

                entity.Ability = null;

                Assert.False(propertyChangingCalled);
                Assert.False(propertyChangedCalled);

                entity.Ability = component;

                Assert.True(propertyChangingCalled);
                Assert.True(propertyChangedCalled);
                Assert.Same(component, entity.FindComponent((int)EntityComponent.Ability));
                Assert.True(entity.HasComponent(EntityComponent.Ability));

                propertyChangingCalled = false;
                propertyChangedCalled = false;
                expectedCurrentValue = component;
                expectedNewValue = null;

                entity.Ability = null;

                Assert.True(propertyChangingCalled);
                Assert.True(propertyChangedCalled);
                Assert.Null(entity.FindComponent((int)EntityComponent.Ability));
                Assert.False(entity.HasComponent(EntityComponent.Ability));
            }
        }

        [Fact]
        public void ForEachComponent()
        {
            var manager = TestHelper.CreateGameManager();
            using (var entityReference = manager.CreateEntity())
            {
                var entity = entityReference.Referenced;
                var ai = entity.GetOrAddComponent<AIComponent>(EntityComponent.AI);
                var being = entity.GetOrAddComponent<BeingComponent>(EntityComponent.Being);

                Assert.True(entity.HasComponents(new[] { (int)EntityComponent.AI, (int)EntityComponent.Being }));
                Assert.False(entity.HasComponents(new[] { (int)EntityComponent.Player, (int)EntityComponent.Being }));
                Assert.True(entity.HasAnyComponent(new[] { (int)EntityComponent.Player, (int)EntityComponent.Being }));
                Assert.False(entity.HasAnyComponent(new[] { (int)EntityComponent.Player, (int)EntityComponent.Knowledge }));

                var count = 0;
                entity.ForEachComponent(new Dictionary<int, Component>
                {
                    { (int)EntityComponent.AI, ai },
                    { (int)EntityComponent.Being, being }
                },
                    (s, id, c) =>
                {
                    count++;
                    Assert.Same(s[id], c);
                });

                Assert.Equal(2, count);
            }
        }

        [Fact]
        public void Removing_reference_returns_to_pool()
        {
            var manager = TestHelper.CreateGameManager();
            GameEntity entity = null;
            var id = 0;
            using (var entityReference = manager.CreateEntity())
            {
                entity = entityReference.Referenced;
                entity.AddComponent<ConnectionComponent>((int)EntityComponent.Connection);

                id = entity.Id;
                Assert.Same(entity, manager.FindEntity(id));
            }

            var count = 0;
            entity.ForEachComponent(count, (_, __, ___) => count++);
            Assert.Equal(0, count);
            Assert.Equal(0, entity.Id);
            Assert.Null(entity.Manager);
            Assert.Null(manager.FindEntity(id));

            using (var entityReference = manager.CreateEntity())
            {
                Assert.Same(entity, entityReference.Referenced);
                Assert.NotEqual(id, entityReference.Referenced.Id);
            }

            Assert.Throws<InvalidOperationException>(() => entity.RemoveReference(manager));
        }

        [Fact]
        public void Removing_from_tracker_does_not_return_to_pool_until_removed_from_entity()
        {
            var manager = TestHelper.CreateGameManager();
            GameEntity entity = null;
            using (var entityReference = manager.CreateEntity())
            {
                entity = entityReference.Referenced;

                manager.Game.Repository.RemoveTracked(entity);

                using (var anotherEntityReference = manager.CreateEntity())
                {
                    Assert.NotSame(entity, anotherEntityReference.Referenced);
                }

                var component = entity.AddComponent<ConnectionComponent>((int)EntityComponent.Connection);
            }

            Assert.Null(entity.Manager);

            using (var entityReference = manager.CreateEntity())
            {
                Assert.Same(entity, entityReference.Referenced);
            }
        }
    }
}
