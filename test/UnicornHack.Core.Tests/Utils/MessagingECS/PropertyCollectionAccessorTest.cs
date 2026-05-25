namespace UnicornHack.Utils.MessagingECS;

public class PropertyCollectionAccessorTest
{
    [Fact]
    public void Can_get_and_set_dependent()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;

        manager.Queue.ProcessQueue(manager);

        var accessor =
            new PropertyCollectionAccessor<GameEntity, Dictionary<short, DirectionFlags>, KeyValuePair<short, DirectionFlags>>(e
                => e.Room!.NeighborChanges);
        accessor.SetDefaultFactory(() => new Dictionary<short, DirectionFlags>());

        Assert.Null(accessor.GetDependents(level.Entity));
        Assert.Null(level.NeighborChanges);

        Assert.Empty(accessor.GetOrCreateDependents(level.Entity));
        Assert.Empty(level.NeighborChanges!);
    }
}
