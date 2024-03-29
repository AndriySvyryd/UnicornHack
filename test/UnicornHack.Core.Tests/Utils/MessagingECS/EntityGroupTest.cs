﻿using UnicornHack.Systems.Effects;
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
            entity.AddComponent<LevelComponent>((int)EntityComponent.Level);

            Assert.Equal(1, manager.Levels.Count);
            Assert.True(manager.Levels.Matcher.Matches(entity));
            Assert.True(manager.Levels.ContainsEntity(entity.Id));
            Assert.Same(entity, manager.Levels.FindEntity(entity.Id));
            Assert.Same(entity, manager.Levels.Single());

            entity.AddComponent<ConnectionComponent>((int)EntityComponent.Connection);

            Assert.Equal(1, manager.Levels.Count);

            entity.RemoveComponent(EntityComponent.Level);

            Assert.Equal(0, manager.Levels.Count);
            Assert.False(manager.Levels.Matcher.Matches(entity));
            Assert.False(manager.Levels.ContainsEntity(entity.Id));
            Assert.Null(manager.Levels.FindEntity(entity.Id));
            Assert.Empty(manager.Levels);
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
            var effect = manager.CreateComponent<EffectComponent>((int)EntityComponent.Effect);

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
}
