using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> LevelKnowledges { get; private set; }
        public EntityRelationship<GameEntity> LevelKnowledgesToLevelRelationship { get; private set; }
        public EntityIndex<GameEntity, (int, byte, byte)> LevelKnowledgeToLevelCellIndex { get; private set; }
        public UniqueEntityRelationship<GameEntity> LevelKnowledgeToLevelEntityRelationship { get; private set; }
        public KnowledgeSystem KnowledgeSystem { get; private set; }

        private void InitializeKnowledge(SequentialMessageQueue<GameManager> queue)
        {
            Add<KnowledgeComponent>(EntityComponent.Knowledge, poolSize: 32);

            LevelKnowledges = CreateGroup(nameof(LevelKnowledges),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Knowledge, (int)EntityComponent.Position));

            LevelKnowledgeToLevelCellIndex = new EntityIndex<GameEntity, (int, byte, byte)>(
                LevelKnowledges,
                new KeyValueGetter<GameEntity, (int, byte, byte)>(
                    (entity, changedComponentId, changedComponent, changedProperty, changedValue) =>
                    {
                        PositionComponent position;
                        if (changedComponentId == (int)EntityComponent.Position)
                        {
                            position = (PositionComponent)changedComponent;
                        }
                        else
                        {
                            position = entity.Position;
                        }

                        var levelId = position.LevelId;
                        var levelX = position.LevelX;
                        var levelY = position.LevelY;

                        switch (changedProperty)
                        {
                            case nameof(PositionComponent.LevelId):
                                levelId = (int)changedValue;
                                break;
                            case nameof(PositionComponent.LevelX):
                                levelX = (byte)changedValue;
                                break;
                            case nameof(PositionComponent.LevelY):
                                levelY = (byte)changedValue;
                                break;
                        }

                        return ((levelId, levelX, levelY), true);
                    },
                    new PropertyMatcher((int)EntityComponent.Position, nameof(PositionComponent.LevelId))
                        .With((int)EntityComponent.Position, nameof(PositionComponent.LevelX))
                        .With((int)EntityComponent.Position, nameof(PositionComponent.LevelY))
                ));

            LevelKnowledgesToLevelRelationship = new EntityRelationship<GameEntity>(
                nameof(LevelKnowledgesToLevelRelationship),
                LevelKnowledges,
                Levels,
                new SimpleNonNullableKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                (effectEntity, _, __, ___) =>
                {
                    effectEntity.RemoveComponent(EntityComponent.Knowledge);
                    effectEntity.RemoveComponent(EntityComponent.Position);
                });

            LevelKnowledgeToLevelEntityRelationship = new UniqueEntityRelationship<GameEntity>(
                nameof(LevelKnowledgeToLevelEntityRelationship),
                LevelKnowledges,
                LevelEntities,
                new SimpleNonNullableKeyValueGetter<GameEntity, int>(
                    component => ((KnowledgeComponent)component).KnownEntityId,
                    (int)EntityComponent.Knowledge),
                (effectEntity, _, __, ___) =>
                {
                    effectEntity.RemoveComponent(EntityComponent.Knowledge);
                    effectEntity.RemoveComponent(EntityComponent.Position);
                });

            KnowledgeSystem = new KnowledgeSystem();
            queue.Add<VisibleTerrainChangedMessage>(KnowledgeSystem, SensorySystem.VisibleTerrainChangedMessageName, 0);
            queue.Add<TraveledMessage>(KnowledgeSystem, TravelSystem.TraveledMessageName, 4);
            queue.Add<ItemMovedMessage>(KnowledgeSystem, ItemMovingSystem.ItemMovedMessageName, 0);
            queue.Add<ItemEquippedMessage>(KnowledgeSystem, ItemUsageSystem.ItemEquippedMessageName, 0);
            queue.Add<ItemActivatedMessage>(KnowledgeSystem, ItemUsageSystem.ItemActivatedMessageName, 0);
            queue.Add<DiedMessage>(KnowledgeSystem, LivingSystem.DiedMessageName, 1);
            queue.Add<EffectsAppliedMessage>(KnowledgeSystem, EffectApplicationSystem.EffectsAppliedMessageName, 1);
        }
    }
}
