// ReSharper disable StaticMemberInGenericType
namespace UnicornHack.Utils.MessagingECS;

public abstract class EntityGroupBase<TEntity> : IEntityGroup<TEntity>
    where TEntity : Entity, new()
{
    private List<IEntityChangeListener<TEntity>>? _changeListeners;
    private string? _entityAddedMessageName;
    private string? _entityRemovedMessageName;
    private Dictionary<string, string>? _propertyValueChangedMessageNames;

    private const string EntityAddedMessageSuffix = "_EntityAdded";
    private const string EntityRemovedMessageSuffix = "_EntityRemoved";
    private const string PropertyValueChangedMessageSuffix = "_PropertyValueChanged_";

    protected EntityGroupBase(string name)
    {
        Name = name;
    }

    public string Name
    {
        get;
    }

    public string GetEntityAddedMessageName()
        => _entityAddedMessageName ??= Name + EntityAddedMessageSuffix;

    public string GetEntityRemovedMessageName()
        => _entityRemovedMessageName ??= Name + EntityRemovedMessageSuffix;

    public string GetPropertyValueChangedMessageName(string propertyName)
    {
        _propertyValueChangedMessageNames ??= [];

        if (!_propertyValueChangedMessageNames.TryGetValue(propertyName, out var messageName))
        {
            messageName = Name + PropertyValueChangedMessageSuffix + propertyName;
            _propertyValueChangedMessageNames[propertyName] = messageName;
        }

        return messageName;
    }

    public void AddListener(IEntityChangeListener<TEntity> index)
    {
        _changeListeners ??= [];
        _changeListeners.Add(index);
    }

    public abstract TEntity? FindEntity(int id);
    public abstract bool ContainsEntity(int id);

    protected void OnAdded(in EntityChange<TEntity> entityChange, TEntity? principal)
    {
        if (_changeListeners != null)
        {
            foreach (var changeListener in _changeListeners)
            {
                changeListener.OnEntityAdded(entityChange);
            }
        }

        var entity = entityChange.Entity;
        var manager = entity.Manager;
        if (_entityAddedMessageName != null
            && manager is { IsLoading: false })
        {
            var message = manager.Queue.CreateMessage<EntityAddedMessage<TEntity>>(_entityAddedMessageName);
            message.Entity = entity;
            message.RemovedComponent = entityChange.RemovedComponent;
            message.PropertyChanges = entityChange.PropertyChanges;
            message.Group = this;
            message.PrincipalEntity = principal;

            manager.Queue.Enqueue(message);
        }
    }

    protected void OnRemoved(in EntityChange<TEntity> entityChange, TEntity? principal)
    {
        var entity = entityChange.Entity;
        var manager = entity.Manager;

        if (_entityRemovedMessageName != null
            && manager != null)
        {
            var message = manager.Queue.CreateMessage<EntityRemovedMessage<TEntity>>(_entityRemovedMessageName);
            message.Entity = entity;
            message.RemovedComponent = entityChange.RemovedComponent;
            message.PropertyChanges = entityChange.PropertyChanges;
            message.Group = this;
            if (principal?.Manager != null)
            {
                message.PrincipalEntity = principal;
            }

            manager.Queue.Enqueue(message);
        }

        if (_changeListeners != null)
        {
            foreach (var changeListener in _changeListeners)
            {
                changeListener.OnEntityRemoved(entityChange);
            }
        }
    }

    protected void OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        var entity = entityChange.Entity;
        if (_changeListeners != null)
        {
            foreach (var changeListener in _changeListeners)
            {
                changeListener.OnPropertyValuesChanged(entityChange);
            }
        }

        if (_propertyValueChangedMessageNames != null)
        {
            var changes = entityChange.PropertyChanges;
            for (var i = 0; i < changes.Count; i++)
            {
                if (_propertyValueChangedMessageNames.TryGetValue(changes.GetChangedPropertyName(i),
                        out var messageName)
                    && entity.HasComponent(changes.GetChangedComponent(i).ComponentId))
                {
                    changes.EnqueuePropertyValueChangedMessage<TEntity>(i, messageName);
                }
            }
        }
    }
}
