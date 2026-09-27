using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> RoomItems
    {
        get;
        private set;
    } = null!;

    public LookupEntityRelationship<GameEntity, Point, SortedDictionary<Point, GameEntity>>
        RoomItemsToRoomCellRelationship
    {
        get;
        private set;
    } = null!;

    public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> EntityItemsToContainerRelationship
    {
        get;
        private set;
    } = null!;

    public ItemMovingSystem ItemMovingSystem
    {
        get;
        private set;
    } = null!;

    public ItemUsageSystem ItemUsageSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeItems(SequentialMessageQueue<GameEntity> queue)
    {
        Add<ItemComponent>(EntityComponent.Item, poolSize: 256, maxPoolSize: 512);

        RoomItems = CreateGroup(nameof(RoomItems),
            new EntityMatcher<GameEntity>()
                .AllOf((int)EntityComponent.Item, (int)EntityComponent.Physical, (int)EntityComponent.Position), capacity: 256);

        RoomItemsToRoomCellRelationship = new(
            nameof(RoomItemsToRoomCellRelationship),
            RoomItems,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            CreateCellKeyValueGetter(),
            (itemEntity, _) => itemEntity.RemoveComponent(EntityComponent.Position),
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.Items,
            keepDependentAlive: true);

        EntityItemsToContainerRelationship = new(
            nameof(EntityItemsToContainerRelationship),
            RoomItems,
            AffectableEntities,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((ItemComponent)component).ContainerId,
                (int)EntityComponent.Item),
            (effectEntity, _) => effectEntity.RemoveComponent((int)EntityComponent.Item),
            containerEntity => (HashSet<GameEntity>)(containerEntity.Being!.Items
                                                     ?? containerEntity.Item!.Items
                                                     ?? containerEntity.Physical!.Items),
            keepPrincipalAlive: false,
            keepDependentAlive: true);

        AvailableAbilitiesToRoomActorsRelationship = new(
            Abilities, RoomActors, EntityItemsToContainerRelationship);

        SlottedAbilitiesToRoomActorsRelationship = new(
            nameof(SlottedAbilitiesToRoomActorsRelationship),
            AvailableAbilitiesToRoomActorsRelationship,
            RoomActors,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((AbilityComponent)component).Slot,
                (int)EntityComponent.Ability),
            (abilityEntity, _) =>
            {
                if (abilityEntity.Ability?.OwnerEntity?.Item != null)
                {
                    abilityEntity.Ability.Slot = null;
                }
                else
                {
                    abilityEntity.RemoveComponent(EntityComponent.Ability);
                }
            },
            actorEntity => (SortedDictionary<int, GameEntity>)actorEntity.Being!.SlottedAbilities);

        ItemMovingSystem = new ItemMovingSystem();
        queue.Register<MoveItemMessage>(ItemMovingSystem, MoveItemMessage.Name, 0);
        queue.Register<TraveledMessage>(ItemMovingSystem, TraveledMessage.Name, 0);
        queue.Register<DiedMessage>(ItemMovingSystem, DiedMessage.Name, 0);

        ItemUsageSystem = new ItemUsageSystem();
        queue.Register(ItemUsageSystem, EquipItemMessage.Name, 0);

        queue.Register<EntityAddedMessage<GameEntity>>(
            AbilityActivationSystem, EntityItemsToContainerRelationship.Dependents.GetEntityAddedMessageName(), 0);
        queue.Register<EntityAddedMessage<GameEntity>>(
            AbilitySlottingSystem, EntityItemsToContainerRelationship.Dependents.GetEntityAddedMessageName(), 1);
        queue.Register<EntityRemovedMessage<GameEntity>>(
            AbilityActivationSystem, EntityItemsToContainerRelationship.Dependents.GetEntityRemovedMessageName(),
            0);
    }
}
