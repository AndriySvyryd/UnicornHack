using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Utils.MessagingECS;

public class EntityGroupTest
{
    [Fact]
    public void Contains_matching_entities()
    {
        var manager = TestHelper.CreateGameManager();
        using (var entityReference = manager.CreateEntity())
        {
            var entity = entityReference.Referenced;
            entity.AddComponent<RoomComponent>((int)EntityComponent.Room);

            Assert.Equal(1, manager.Rooms.Count);
            Assert.True(manager.Rooms.Matcher.Matches(entity));
            Assert.True(manager.Rooms.ContainsEntity(entity.Id));
            Assert.Same(entity, manager.Rooms.FindEntity(entity.Id));
            Assert.Same(entity, manager.Rooms.Single());

            entity.AddComponent<ConnectionComponent>((int)EntityComponent.Connection);

            Assert.Equal(1, manager.Rooms.Count);

            entity.RemoveComponent(EntityComponent.Room);

            Assert.Equal(0, manager.Rooms.Count);
            Assert.False(manager.Rooms.Matcher.Matches(entity));
            Assert.False(manager.Rooms.ContainsEntity(entity.Id));
            Assert.Null(manager.Rooms.FindEntity(entity.Id));
            Assert.Empty(manager.Rooms);
        }

        Assert.Equal(0, manager.Queue.QueuedCount);
    }

    [Fact]
    public void Queues_messages_for_registered_events()
    {
        var manager = TestHelper.CreateGameManager();

        Assert.Equal(nameof(manager.Effects), manager.Effects.Name);

        GroupTestSystem testSystem;
        using (var entityReference = ((IEntityManager)manager).CreateEntity())
        {
            var entity = (GameEntity)entityReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;

            testSystem = new GroupTestSystem(entity, effect);

            manager.Queue.Register<EntityAddedMessage<GameEntity>>(
                testSystem, manager.Effects.GetEntityAddedMessageName(), 10);
            manager.Queue.Register<EntityRemovedMessage<GameEntity>>(
                testSystem, manager.Effects.GetEntityRemovedMessageName(), 10);
            manager.Queue.Register<PropertyValueChangedMessage<GameEntity, string>>(
                testSystem, manager.Effects.GetPropertyValueChangedMessageName(nameof(EffectComponent.DurationAmount)),
                10);

            entity.Effect = effect;

            Assert.Equal(1, manager.Queue.QueuedCount);

            effect.DurationAmount = "10";

            Assert.Equal(2, manager.Queue.QueuedCount);
        }

        Assert.Equal(2, manager.Queue.QueuedCount);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, manager.Queue.QueuedCount);
        Assert.Equal(3, testSystem.MessagesProcessed);
    }

    [Fact]
    public void Queues_typed_messages_for_registered_nested_property_changes()
    {
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        entity.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        var propertyEffect = CreatePropertyEffect(entity.ActiveEffects, 0, 0, 0, EffectType.Stun, null);
        propertyEffect = entity.ActiveEffects.AddPropertyEffect(propertyEffect);
        var testSystem = new NestedGroupTestSystem(entity, entity.ActiveEffects, propertyEffect);

        var messageName = manager.AffectableEntities.GetPropertyValueChangedMessageName(
            nameof(ActiveEffectsComponent.PropertyEffects),
            nameof(ActivePropertyEffect.MagnitudePrimary));
        manager.Queue.Register<PropertyValueChangedMessage<GameEntity, int>>(testSystem, messageName, 10);

        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, testSystem.MessagesProcessed);

        propertyEffect.MagnitudePrimary = 10;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, testSystem.MessagesProcessed);

        entity.ActiveEffects.PropertyEffects.Remove(propertyEffect);
        manager.Queue.ProcessQueue(manager);
        propertyEffect.MagnitudePrimary = 20;
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, testSystem.MessagesProcessed);
    }

    private sealed class NestedGroupTestSystem :
        IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
    {
        private readonly GameEntity _entity;
        private readonly ActiveEffectsComponent _component;
        private readonly ActivePropertyEffect _propertyEffect;

        public NestedGroupTestSystem(
            GameEntity entity, ActiveEffectsComponent component, ActivePropertyEffect propertyEffect)
        {
            _entity = entity;
            _component = component;
            _propertyEffect = propertyEffect;
        }

        public int MessagesProcessed { get; private set; }

        public MessageProcessingResult Process(
            PropertyValueChangedMessage<GameEntity, int> message, GameManager state)
        {
            Assert.Same(_entity, message.Entity);
            Assert.Same(_component, message.ChangedComponent);
            Assert.Equal(nameof(ActiveEffectsComponent.PropertyEffects), message.ChangedPropertyName);
            Assert.Same(_propertyEffect, message.NestedItem);
            Assert.Equal(nameof(ActivePropertyEffect.MagnitudePrimary), message.NestedPropertyName);
            Assert.Equal(0, message.OldValue);
            Assert.Equal(10, message.NewValue);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }
    }

    private class GroupTestSystem :
        IGameSystem<EntityAddedMessage<GameEntity>>,
        IGameSystem<EntityRemovedMessage<GameEntity>>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, string>>
    {
        private readonly GameEntity _testEntity;
        private readonly GameComponent _testComponent;

        public GroupTestSystem(GameEntity testEntity, GameComponent testComponent)
        {
            _testEntity = testEntity;
            _testComponent = testComponent;
        }

        public int MessagesProcessed
        {
            get;
            private set;
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Null(message.RemovedComponent);
            Assert.Equal(0, message.PropertyChanges.Count);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Same(_testComponent, message.RemovedComponent);
            Assert.Equal(0, message.PropertyChanges.Count);
            Assert.Same(_testComponent, message.RemovedComponent);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, string> message,
            GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Equal(_testComponent.ComponentId, message.ChangedComponent?.ComponentId);
            Assert.Same(_testComponent, message.ChangedComponent);
            Assert.Equal(nameof(EffectComponent.DurationAmount), message.ChangedPropertyName);
            Assert.Null(message.OldValue);
            Assert.Equal("10", message.NewValue);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }
    }

    [Fact]
    public void RemoveListener_stops_notifications()
    {
        var manager = TestHelper.CreateGameManager();
        var listener = new CountingListener();
        manager.Effects.AddListener(listener);

        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = EffectType.ChangeProperty;
        entity.Effect = effect;

        Assert.Equal(1, listener.AddedCount);

        manager.Effects.RemoveListener(listener);

        effect.DurationAmount = "10";

        // Listener was removed, so no property change should be detected
        Assert.Equal(0, listener.PropertyChangedCount);

        entity.Effect = null;

        // Listener was removed, so no removal should be detected
        Assert.Equal(0, listener.RemovedCount);
    }

    private class CountingListener : IEntityChangeListener<GameEntity>
    {
        public int AddedCount
        {
            get; private set;
        }
        public int RemovedCount
        {
            get; private set;
        }
        public int PropertyChangedCount
        {
            get; private set;
        }

        public void OnEntityAdded(in EntityChange<GameEntity> entityChange) => AddedCount++;
        public void OnEntityRemoved(in EntityChange<GameEntity> entityChange) => RemovedCount++;
        public void OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange) => PropertyChangedCount++;
    }
}
