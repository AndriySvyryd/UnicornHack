namespace UnicornHack.Utils.MessagingECS;

public enum EntityCollectionChangeAction
{
    None,
    Add,
    Remove
}

public readonly record struct EntityCollectionChange(
    EntityCollectionChangeAction Action,
    object? Item);
