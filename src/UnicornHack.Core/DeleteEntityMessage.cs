using UnicornHack.Utils.MessagingECS;

namespace UnicornHack;

/// <summary>
///     Requests the permanent deletion of an entity. Processed by <see cref="GameManager" />, which
///     removes every component the entity still has so that — once the resulting cascade drains —
///     the entity and everything kept alive solely by it are torn down (and, for a database-backed
///     game, their rows are deleted).
///     <para>
///         Enqueued at low priority so the teardown runs after the current cascade settles rather
///         than re-entrantly. In particular, a dying actor's inventory is dropped to the floor (by
///         <c>ItemMovingSystem</c>) before this message tears the actor down, so its loot survives
///         as floor items; effects the deleted entity applied to <em>other</em> entities are not
///         removed either — they merely lose their source link.
///     </para>
///     <para>
///         The message holds a reference to its target, keeping it alive until processed, so a
///         queued deletion survives any intervening teardown (e.g. an abandoned room being removed)
///         and is then completed.
///     </para>
/// </summary>
public class DeleteEntityMessage : IMessage
{
    public const string Name = "DeleteEntity";

    public static void Enqueue(GameEntity entity, GameManager manager)
    {
        var message = manager.Queue.CreateMessage<DeleteEntityMessage>(Name);
        message.Entity = entity;

        manager.Enqueue(message, lowPriority: true);
    }

    private GameEntity? _entity;

    public GameEntity Entity
    {
        get => _entity!;
        set
        {
            _entity?.RemoveReference(this);
            _entity = value;
            _entity?.AddReference(this);
        }
    }

    string IMessage.MessageName
    {
        get;
        set;
    } = null!;

    public void Clean() => Entity = default!;
}
