using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Utils.MessagingECS;

public class EntityTest
{
    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(0x80000000u)]
    [InlineData(0x80000001u)]
    [InlineData(uint.MaxValue)]
    public void Component_masks_cover_all_bits_queries_matchers_and_pool_reuse(uint present)
    {
        var manager = new MaskEntityManager();
        manager.Initialize(new SequentialMessageQueue<Entity>());
        Entity entity;
        using (var reference = manager.CreateEntity())
        {
            entity = reference.Referenced;
            for (var componentId = 0; componentId < 32; componentId++)
            {
                if ((present & (1u << componentId)) != 0)
                {
                    entity.AddComponent(new MaskComponent(componentId));
                }
            }

            Assert.Equal(present, entity.ComponentMask);
            for (var componentId = 0; componentId < 32; componentId++)
            {
                Assert.Equal((present & (1u << componentId)) != 0, entity.HasComponent(componentId));
            }
            foreach (var query in new uint[] { 0, 1, 0x80000000, 0x80000001, uint.MaxValue })
            {
                var ids = Enumerable.Range(0, 32).Where(componentId => (query & (1u << componentId)) != 0).ToArray();
                var all = (present & query) == query;
                var any = (present & query) != 0;
                Assert.Equal(all, entity.HasComponents(query));
                Assert.Equal(any, entity.HasAnyComponent(query));
                Assert.Equal(all, new EntityMatcher<Entity>().AllOf(ids).Matches(entity));
                Assert.Equal(any, new EntityMatcher<Entity>().AnyOf(ids).Matches(entity));
                Assert.Equal(!any, new EntityMatcher<Entity>().NoneOf(ids).Matches(entity));
                var registrations = new EntityMatcher<Entity>().AllOf(ids).AnyOf(ids).NoneOf(ids);
                Assert.Equal(ids, registrations.GetAllIds());
                Assert.Same(registrations.GetAllIds(), registrations.GetAllIds());
            }

            Assert.Equal(entity.HasComponent(31), entity.HasComponents(0x80000000u));
            Assert.Equal(entity.HasComponent(31), new EntityMatcher<Entity>().AllOf(31, 31).Matches(entity));
            Assert.Throws<ArgumentOutOfRangeException>(() => entity.HasComponent(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => entity.HasComponent(32));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EntityMatcher<Entity>().AnyOf(32));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Entity().Initialize(1, 33, manager));
        }

        Assert.Equal(0u, entity.ComponentMask);
        using var reusedReference = manager.CreateEntity();
        Assert.Same(entity, reusedReference.Referenced);
        Assert.Equal(0u, reusedReference.Referenced.ComponentMask);
        reusedReference.Referenced.AddComponent(new MaskComponent(31));
        Assert.Equal(0x80000000u, reusedReference.Referenced.ComponentMask);
        reusedReference.Referenced.RemoveComponent(31);
        Assert.Equal(0u, reusedReference.Referenced.ComponentMask);
    }

    public sealed class MaskComponent : Component
    {
        public MaskComponent()
        {
        }

        public MaskComponent(int componentId)
        {
            ComponentId = componentId;
        }
    }

    private sealed class MaskEntityManager() : EntityManager<Entity>(32, 1, 1, 1)
    {
        private int _nextId;

        protected override void InitializeSystems(IMessageQueue queue)
        {
            for (var componentId = 0; componentId < ComponentCount; componentId++)
            {
                Add<MaskComponent>(componentId, 1);
            }
        }

        public override int GetNextEntityId() => ++_nextId;

        public override string GetComponentPropertyName(int componentId) => "Component" + componentId;
    }

    [Fact]
    public void AddComponent()
    {
        var manager = TestHelper.CreateGameManager();
        using (var entityReference = manager.CreateEntity())
        {
            var entity = entityReference.Referenced;
            var component = entity.AddComponent<PositionComponent>((int)EntityComponent.Position);

            Assert.Same(component, entityReference.Referenced.FindComponent((int)EntityComponent.Position));
            Assert.Equal(1u << (int)EntityComponent.Position, entity.ComponentMask);

            using (var anotherEntityReference = manager.CreateEntity())
            {
                Assert.Throws<InvalidOperationException>(() =>
                    anotherEntityReference.Referenced.RemoveComponent(component));
            }

            entity.RemoveComponent(component);

            Assert.False(entity.HasComponent(EntityComponent.Position));
            Assert.Equal(0u, entity.ComponentMask);

            entity.RemoveComponent(component);
        }
    }

    [Fact]
    public void Readding_the_same_component_is_a_noop_and_replacing_it_throws()
    {
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        var position = entity.AddComponent<PositionComponent>((int)EntityComponent.Position);
        var componentChanges = 0;
        entity.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameEntity.Position))
            {
                componentChanges++;
            }
        };

        entity.Position = position;

        Assert.Equal(0, componentChanges);
        Assert.Same(position, entity.Position);

        var replacement = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        Assert.Throws<InvalidOperationException>(() => entity.Position = replacement);
        Assert.Same(position, entity.Position);
    }

    [Fact]
    public void Setting_component_property_fires_events()
    {
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var component = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);

        AbilityComponent? expectedCurrentValue = null;
        var expectedNewValue = component;

        var entity = entityReference.Referenced;
        var propertyChangingCalled = false;
        entity.PropertyChanging += (s, e) =>
        {
            propertyChangingCalled = true;

            Assert.Equal(nameof(GameEntity.Ability), e.PropertyName);
            Assert.Equal(expectedCurrentValue, ((GameEntity)s!).Ability);
            Assert.Equal(expectedCurrentValue != null, ((GameEntity)s).HasComponent(EntityComponent.Ability));
        };

        var propertyChangedCalled = false;
        entity.PropertyChanged += (s, e) =>
        {
            propertyChangedCalled = true;

            Assert.Equal(nameof(GameEntity.Ability), e.PropertyName);
            Assert.Equal(expectedNewValue, ((GameEntity)s!).Ability);
            Assert.Equal(expectedNewValue != null, ((GameEntity)s).HasComponent(EntityComponent.Ability));
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

    [Fact]
    public void ForEachComponent()
    {
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        var ai = entity.GetOrAddComponent<AIComponent>(EntityComponent.AI);
        var being = entity.GetOrAddComponent<BeingComponent>(EntityComponent.Being);

        Assert.True(entity.HasComponents((1u << (int)EntityComponent.AI) | (1u << (int)EntityComponent.Being)));
        Assert.False(entity.HasComponents((1u << (int)EntityComponent.Player) | (1u << (int)EntityComponent.Being)));
        Assert.True(entity.HasAnyComponent((1u << (int)EntityComponent.Player) | (1u << (int)EntityComponent.Being)));
        Assert.False(entity.HasAnyComponent((1u << (int)EntityComponent.Player) | (1u << (int)EntityComponent.Knowledge)));

        var count = 0;
        entity.ForEachComponent(
            new Dictionary<int, Component>
            {
                { (int)EntityComponent.AI, ai }, { (int)EntityComponent.Being, being }
            },
            (s, id, c) =>
            {
                count++;
                Assert.Same(s[id], c);
            });

        Assert.Equal(2, count);
    }

    [Fact]
    public void Removing_reference_returns_to_pool()
    {
        var manager = TestHelper.CreateGameManager();
        GameEntity entity;
        int id;
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
        Assert.Equal(0u, entity.ComponentMask);
        Assert.Null(entity.Manager);
        Assert.Null(manager.FindEntity(id));

        GameEntity? reused = null;
        for (var i = 0; i < 8192 && reused is null; i++)
        {
            using var entityReference = manager.CreateEntity();
            if (ReferenceEquals(entityReference.Referenced, entity))
            {
                reused = entityReference.Referenced;
                Assert.NotEqual(id, reused.Id);
            }
        }

        Assert.Same(entity, reused);

        Assert.Throws<InvalidOperationException>(() => entity.RemoveReference(manager));
        Assert.True(PoolContainsEntity(manager, entity));
    }

    [Fact]
    public void Removing_from_tracker_does_not_return_to_pool_until_removed_from_entity()
    {
        var manager = TestHelper.CreateGameManager();
        GameEntity entity;
        using (var entityReference = manager.CreateEntity())
        {
            entity = entityReference.Referenced;

            manager.Game.Repository.RemoveTracked(entity);

            using (var anotherEntityReference = manager.CreateEntity())
            {
                Assert.NotSame(entity, anotherEntityReference.Referenced);
            }

            entity.AddComponent<ConnectionComponent>((int)EntityComponent.Connection);
        }

        Assert.Null(entity.Manager);

        Assert.True(PoolContainsEntity(manager, entity));
    }

    // The entity pool is FIFO, so a just-returned instance goes to the back of the queue and isn't
    // handed back on the very next rent when other instances are already pooled. Rent (disposing
    // each non-match back to the pool so the FIFO queue rotates) until the instance reappears.
    private static bool PoolContainsEntity(GameManager manager, GameEntity target)
    {
        for (var i = 0; i < 16384; i++)
        {
            using var entityReference = manager.CreateEntity();
            if (ReferenceEquals(entityReference.Referenced, target))
            {
                return true;
            }
        }

        return false;
    }
}
