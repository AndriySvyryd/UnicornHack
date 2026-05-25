using UnicornHack.Utils;

namespace UnicornHack.Systems.Levels;

/// <summary>
///     Persistable entry for a <c>Dictionary&lt;TKey, int&gt;</c> instance count on
///     <see cref="GenerationStats" />. Mapped as an owned collection so the keys/values
///     live in their own relational table instead of a JSON blob. Each closed generic
///     instantiation (e.g. <c>KeyCountPair&lt;string&gt;</c>,
///     <c>KeyCountPair&lt;RoomFeatures&gt;</c>) gets its own owned-type identity in
///     EF Core, and each navigation that uses it gets its own table.
/// </summary>
public sealed class KeyCountPair<TKey> : NotificationEntity
    where TKey : notnull
{
    private TKey _key = default!;
    private int _count;

    public TKey Key
    {
        get => _key;
        set => SetWithNotify(value, ref _key);
    }

    public int Count
    {
        get => _count;
        set => SetWithNotify(value, ref _count);
    }
}
