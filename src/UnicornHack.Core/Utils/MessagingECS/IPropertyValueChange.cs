namespace UnicornHack.Utils.MessagingECS
{
    public interface IPropertyValueChange
    {
        Component ChangedComponent { get; }
        string ChangedPropertyName { get; }
        bool IsEmpty { get; }

        void EnqueuePropertyValueChangedMessage<TEntity>(string messageName)
            where TEntity : Entity, new();
    }
}
