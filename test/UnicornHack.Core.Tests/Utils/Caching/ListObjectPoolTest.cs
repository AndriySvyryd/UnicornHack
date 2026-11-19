using System.Threading;
using UnicornHack.Utils.Caching;

namespace UnicornHack.Utils;

public class ListObjectPoolTest
{
    [Fact]
    public void Returns_from_another_thread_do_not_mutate_the_owner_pool()
    {
        var pool = new ListObjectPool<PooledItem>(static () => new PooledItem(), initialSize: 1, maxSize: 2, preallocatedCount: 0);
        var item = pool.Rent();
        Assert.Same(pool, item.Owner);
        PooledItem? other = null;
        var thread = new Thread(() =>
        {
            var threadPool = new ListObjectPool<PooledItem>(static () => new PooledItem(), 1, 2, 0);
            other = threadPool.Rent();
            item.Owner!.Return(item);
        });
        thread.Start();
        thread.Join();

        Assert.NotSame(item, other);
        Assert.Same(pool, item.Owner);
        Assert.NotSame(item, pool.Rent());
    }

    [Fact]
    public void Returned_objects_preserve_fifo_order_and_capacity()
    {
        var pool = new ListObjectPool<FifoItem>(static () => new FifoItem(), initialSize: 1, maxSize: 2, preallocatedCount: 0);
        var first = pool.Rent();
        var second = pool.Rent();
        var surplus = pool.Rent();
        pool.Return(first);
        pool.Return(second);
        pool.Return(surplus);

        Assert.Same(first, pool.Rent());
        Assert.Same(second, pool.Rent());
        Assert.NotSame(surplus, pool.Rent());
    }

    [Fact]
    public void Different_retention_settings_do_not_share_storage()
    {
        var small = new ListObjectPool<SettingsItem>(static () => new SettingsItem(), initialSize: 1, maxSize: 1, preallocatedCount: 0);
        var large = new ListObjectPool<SettingsItem>(static () => new SettingsItem(), initialSize: 1, maxSize: 2, preallocatedCount: 0);
        var item = small.Rent();
        small.Return(item);

        Assert.NotSame(small, large);
        Assert.NotSame(item, large.Rent());
        Assert.Same(item, small.Rent());
    }

    [Fact]
    public void Manager_scratch_pools_are_shared_only_within_the_current_thread()
    {
        var manager = TestHelper.CreateGameManager();
        var pool = manager.PointByteListArrayPool;
        Assert.Same(pool, TestHelper.CreateGameManager().PointByteListArrayPool);
        object? otherThreadPool = null;
        var thread = new Thread(() => otherThreadPool = manager.PointByteListArrayPool);
        thread.Start();
        thread.Join();

        Assert.NotSame(pool, otherThreadPool);
        Assert.Same(pool, manager.PointByteListArrayPool);
    }

    public sealed class PooledItem : IPoolable
    {
        public IObjectPool? Owner { get; private set; }

        public void SetPool(IObjectPool pool)
        {
            Assert.True(Owner == null || ReferenceEquals(Owner, pool));
            Owner = pool;
        }
    }

    public sealed class FifoItem;

    public sealed class SettingsItem;
}