using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class EntityRelationshipTest
    {
        [Fact]
        public void Relationship_is_updated()
        {
            var manager = TestHelper.CreateGameManager();
            using (var abilityEntityReference = manager.CreateEntity())
            {
                var abilityEntity = abilityEntityReference.Referenced;
                abilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);

                Assert.Empty(manager.EffectsToContainingAbilityRelationship[abilityEntity.Id]);

                GameEntity effectEntity;
                using (var effectEntityReference = manager.CreateEntity())
                {
                    effectEntity = effectEntityReference.Referenced;
                    var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                    effect.ContainingAbilityId = abilityEntity.Id;

                    Assert.Equal(1, manager.EffectsToContainingAbilityRelationship.Count);
                    Assert.True(manager.EffectsToContainingAbilityRelationship.ContainsEntity(effectEntity.Id));
                    Assert.Same(effectEntity, manager.EffectsToContainingAbilityRelationship.FindEntity(effectEntity.Id));
                    Assert.Same(effectEntity, manager.EffectsToContainingAbilityRelationship.Single());
                    Assert.Same(effectEntity, manager.EffectsToContainingAbilityRelationship[abilityEntity.Id].Single());

                    effectEntity.Effect.ContainingAbilityId = null;

                    Assert.Equal(0, manager.EffectsToContainingAbilityRelationship.Count);
                    Assert.False(manager.EffectsToContainingAbilityRelationship.ContainsEntity(effectEntity.Id));
                    Assert.Null(manager.EffectsToContainingAbilityRelationship.FindEntity(effectEntity.Id));
                    Assert.Empty(manager.EffectsToContainingAbilityRelationship);
                    Assert.Empty(manager.EffectsToContainingAbilityRelationship[abilityEntity.Id]);
                }
            }

            Assert.Equal(1, manager.Queue.QueuedCount);
        }

        [Fact]
        public void Referencing_entity_is_kept_alive()
        {
            var manager = TestHelper.CreateGameManager();
            using (var abilityEntityReference = manager.CreateEntity())
            {
                var abilityEntity = abilityEntityReference.Referenced;
                abilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);

                using (var effectEntityReference = manager.CreateEntity())
                {
                    var effectEntity = effectEntityReference.Referenced;
                    var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                    effect.ContainingAbilityId = abilityEntity.Id;
                }

                using (var effectEntityReference = manager.CreateEntity())
                {
                    var effectEntity = effectEntityReference.Referenced;
                    var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                    effect.ContainingAbilityId = abilityEntity.Id;
                }

                Assert.Equal(2, manager.EffectsToContainingAbilityRelationship.Count);
                Assert.Equal(2, manager.Effects.Count);
            }

            Assert.Equal(0, manager.Abilities.Count);
            Assert.Equal(0, manager.EffectsToContainingAbilityRelationship.Count);
            Assert.Equal(0, manager.Effects.Count);
        }

        [Fact]
        public void Referenced_entity_not_present_throws()
        {
            var manager = TestHelper.CreateGameManager();
            using (var effectEntityReference = manager.CreateEntity())
            {
                var effectEntity = effectEntityReference.Referenced;
                var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

                Assert.Throws<InvalidOperationException>(() => effect.ContainingAbilityId = 1);
            }
        }

        [Fact]
        public void Referenced_entity_not_present_is_found_when_loaded()
        {
            var manager = TestHelper.CreateGameManager();
            manager.IsLoading = true;

            Assert.True(manager.Abilities.IsLoading);

            GameEntity effectEntity;
            using (var effectEntityReference = manager.CreateEntity())
            {
                effectEntity = effectEntityReference.Referenced;
                var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                effect.ContainingAbilityId = 2;

                using (var abilityEntityReference = manager.CreateEntity())
                {
                    var abilityEntity = abilityEntityReference.Referenced;
                    abilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);

                    Assert.Equal(2, abilityEntity.Id);
                    Assert.Same(effectEntity, manager.EffectsToContainingAbilityRelationship[abilityEntity.Id].Single());
                }
            }

            manager.IsLoading = false;
        }

        [Fact]
        public void Referenced_entity_not_loaded_referencing_is_not_kept_alive()
        {
            var manager = TestHelper.CreateGameManager();
            manager.IsLoading = true;

            using (var effectEntityReference = manager.CreateEntity())
            {
                var effectEntity = effectEntityReference.Referenced;
                var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                effect.ContainingAbilityId = 1;
            }

            Assert.Equal(0, manager.Effects.Count);

            manager.IsLoading = false;
        }

        [Fact]
        public void Queues_messages_for_registered_events()
        {
            var manager = TestHelper.CreateGameManager();

            Assert.Equal(nameof(manager.EffectsToContainingAbilityRelationship), manager.EffectsToContainingAbilityRelationship.Name);

            RelationshipTestSystem testSystem;
            using (var abilityEntityReference = manager.CreateEntity())
            {
                var abilityEntity = abilityEntityReference.Referenced;
                abilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);

                using (var effectEntityReference = ((IEntityManager)manager).CreateEntity())
                {
                    var effectEntity = (GameEntity)effectEntityReference.Referenced;
                    var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

                    manager.Queue.ProcessQueue(manager);

                    testSystem = new RelationshipTestSystem(effectEntity, effect, manager.EffectsToContainingAbilityRelationship);

                    manager.Queue.Add<EntityAddedMessage<GameEntity>>(
                        testSystem, manager.EffectsToContainingAbilityRelationship.GetEntityAddedMessageName(), 10);
                    manager.Queue.Add<EntityRemovedMessage<GameEntity>>(
                        testSystem, manager.EffectsToContainingAbilityRelationship.GetEntityRemovedMessageName(), 10);
                    manager.Queue.Add<PropertyValueChangedMessage<GameEntity, int>>(
                        testSystem, manager.EffectsToContainingAbilityRelationship.GetPropertyValueChangedMessageName(
                            nameof(EffectComponent.DurationAmount)), 10);
                    manager.EffectsToContainingAbilityRelationship.AddListener(testSystem);

                    effect.ContainingAbilityId = abilityEntity.Id;

                    Assert.Equal(1, manager.Queue.QueuedCount);

                    effect.DurationAmount = 10;

                    Assert.Equal(2, manager.Queue.QueuedCount);
                }
            }

            Assert.Equal(2, manager.Queue.QueuedCount);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(3, testSystem.MessagesProcessed);
            Assert.Equal(3, testSystem.GroupChangesDetected);
        }

        private class RelationshipTestSystem :
            IGameSystem<EntityAddedMessage<GameEntity>>,
            IGameSystem<EntityRemovedMessage<GameEntity>>,
            IGameSystem<PropertyValueChangedMessage<GameEntity, int>>,
            IGroupChangesListener<GameEntity>
        {
            private readonly GameEntity _testEntity;
            private readonly GameComponent _testComponent;
            private readonly IEntityGroup<GameEntity> _group;

            public RelationshipTestSystem(GameEntity testEntity, GameComponent testComponent, IEntityGroup<GameEntity> group)
            {
                _testEntity = testEntity;
                _testComponent = testComponent;
                _group = group;
            }

            public int MessagesProcessed { get; private set; }
            public int GroupChangesDetected { get; private set; }

            public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager state)
            {
                Assert.Same(_testEntity, message.Entity);
                Assert.Same(_testComponent, message.ChangedComponent);

                MessagesProcessed++;
                return MessageProcessingResult.ContinueProcessing;
            }

            public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager state)
            {
                Assert.Same(_testEntity, message.Entity);

                MessagesProcessed++;
                return MessageProcessingResult.ContinueProcessing;
            }

            public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, int> message, GameManager state)
            {
                Assert.Same(_testEntity, message.Entity);
                Assert.Equal(_testComponent.ComponentId, message.ChangedComponent?.ComponentId);
                Assert.Same(_testComponent, message.ChangedComponent);
                Assert.Equal(nameof(EffectComponent.DurationAmount), message.ChangedPropertyName);
                Assert.Equal(0, message.OldValue);
                Assert.Equal(10, message.NewValue);

                MessagesProcessed++;
                return MessageProcessingResult.ContinueProcessing;
            }

            public void HandleEntityAdded(
                GameEntity entity, Component addedComponent, IEntityGroup<GameEntity> group)
            {
                Assert.Same(_testEntity, entity);
                Assert.Same(_testComponent, addedComponent);
                Assert.Same(_group, group);

                GroupChangesDetected++;
            }

            public void HandleEntityRemoved(
                GameEntity entity, Component removedComponent, IEntityGroup<GameEntity> group)
            {
                Assert.Same(_testEntity, entity);
                Assert.Same(_group, group);

                GroupChangesDetected++;
            }

            public bool HandlePropertyValueChanged<T>(
                string propertyName, T oldValue, T newValue, int componentId, Component component,
                GameEntity entity, IEntityGroup<GameEntity> group)
            {
                Assert.Same(_testEntity, entity);
                Assert.Equal(_testComponent.ComponentId, componentId);
                Assert.Same(_testComponent, component);
                Assert.Equal(nameof(EffectComponent.DurationAmount), propertyName);
                Assert.Equal(0, (int)(object)oldValue);
                Assert.Equal(10, (int)(object)newValue);
                Assert.Same(_group, group);

                GroupChangesDetected++;

                return false;
            }

            public bool HandlePropertyValuesChanged(
                IReadOnlyList<IPropertyValueChange> changes, GameEntity entity, IEntityGroup<GameEntity> group)
            {
                var change = (PropertyValueChange<int>)changes[0];

                Assert.Same(_testEntity, entity);
                Assert.Equal(_testComponent.ComponentId, change.ChangedComponent?.ComponentId);
                Assert.Same(_testComponent, change.ChangedComponent);
                Assert.Equal(nameof(EffectComponent.DurationAmount), change.ChangedPropertyName);
                Assert.Equal(0, change.OldValue);
                Assert.Equal(10, change.NewValue);
                Assert.Same(_group, group);

                GroupChangesDetected++;

                return false;
            }
        }
    }
}
