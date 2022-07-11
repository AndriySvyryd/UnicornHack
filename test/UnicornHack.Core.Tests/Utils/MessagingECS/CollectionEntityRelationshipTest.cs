using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Senses;

namespace UnicornHack.Utils.MessagingECS;

public class CollectionEntityRelationshipTest
{
    [Fact]
    public void Relationship_is_updated()
    {
        var manager = TestHelper.CreateGameManager();
        using (var abilityEntityReference = manager.CreateEntity())
        {
            var abilityEntity = abilityEntityReference.Referenced;
            abilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);

            Assert.Empty(manager.EffectsToContainingAbilityRelationship.GetDependents(abilityEntity));
            Assert.Empty(abilityEntity.Ability!.Effects);
            Assert.Null(abilityEntity.Effect);

            using (var effectEntityReference = manager.CreateEntity())
            {
                var effectEntity = effectEntityReference.Referenced;
                var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                effect.ContainingAbilityId = abilityEntity.Id;

                Assert.Same(abilityEntity, effectEntity.Effect!.ContainingAbility);
                Assert.True(manager.EffectsToContainingAbilityRelationship.Dependents.ContainsEntity(effectEntity.Id));
                Assert.Same(effectEntity,
                    manager.EffectsToContainingAbilityRelationship.Dependents.FindEntity(effectEntity.Id));
                Assert.Same(abilityEntity, manager.EffectsToContainingAbilityRelationship.GetPrincipal(effectEntity));
                Assert.Same(effectEntity,
                    manager.EffectsToContainingAbilityRelationship.GetDependents(abilityEntity).Single());
                Assert.Same(effectEntity, abilityEntity.Ability.Effects.Single());

                effectEntity.Effect.ContainingAbilityId = null;

                Assert.Null(effectEntity.Effect.ContainingAbility);
                Assert.False(manager.EffectsToContainingAbilityRelationship.Dependents.ContainsEntity(effectEntity.Id));
                Assert.Null(manager.EffectsToContainingAbilityRelationship.Dependents.FindEntity(effectEntity.Id));
                Assert.Null(manager.EffectsToContainingAbilityRelationship.GetPrincipal(effectEntity));
                Assert.Empty(manager.EffectsToContainingAbilityRelationship.GetDependents(abilityEntity));
                Assert.Empty(abilityEntity.Ability.Effects);
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

            Assert.Equal(2, manager.Effects.Count);
        }

        Assert.Equal(0, manager.Abilities.Count);
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

        Assert.True(manager.IsLoading);

        using (var effectEntityReference = manager.CreateEntity())
        {
            var effectEntity = effectEntityReference.Referenced;
            var effect = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
            effect.ContainingAbilityId = 2;

            using (var abilityEntityReference = manager.CreateEntity())
            {
                var abilityEntity = abilityEntityReference.Referenced;
                abilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);

                Assert.Equal(2, abilityEntity.Id);
                Assert.Same(effectEntity, abilityEntity.Ability!.Effects.Single());
            }
        }

        manager.IsLoading = false;

        Assert.Equal(1, manager.Queue.QueuedCount);
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

        Assert.Equal(1, manager.Queue.QueuedCount);
    }

    [Fact]
    public void Collections_initialized_to_same_value()
    {
        var manager = TestHelper.CreateGameManager();
        using (var containerEntityReference = manager.CreateEntity())
        {
            var containerEntity = containerEntityReference.Referenced;
            var being = containerEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            var physical = containerEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
            var item = containerEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
            var sensor = containerEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

            Assert.Same(sensor.Items, item.Items);
            Assert.Same(sensor.Items, physical.Items);
            Assert.Same(physical.Items, being.Items);

            Assert.Same(sensor.Abilities, item.Abilities);
            Assert.Same(sensor.Abilities, physical.Abilities);
            Assert.Same(physical.Abilities, being.Abilities);
        }
    }

    [Fact]
    public void Queues_messages_for_registered_events()
    {
        var manager = TestHelper.CreateGameManager();

        Assert.Equal(nameof(manager.EffectsToContainingAbilityRelationship),
            manager.EffectsToContainingAbilityRelationship.Name);

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

                testSystem = new RelationshipTestSystem(effectEntity, effect,
                    manager.EffectsToContainingAbilityRelationship.Dependents);

                manager.Queue.Register<EntityAddedMessage<GameEntity>>(
                    testSystem, manager.EffectsToContainingAbilityRelationship.Dependents.GetEntityAddedMessageName(),
                    10);
                manager.Queue.Register<EntityRemovedMessage<GameEntity>>(
                    testSystem, manager.EffectsToContainingAbilityRelationship.Dependents.GetEntityRemovedMessageName(),
                    10);
                manager.Queue.Register<PropertyValueChangedMessage<GameEntity, string>>(
                    testSystem, manager.EffectsToContainingAbilityRelationship.Dependents
                        .GetPropertyValueChangedMessageName(
                            nameof(EffectComponent.DurationAmount)), 10);
                manager.EffectsToContainingAbilityRelationship.Dependents.AddListener(testSystem);

                effect.ContainingAbilityId = abilityEntity.Id;

                Assert.Equal(1, manager.Queue.QueuedCount);

                effect.DurationAmount = "10";

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
        IGameSystem<PropertyValueChangedMessage<GameEntity, string>>,
        IEntityChangeListener<GameEntity>
    {
        private readonly GameEntity _testEntity;
        private readonly GameComponent _testComponent;
        private readonly IEntityGroup<GameEntity> _group;

        public RelationshipTestSystem(GameEntity testEntity, GameComponent testComponent,
            IEntityGroup<GameEntity> group)
        {
            _testEntity = testEntity;
            _testComponent = testComponent;
            _group = group;
        }

        public int MessagesProcessed
        {
            get;
            private set;
        }

        public int GroupChangesDetected
        {
            get;
            private set;
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Same(_group, message.Group);
            Assert.Null(message.RemovedComponent);
            Assert.Equal(1, message.PropertyChanges.Count);
            Assert.Same(_testComponent, message.PropertyChanges.GetChangedComponent(0));

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Same(_group, message.Group);
            Assert.Null(message.RemovedComponent);
            Assert.Equal(0, message.PropertyChanges.Count);

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

        public void OnEntityAdded(in EntityChange<GameEntity> entityChange)
        {
            Assert.Same(_testEntity, entityChange.Entity);
            Assert.Null(entityChange.RemovedComponent);
            Assert.Equal(1, entityChange.PropertyChanges.Count);
            Assert.Same(_testComponent, entityChange.PropertyChanges.GetChangedComponent(0));

            GroupChangesDetected++;
        }

        public void OnEntityRemoved(in EntityChange<GameEntity> entityChange)
        {
            Assert.Same(_testEntity, entityChange.Entity);
            Assert.Null(entityChange.RemovedComponent);
            Assert.Equal(0, entityChange.PropertyChanges.Count);

            GroupChangesDetected++;
        }

        public bool OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
        {
            var changes = entityChange.PropertyChanges;

            Assert.Same(_testEntity, entityChange.Entity);
            Assert.Equal(1, entityChange.PropertyChanges.Count);
            Assert.Equal(_testComponent.ComponentId, changes.GetChangedComponent(0).ComponentId);
            Assert.Same(_testComponent, changes.GetChangedComponent(0));
            Assert.Equal(nameof(EffectComponent.DurationAmount), changes.GetChangedPropertyName(0));
            Assert.Null(changes.GetValue<string>(0, ValueType.Old));
            Assert.Equal("10", changes.GetValue<string>(0, ValueType.Current));

            GroupChangesDetected++;

            return false;
        }
    }
}
