using Microsoft.EntityFrameworkCore.Diagnostics;
using UnicornHack.Utils.Caching;

namespace UnicornHack.Data;

public sealed class PooledMaterializationInterceptor : IMaterializationInterceptor
{
    public static PooledMaterializationInterceptor Instance { get; } = new();

    private PooledMaterializationInterceptor()
    {
    }

    public InterceptionResult<object> CreatingInstance(
        MaterializationInterceptionData materializationData,
        InterceptionResult<object> result)
    {
        var type = materializationData.EntityType.ClrType;
        if (!typeof(IPoolable).IsAssignableFrom(type))
        {
            return result;
        }

        if (materializationData.Context is not GameDbContext { Manager: { } manager })
        {
            throw new InvalidOperationException(
                $"A game manager must be assigned before querying poolable type {type.Name}.");
        }

        var instance = manager.Rent(type)
                       ?? throw new InvalidOperationException(
                           $"No object pool is registered for queried type {type.Name}.");
        return InterceptionResult<object>.SuppressWithResult(instance);
    }
}