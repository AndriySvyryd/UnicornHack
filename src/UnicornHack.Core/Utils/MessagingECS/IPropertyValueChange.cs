namespace UnicornHack.Utils.MessagingECS
{
    public interface IPropertyValueChange
    {
        Component ChangedComponent { get; }
        string ChangedPropertyName { get; }

        void EnqueuePropertyValueChangedMessage<TEntity>(string messageName, IEntityManager manager)
            where TEntity : Entity, new();
    }
}
