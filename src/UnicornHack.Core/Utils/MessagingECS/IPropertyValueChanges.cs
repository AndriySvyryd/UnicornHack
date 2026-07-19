namespace UnicornHack.Utils.MessagingECS;

public interface IPropertyValueChanges
{
    int Count
    {
        get;
    }

    Component GetChangedComponent(int index);
    string GetChangedPropertyName(int index);
    object? GetNestedItem(int index);
    string? GetNestedPropertyName(int index);
    T GetValue<T>(int index, ValueType type);

    void EnqueuePropertyValueChangedMessage<TEntity>(int index, string messageName)
        where TEntity : Entity, new();

    /// <summary>
    ///     Returns an independent copy backed by the per-thread pool. A holder that outlives the
    ///     creator takes a clone so it owns its instance
    ///     exclusively and can <see cref="Return" /> it when cleaned, without the creator and the
    ///     message sharing (and racing to pool) the same instance. The shared empty singleton
    ///     clones to itself.
    /// </summary>
    IPropertyValueChanges Clone();

    /// <summary>
    ///     Resets the captured state and returns this instance to the per-thread pool. Each owner
    ///     (the creator, and every message holding a clone) returns exactly its own instance once.
    ///     A no-op on the shared empty singleton.
    /// </summary>
    void Return();

    static IPropertyValueChanges Create<T1>(
        in PropertyValueChange<T1> change1)
        => change1.IsEmpty
            ? PropertyValueChanges.Instance
            : PropertyValueChanges<T1>.Rent(change1);

    static IPropertyValueChanges Create<T1, T2>(
        in PropertyValueChange<T1> change1,
        in PropertyValueChange<T2> change2)
    {
        if (change1.IsEmpty)
        {
            return Create(change2);
        }

        return change2.IsEmpty
            ? PropertyValueChanges<T1>.Rent(change1)
            : PropertyValueChanges<T1, T2>.Rent(change1, change2);
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
                ? PropertyValueChanges<T1>.Rent(change1)
                : PropertyValueChanges<T1, T3>.Rent(change1, change3);
        }

        return change3.IsEmpty
            ? PropertyValueChanges<T1, T2>.Rent(change1, change2)
            : PropertyValueChanges<T1, T2, T3>.Rent(change1, change2, change3);
    }
}
