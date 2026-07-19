namespace UnicornHack.Utils.MessagingECS;

public readonly struct PropertyValueChange<T> : IPropertyValueChange
{
    public PropertyValueChange(
        Component changedComponent, string changedPropertyName, T oldValue, T newValue,
        object? nestedItem = null, string? nestedPropertyName = null)
    {
        ChangedComponent = changedComponent;
        ChangedPropertyName = changedPropertyName;
        OldValue = oldValue;
        NewValue = newValue;
        NestedItem = nestedItem;
        NestedPropertyName = nestedPropertyName;
    }

    public Component ChangedComponent
    {
        get;
    }

    public string ChangedPropertyName
    {
        get;
    }

    public T OldValue
    {
        get;
    }

    public T NewValue
    {
        get;
    }

    public object? NestedItem { get; }

    public string? NestedPropertyName { get; }

    public bool IsEmpty => ChangedComponent == null;

    public static readonly PropertyValueChange<T> Empty = new(null!, null!, default!, default!);

    public void EnqueuePropertyValueChangedMessage<TEntity>(string messageName)
        where TEntity : Entity, new()
    {
        var entity = (TEntity)ChangedComponent.Entity;
        var manager = entity.Manager;
        var message = manager.Queue.CreateMessage<PropertyValueChangedMessage<TEntity, T>>(messageName);
        message.ChangedPropertyName = ChangedPropertyName;
        message.NestedItem = NestedItem;
        message.NestedPropertyName = NestedPropertyName;
        message.OldValue = OldValue;
        message.NewValue = NewValue;
        message.ChangedComponent = ChangedComponent;
        message.Entity = entity;

        manager.Queue.Enqueue(message);
    }
}
