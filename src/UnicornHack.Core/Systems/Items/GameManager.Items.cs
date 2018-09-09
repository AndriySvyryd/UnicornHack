using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> LevelItems { get; private set; }
        public EntityGroup<GameEntity> EntityItems { get; private set; }
        public EntityRelationship<GameEntity> EntityItemsToContainerRelationship { get; private set; }
        public UniqueEntityIndex<GameEntity, (int, byte, byte)> LevelItemsToLevelCellIndex { get; private set; }
        public EntityRelationship<GameEntity> LevelItemsToLevelRelationship { get; private set; }
        public ItemMovingSystem ItemMovingSystem { get; private set; }
        public ItemUsageSystem ItemUsageSystem { get; private set; }

        private void InitializeItems(SequentialMessageQueue<GameManager> queue)
        {
            Add<ItemComponent>(EntityComponent.Item, poolSize: 32);

            EntityItems = CreateGroup(nameof(EntityItems),
                new EntityMatcher<GameEntity>()
                    .AllOf((int)EntityComponent.Item, (int)EntityComponent.Physical)
                    .NoneOf((int)EntityComponent.Position));

            LevelItems = CreateGroup(nameof(LevelItems),
                new EntityMatcher<GameEntity>()
                    .AllOf((int)EntityComponent.Item, (int)EntityComponent.Physical, (int)EntityComponent.Position));

            LevelItemsToLevelCellIndex = new UniqueEntityIndex<GameEntity, (int, byte, byte)>(
                LevelItems,
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

            LevelItemsToLevelRelationship = new EntityRelationship<GameEntity>(
                nameof(LevelItemsToLevelRelationship),
                LevelItems,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                (effectEntity, _, __, ___) => effectEntity.RemoveComponent(EntityComponent.Position));

            EntityItemsToContainerRelationship = new EntityRelationship<GameEntity>(
                nameof(EntityItemsToContainerRelationship),
                EntityItems,
                AffectableEntities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((ItemComponent)component).ContainerId,
                    (int)EntityComponent.Item),
                (effectEntity, _, __, ___) => effectEntity.RemoveComponent((int)EntityComponent.Item),
                referencedKeepAlive: false,
                referencingKeepAlive: true);

            ItemMovingSystem = new ItemMovingSystem();
            queue.Add<MoveItemMessage>(ItemMovingSystem, ItemMovingSystem.MoveItemMessageName, 0);
            queue.Add<TraveledMessage>(ItemMovingSystem, TravelSystem.TraveledMessageName, 0);
            queue.Add<DiedMessage>(ItemMovingSystem, LivingSystem.DiedMessageName, 0);

            ItemUsageSystem = new ItemUsageSystem();
            queue.Add<EquipItemMessage>(ItemUsageSystem, ItemUsageSystem.EquipItemMessageName, 0);
            queue.Add<ActivateItemMessage>(ItemUsageSystem, ItemUsageSystem.ActivateItemMessageName, 0);
        }
    }
}
