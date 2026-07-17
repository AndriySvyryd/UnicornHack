namespace UnicornHack.Utils.MessagingECS;

public class EntityAddedMessage<TEntity> : IMessage
    where TEntity : Entity, new()
{
    private TEntity? _entity;
    private Component? _removedComponent;
    private TEntity? _principalEntity;
    private IPropertyValueChanges _propertyChanges = PropertyValueChanges.Instance;

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

    public Component? RemovedComponent
    {
        get => _removedComponent;
        set
        {
            (_removedComponent as IReferenceable)?.RemoveReference(this);
            _removedComponent = value;
            (_removedComponent as IReferenceable)?.AddReference(this);
        }
    }

    public IPropertyValueChanges PropertyChanges
    {
        get => _propertyChanges;
        set
        {
            _propertyChanges.Return();
            _propertyChanges = value.Clone();
        }
    }

    public TEntity? PrincipalEntity
    {
        get => _principalEntity;
        set
        {
            _principalEntity?.RemoveReference(this);
            _principalEntity = value;
            _principalEntity?.AddReference(this);
        }
    }

    public IEntityGroup<TEntity> Group
    {
        get;
        set;
    } = null!;

    string IMessage.MessageName
    {
        get;
        set;
    } = null!;

    public void Clean()
    {
        RemovedComponent = default;
        // Return the owned clone to the pool and reset to the shared empty singleton.
        _propertyChanges = PropertyValueChanges.Instance;
        Entity = default!;
        Group = default!;
        PrincipalEntity = default;
    }
}
