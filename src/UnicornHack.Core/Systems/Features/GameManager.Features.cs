using UnicornHack.Systems.Features;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> Features { get; private set; } = null!;

    public LookupEntityRelationship<GameEntity, Point, SortedDictionary<Point, GameEntity>>
        FeaturesToRoomCellRelationship { get; private set; } = null!;

    private void InitializeFeatures(SequentialMessageQueue<GameEntity> queue)
    {
        Add<FeatureComponent>(EntityComponent.Feature, poolSize: 32, maxPoolSize: 64);
        Features = CreateGroup(nameof(Features),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Feature, (int)EntityComponent.Position), capacity: 64);
        FeaturesToRoomCellRelationship = new(
            nameof(FeaturesToRoomCellRelationship),
            Features,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            CreateCellKeyValueGetter(),
            (featureEntity, _) => featureEntity.RemoveComponent(EntityComponent.Position),
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.Features,
            keepDependentAlive: true);
    }
}