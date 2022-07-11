namespace UnicornHack.Utils.MessagingECS;

public interface IPropertyValueChanges
{
    int Count
    {
        get;
    }

    Component GetChangedComponent(int index);
    string GetChangedPropertyName(int index);
    T GetValue<T>(int index, ValueType type);

    void EnqueuePropertyValueChangedMessage<TEntity>(int index, string messageName)
        where TEntity : Entity, new();

    static IPropertyValueChanges Create<T1>(
        in PropertyValueChange<T1> change1)
        => change1.IsEmpty
            ? PropertyValueChanges.Instance
            : new PropertyValueChanges<T1>(change1);

    static IPropertyValueChanges Create<T1, T2>(
        in PropertyValueChange<T1> change1,
        in PropertyValueChange<T2> change2)
    {
        if (change1.IsEmpty)
        {
            return Create(change2);
        }

        return change2.IsEmpty
            ? new PropertyValueChanges<T1>(change1)
            : new PropertyValueChanges<T1, T2>(change1, change2);
    }

    static IPropertyValueChanges Create<T1, T2, T3>(
        in PropertyValueChange<T1> change1,
        in PropertyValueChange<T2> change2,
        in PropertyValueChange<T3> change3)
    {
        if (change1.IsEmpty)
        {
            return Create(change2, change3);
        }

        if (change2.IsEmpty)
        {
            return change3.IsEmpty
                ? new PropertyValueChanges<T1>(change1)
                : new PropertyValueChanges<T1, T3>(change1, change3);
        }

        return change3.IsEmpty
            ? new PropertyValueChanges<T1, T2>(change1, change2)
            : new PropertyValueChanges<T1, T2, T3>(change1, change2, change3);
    }
}
