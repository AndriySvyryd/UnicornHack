namespace UnicornHack.Utils.MessagingECS
{
    public class EntityAddedMessage<TEntity> : IMessage
        where TEntity : Entity
    {
        private TEntity _entity;
        private Component _changedComponent;

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

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            Entity = default;
            ChangedComponent = default;
        }
    }
}
