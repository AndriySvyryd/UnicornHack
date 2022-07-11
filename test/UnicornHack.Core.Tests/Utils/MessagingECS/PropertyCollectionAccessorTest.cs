namespace UnicornHack.Utils.MessagingECS;

public class PropertyCollectionAccessorTest
{
    [Fact]
    public void Can_get_and_set_dependent()
    {
        var level = TestHelper.BuildLevel(".");
        var manager = level.Entity.Manager;

        manager.Queue.ProcessQueue(manager);

        var accessor =
            new PropertyCollectionAccessor<GameEntity, Dictionary<int, byte>, KeyValuePair<int, byte>>(e
                => e.Level!.WallNeighborsChanges);
        accessor.SetDefaultFactory(() => new Dictionary<int, byte>());
        
        Assert.Null(accessor.GetDependents(level.Entity));
        Assert.Null(level.WallNeighborsChanges);

        Assert.Empty(accessor.GetOrCreateDependents(level.Entity));
        Assert.Empty(level.WallNeighborsChanges!);
    }
}
