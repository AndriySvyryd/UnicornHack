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
            new PropertyCollectionAccessor<GameEntity, Dictionary<short, byte>, KeyValuePair<short, byte>>(e
                => e.Level!.WallNeighborsChanges);
        accessor.SetDefaultFactory(() => new Dictionary<short, byte>());

        Assert.Null(accessor.GetDependents(level.Entity));
        Assert.Null(level.WallNeighborsChanges);

        Assert.Empty(accessor.GetOrCreateDependents(level.Entity));
        Assert.Empty(level.WallNeighborsChanges!);
    }
}
