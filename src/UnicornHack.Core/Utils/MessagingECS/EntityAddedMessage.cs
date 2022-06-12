namespace UnicornHack.Utils.MessagingECS
{
    public class EntityAddedMessage<TEntity> : IMessage
        where TEntity : Entity, new()
    {
        private TEntity _entity;
        private Component _removedComponent;
        private IPropertyValueChanges _propertyChanges;
        private TEntity _principalEntity;

        public TEntity Entity
        {
            get => _entity;
            set
            {
                _entity?.RemoveReference(this);
                _entity = value;
                _entity?.AddReference(this);
            }
        }

        public Component RemovedComponent
        {
            get => _removedComponent;
            set
            {
                (_removedComponent as IOwnerReferenceable)?.RemoveReference(this);
                _removedComponent = value;
                (_removedComponent as IOwnerReferenceable)?.AddReference(this);
            }
        }

        public IPropertyValueChanges PropertyChanges
        {
            get => _propertyChanges;
            set
            {
                if (_propertyChanges != null)
                {
                    for (var i = 0; i < _propertyChanges.Count; i++)
                    {
                        _propertyChanges.GetChange<IOwnerReferenceable>(i)?.RemoveReference(this);
                    }
                }

                _propertyChanges = value;

                if (_propertyChanges != null)
                {
                    for (var i = 0; i < _propertyChanges.Count; i++)
                    {
                        _propertyChanges.GetChange<IOwnerReferenceable>(i)?.AddReference(this);
                    }
                }
            }
        }

        public TEntity PrincipalEntity
        {
            get => _principalEntity;
            set
            {
                _principalEntity?.RemoveReference(this);
                _principalEntity = value;
                _principalEntity?.AddReference(this);
            }
        }

        public IEntityGroup<TEntity> Group { get; set; }

        string IMessage.MessageName { get; set; }

        public void Clean()
        {
            RemovedComponent = default;
            PropertyChanges = default;
            Entity = default;
            Group = default;
            PrincipalEntity = default;
        }
    }
}
