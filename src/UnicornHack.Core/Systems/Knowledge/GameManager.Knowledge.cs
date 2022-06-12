using System.Collections.Generic;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> KnownPositions { get; private set; }
        public LookupEntityRelationship<GameEntity, Point, Dictionary<Point, GameEntity>> KnownActorsToLevelCellRelationship { get; private set; }
        public LookupEntityRelationship<GameEntity, Point, Dictionary<Point, GameEntity>> KnownItemsToLevelCellRelationship { get; private set; }
        public LookupEntityRelationship<GameEntity, Point, Dictionary<Point, GameEntity>> KnownConnectionsToLevelCellRelationship { get; private set; }
        public UniqueEntityRelationship<GameEntity> LevelKnowledgeToLevelEntityRelationship { get; private set; }
        public UniqueEntityIndex<GameEntity, int> XPCooldownEntities { get; private set; }
        public KnowledgeSystem KnowledgeSystem { get; private set; }
        public XPSystem XPSystem { get; private set; }
        public LoggingSystem LoggingSystem { get; private set; }

        private void InitializeKnowledge(SequentialMessageQueue<GameManager> queue)
        {
            Add<KnowledgeComponent>(EntityComponent.Knowledge, poolSize: 32);

            KnownPositions = CreateGroup(nameof(KnownPositions),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Knowledge, (int)EntityComponent.Position));

            KnownActorsToLevelCellRelationship = new(
                nameof(KnownActorsToLevelCellRelationship),
                KnownPositions,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                new KeyValueGetter<GameEntity, Point>(
                    (change, matcher, valueType) =>
                    {
                        if (!matcher.TryGetValue<int>(
                                change, (int)EntityComponent.Knowledge, nameof(KnowledgeComponent.KnownEntityId), valueType,
                                out var knownEntityId)
                            || !change.Entity.Manager.LevelActors.ContainsEntity(knownEntityId)
                            || !matcher.TryGetValue<byte>(
                                change, (int)EntityComponent.Position, nameof(PositionComponent.LevelX), valueType,
                                out var levelX)
                            || !matcher.TryGetValue<byte>(
                                change, (int)EntityComponent.Position, nameof(PositionComponent.LevelY), valueType,
                                out var levelY))
                        {
                            return (new Point(0, 0), false);
                        }

                        return (new Point(levelX, levelY), true);
                    },
                    new PropertyMatcher<GameEntity>()
                        .With(component => ((KnowledgeComponent)component).KnownEntityId, (int)EntityComponent.Knowledge)
                        .With(component => ((PositionComponent)component).LevelId, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelX, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelY, (int)EntityComponent.Position)
                ),
                (knowledgeEntity, _) =>
                {
                    knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                    knowledgeEntity.RemoveComponent(EntityComponent.Position);
                },
                levelEntity => (Dictionary<Point, GameEntity>)levelEntity.Level.KnownActors);

            KnownItemsToLevelCellRelationship = new(
                nameof(KnownItemsToLevelCellRelationship),
                KnownPositions,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                new KeyValueGetter<GameEntity, Point>(
                    (change, matcher, valueType) =>
                    {
                        if (!matcher.TryGetValue<int>(
                                change, (int)EntityComponent.Knowledge, nameof(KnowledgeComponent.KnownEntityId), valueType,
                                out var knownEntityId)
                            || !change.Entity.Manager.LevelItems.ContainsEntity(knownEntityId)
                            || !matcher.TryGetValue<byte>(
                                change, (int)EntityComponent.Position, nameof(PositionComponent.LevelX), valueType,
                                out var levelX)
                            || !matcher.TryGetValue<byte>(
                                change, (int)EntityComponent.Position, nameof(PositionComponent.LevelY), valueType,
                                out var levelY))
                        {
                            return (new Point(0, 0), false);
                        }

                        return (new Point(levelX, levelY), true);
                    },
                    new PropertyMatcher<GameEntity>()
                        .With(component => ((KnowledgeComponent)component).KnownEntityId, (int)EntityComponent.Knowledge)
                        .With(component => ((PositionComponent)component).LevelId, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelX, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelY, (int)EntityComponent.Position)
                ),
                (knowledgeEntity, _) =>
                {
                    knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                    knowledgeEntity.RemoveComponent(EntityComponent.Position);
                },
                levelEntity => (Dictionary<Point, GameEntity>)levelEntity.Level.KnownItems);

            KnownConnectionsToLevelCellRelationship = new(
                nameof(KnownConnectionsToLevelCellRelationship),
                KnownPositions,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                new KeyValueGetter<GameEntity, Point>(
                    (change, matcher, valueType) =>
                    {
                        if (!matcher.TryGetValue<int>(
                                change, (int)EntityComponent.Knowledge, nameof(KnowledgeComponent.KnownEntityId), valueType,
                                out var knownEntityId)
                            || !change.Entity.Manager.Connections.ContainsEntity(knownEntityId)
                            || !matcher.TryGetValue<byte>(
                                change, (int)EntityComponent.Position, nameof(PositionComponent.LevelX), valueType,
                                out var levelX)
                            || !matcher.TryGetValue<byte>(
                                change, (int)EntityComponent.Position, nameof(PositionComponent.LevelY), valueType,
                                out var levelY))
                        {
                            return (new Point(0, 0), false);
                        }

                        return (new Point(levelX, levelY), true);
                    },
                    new PropertyMatcher<GameEntity>()
                        .With(component => ((KnowledgeComponent)component).KnownEntityId, (int)EntityComponent.Knowledge)
                        .With(component => ((PositionComponent)component).LevelId, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelX, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelY, (int)EntityComponent.Position)
                ),
                (knowledgeEntity, _) =>
                {
                    knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                    knowledgeEntity.RemoveComponent(EntityComponent.Position);
                },
                levelEntity => (Dictionary<Point, GameEntity>)levelEntity.Level.KnownConnections);

            LevelKnowledgeToLevelEntityRelationship = new (
                nameof(LevelKnowledgeToLevelEntityRelationship),
                KnownPositions,
                LevelEntities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((KnowledgeComponent)component).KnownEntityId,
                    (int)EntityComponent.Knowledge),
                (effectEntity, _) =>
                {
                    effectEntity.RemoveComponent(EntityComponent.Knowledge);
                    effectEntity.RemoveComponent(EntityComponent.Position);
                },
                levelEntity => levelEntity.Position.Knowledge);

            XPCooldownEntities = new (
                nameof(XPCooldownEntities),
                TemporalEntities,
                new KeyValueGetter<GameEntity, int>(
                    (change, matcher, valueType) =>
                    {
                        if (matcher.TryGetValue<int?>(
                                change, (int)EntityComponent.Ability, nameof(AbilityComponent.CooldownXpLeft), valueType,
                                out var abilityXp)
                            && abilityXp.HasValue)
                        {
                            return (change.Entity.Id, true);
                        }

                        if (matcher.TryGetValue<int?>(
                                change, (int)EntityComponent.Effect, nameof(EffectComponent.ExpirationXp), valueType,
                                out var effectXp)
                            && effectXp.HasValue)
                        {
                            return (change.Entity.Id, true);
                        }

                        return (0, false);
                    },
                    new PropertyMatcher<GameEntity>()
                        .With(component => ((EffectComponent)component).ExpirationXp, (int)EntityComponent.Effect)
                        .With(component => ((AbilityComponent)component).CooldownXpLeft, (int)EntityComponent.Ability)
                )
            );

            KnowledgeSystem = new KnowledgeSystem();
            queue.Register<VisibleTerrainChangedMessage>(KnowledgeSystem, VisibleTerrainChangedMessage.Name, 0);
            queue.Register<TraveledMessage>(KnowledgeSystem, TraveledMessage.Name, 4);
            queue.Register<ItemMovedMessage>(KnowledgeSystem, ItemMovedMessage.Name, 0);
            queue.Register<EffectsAppliedMessage>(KnowledgeSystem, EffectsAppliedMessage.Name, 1);

            XPSystem = new XPSystem();
            queue.Register<KnownTerrainChangedMessage>(XPSystem, KnownTerrainChangedMessage.Name, 1);
            queue.Register<DiedMessage>(XPSystem, DiedMessage.Name, 3);
            queue.Register<EntityAddedMessage<GameEntity>>(XPSystem, Races.GetEntityAddedMessageName(), 0);
            queue.Register<EntityRemovedMessage<GameEntity>>(XPSystem, Races.GetEntityRemovedMessageName(), 0);

            LoggingSystem = new LoggingSystem();
            queue.Register<ItemMovedMessage>(LoggingSystem, ItemMovedMessage.Name, 1);
            queue.Register<ItemEquippedMessage>(LoggingSystem, ItemEquippedMessage.Name, 1);
            queue.Register<EffectsAppliedMessage>(LoggingSystem, EffectsAppliedMessage.Name, 2);
            queue.Register<DiedMessage>(LoggingSystem, DiedMessage.Name, 1);
            queue.Register<LeveledUpMessage>(LoggingSystem, LeveledUpMessage.Name, 1);
        }
    }
}
