using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Senses;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class SortedEntityRelationshipTest
    {
        [Fact]
        public void Relationship_is_updated()
        {
            var manager = TestHelper.CreateGameManager();
            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                using (var firstRaceEntityReference = manager.CreateEntity())
                {
                    var firstRaceEntity = firstRaceEntityReference.Referenced;
                    var firstRace = firstRaceEntity.AddComponent<RaceComponent>((int)EntityComponent.Race);
                    var firstEffect = firstRaceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                    firstEffect.DurationTicks = -1;

                    Assert.Empty(manager.RacesToBeingRelationship[beingEntity.Id]);

                    firstEffect.AffectedEntityId = beingEntity.Id;

                    Assert.Equal(1, manager.RacesToBeingRelationship.Count);
                    Assert.True(manager.RacesToBeingRelationship.ContainsEntity(firstRaceEntity.Id));
                    Assert.Same(firstRaceEntity, manager.RacesToBeingRelationship.FindEntity(firstRaceEntity.Id));
                    Assert.Same(firstRaceEntity, manager.RacesToBeingRelationship.Single());
                    Assert.Same(firstRaceEntity, manager.RacesToBeingRelationship[beingEntity.Id].Single().Value);

                    using (var secondRaceEntityReference = manager.CreateEntity())
                    {
                        var secondRaceEntity = secondRaceEntityReference.Referenced;
                        secondRaceEntity.AddComponent<RaceComponent>((int)EntityComponent.Race);
                        var secondEffect = secondRaceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                        secondEffect.DurationTicks = -1;

                        secondEffect.AffectedEntityId = beingEntity.Id;

                        Assert.Equal(2, manager.RacesToBeingRelationship.Count);
                        Assert.True(manager.RacesToBeingRelationship.ContainsEntity(secondRaceEntity.Id));
                        Assert.Same(secondRaceEntity, manager.RacesToBeingRelationship.FindEntity(secondRaceEntity.Id));
                        Assert.Equal(new[] { firstRaceEntity, secondRaceEntity }, manager.RacesToBeingRelationship);
                        Assert.Equal(new[] { firstRaceEntity, secondRaceEntity }, manager.RacesToBeingRelationship[beingEntity.Id].Values);

                        firstRace.Level = 2;

                        Assert.Equal(new[] { secondRaceEntity, firstRaceEntity }, manager.RacesToBeingRelationship);
                        Assert.Equal(new[] { secondRaceEntity, firstRaceEntity }, manager.RacesToBeingRelationship[beingEntity.Id].Values);

                        firstEffect.AffectedEntityId = null;
                        secondEffect.AffectedEntityId = null;

                        Assert.Equal(0, manager.RacesToBeingRelationship.Count);
                        Assert.False(manager.RacesToBeingRelationship.ContainsEntity(firstRaceEntity.Id));
                        Assert.False(manager.RacesToBeingRelationship.ContainsEntity(secondRaceEntity.Id));
                        Assert.Null(manager.RacesToBeingRelationship.FindEntity(firstRaceEntity.Id));
                        Assert.Null(manager.RacesToBeingRelationship.FindEntity(secondRaceEntity.Id));
                        Assert.Empty(manager.RacesToBeingRelationship);
                        Assert.Empty(manager.RacesToBeingRelationship[beingEntity.Id]);
                    }
                }
            }

            Assert.Equal(6, manager.Queue.QueuedCount);
        }

        [Fact]
        public void Referenced_entity_not_present_throws()
        {
            var manager = TestHelper.CreateGameManager();

            using (var raceEntityReference = manager.CreateEntity())
            {
                var raceEntity = raceEntityReference.Referenced;
                raceEntity.AddComponent<RaceComponent>((int)EntityComponent.Race);
                var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

                Assert.Throws<InvalidOperationException>(() => effect.AffectedEntityId = 2);
            }
        }

        [Fact]
        public void Referenced_entity_not_present_is_found_when_loaded()
        {
            var manager = TestHelper.CreateGameManager();
            manager.IsLoading = true;

            Assert.True(manager.RacesToBeingRelationship.IsLoading);

            using (var raceEntityReference = manager.CreateEntity())
            {
                var raceEntity = raceEntityReference.Referenced;
                raceEntity.AddComponent<RaceComponent>((int)EntityComponent.Race);
                var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                effect.DurationTicks = -1;
                effect.AffectedEntityId = 2;

                using (var beingEntityReference = manager.CreateEntity())
                {
                    var beingEntity = beingEntityReference.Referenced;
                    var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                    beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                    beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                    Assert.Equal(2, beingEntity.Id);
                    Assert.Same(raceEntity, manager.RacesToBeingRelationship[beingEntity.Id].Single().Value);
                }
            }

            manager.IsLoading = false;

            Assert.Equal(1, manager.Queue.QueuedCount);
        }

        [Fact]
        public void Queues_messages_for_registered_events()
        {
            var manager = TestHelper.CreateGameManager();

            Assert.Equal(nameof(manager.RacesToBeingRelationship), manager.RacesToBeingRelationship.Name);

            RelationshipTestSystem testSystem;
            using (var beingEntityReference = ((IEntityManager)manager).CreateEntity())
            {
                var beingEntity = (GameEntity)beingEntityReference.Referenced;
                var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                using (var raceEntityReference = manager.CreateEntity())
                {
                    var raceEntity = raceEntityReference.Referenced;
                    var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                    effect.DurationTicks = -1;
                    effect.AffectedEntityId = beingEntity.Id;
                    var race = manager.CreateComponent<RaceComponent>((int)EntityComponent.Race);

                    manager.Queue.ProcessQueue(manager);

                    testSystem = new RelationshipTestSystem(raceEntity, race, manager.RacesToBeingRelationship);

                    manager.Queue.Add<EntityAddedMessage<GameEntity>>(
                        testSystem, manager.RacesToBeingRelationship.GetEntityAddedMessageName(), 10);
                    manager.Queue.Add<EntityRemovedMessage<GameEntity>>(
                        testSystem, manager.RacesToBeingRelationship.GetEntityRemovedMessageName(), 10);
                    manager.Queue.Add<PropertyValueChangedMessage<GameEntity, Species>>(
                        testSystem, manager.RacesToBeingRelationship.GetPropertyValueChangedMessageName(
                            nameof(RaceComponent.Species)), 10);
                    manager.RacesToBeingRelationship.AddListener(testSystem);

                    raceEntity.Race = race;

                    Assert.Equal(1, manager.Queue.QueuedCount);

                    race.Species = Species.Dragon;

                    Assert.Equal(2, manager.Queue.QueuedCount);
                }
            }

            Assert.Equal(0, manager.Beings.Count);
            Assert.Equal(0, manager.EntityItems.Count);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, manager.Queue.QueuedCount);
            Assert.Equal(3, testSystem.MessagesProcessed);
            Assert.Equal(3, testSystem.GroupChangesDetected);
        }

        private class RelationshipTestSystem :
            IGameSystem<EntityAddedMessage<GameEntity>>,
            IGameSystem<EntityRemovedMessage<GameEntity>>,
            IGameSystem<PropertyValueChangedMessage<GameEntity, Species>>,
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
                Assert.Same(_testComponent, message.ChangedComponent);

                MessagesProcessed++;
                return MessageProcessingResult.ContinueProcessing;
            }

            public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, Species> message, GameManager state)
            {
                Assert.Same(_testEntity, message.Entity);
                Assert.Equal(_testComponent.ComponentId, message.ChangedComponent?.ComponentId);
                Assert.Same(_testComponent, message.ChangedComponent);
                Assert.Equal(nameof(RaceComponent.Species), message.ChangedPropertyName);
                Assert.Equal(Species.Default, message.OldValue);
                Assert.Equal(Species.Dragon, message.NewValue);

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
                Assert.Same(_testComponent, removedComponent);
                Assert.Same(_group, group);

                GroupChangesDetected++;
            }

            public bool HandlePropertyValuesChanged(
                IReadOnlyList<IPropertyValueChange> changes, GameEntity entity, IEntityGroup<GameEntity> group)
            {
                var change = (PropertyValueChange<Species>)changes[0];

                Assert.Same(_testEntity, entity);
                Assert.Equal(_testComponent.ComponentId, change.ChangedComponent?.ComponentId);
                Assert.Same(_testComponent, change.ChangedComponent);
                Assert.Equal(nameof(RaceComponent.Species), change.ChangedPropertyName);
                Assert.Equal(Species.Default, change.OldValue);
                Assert.Equal(Species.Dragon, change.NewValue);
                Assert.Same(_group, group);

                GroupChangesDetected++;

                return false;
            }
        }
    }
}
