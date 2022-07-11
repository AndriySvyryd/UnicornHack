namespace UnicornHack.Utils.MessagingECS;

public class PropertyValueChangedMessage<TEntity, T> : IMessage
    where TEntity : Entity
{
    private TEntity? _entity;
    private Component? _changedComponent;

    public TEntity Entity
    {
        get => _entity!;
        set
        {
            _entity?.RemoveReference(this);
            _entity = value;
            _entity?.AddReference(this);
        }
    }

    public Component ChangedComponent
    {
        get => _changedComponent!;
        set
        {
            (_changedComponent as IReferenceable)?.RemoveReference(this);
            _changedComponent = value;
            (_changedComponent as IReferenceable)?.AddReference(this);
        }
    }

    public string ChangedPropertyName
    {
        get;
        set;
    } = null!;

    public T OldValue
    {
        get;
        set;
    } = default!;

    public T NewValue
    {
        get;
        set;
    } = default!;

    string IMessage.MessageName
    {
        get;
        set;
    } = null!;

    public void Clean()
    {
        ChangedPropertyName = default!;
        OldValue = default!;
        NewValue = default!;
        ChangedComponent = default!;
        Entity = default!;
    }
}
