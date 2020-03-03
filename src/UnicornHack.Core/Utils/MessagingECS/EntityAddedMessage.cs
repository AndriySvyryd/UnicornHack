namespace UnicornHack.Utils.MessagingECS
{
    public class EntityAddedMessage<TEntity> : IMessage
        where TEntity : Entity
    {
        private TEntity _entity;
        private Component _changedComponent;
        private TEntity _referencedEntity;

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

        public Component ChangedComponent
        {
            get => _changedComponent;
            set
            {
                (_changedComponent as IOwnerReferenceable)?.RemoveReference(this);
                _changedComponent = value;
                (_changedComponent as IOwnerReferenceable)?.AddReference(this);
            }
        }

        public TEntity ReferencedEntity
        {
            get => _referencedEntity;
            set
            {
                _referencedEntity?.RemoveReference(this);
                _referencedEntity = value;
                _referencedEntity?.AddReference(this);
            }
        }

        public IEntityGroup<TEntity> Group { get; set; }

        string IMessage.MessageName { get; set; }

        public void Clean()
        {
            ChangedComponent = default;
            Entity = default;
            Group = default;
            ReferencedEntity = default;
        }
    }
}
