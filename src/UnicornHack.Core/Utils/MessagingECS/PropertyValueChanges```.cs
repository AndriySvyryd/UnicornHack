namespace UnicornHack.Utils.MessagingECS;

public class PropertyValueChanges<T1, T2, T3> : IPropertyValueChanges
{
    private PropertyValueChange<T1> _change0;
    private PropertyValueChange<T2> _change1;
    private PropertyValueChange<T3> _change2;
#if DEBUG
    private bool _pooled;
#endif

    public PropertyValueChanges(
        in PropertyValueChange<T1> change0,
        in PropertyValueChange<T2> change1,
        in PropertyValueChange<T3> change2)
    {
        _change0 = change0;
        _change1 = change1;
        _change2 = change2;
    }

    public static PropertyValueChanges<T1, T2, T3> Rent(
        in PropertyValueChange<T1> change0,
        in PropertyValueChange<T2> change1,
        in PropertyValueChange<T3> change2)
    {
        var pooled = PropertyValueChangesPool<PropertyValueChanges<T1, T2, T3>>.TryRent();
        if (pooled == null)
        {
            return new PropertyValueChanges<T1, T2, T3>(change0, change1, change2);
        }

        pooled._change0 = change0;
        pooled._change1 = change1;
        pooled._change2 = change2;
#if DEBUG
        pooled._pooled = false;
#endif
        return pooled;
    }

    public int Count => 3;

    public IPropertyValueChanges Clone() => Rent(_change0, _change1, _change2);

    public void Return()
    {
#if DEBUG
        Debug.Assert(!_pooled, "PropertyValueChanges returned to the pool twice");
        _pooled = true;
#endif
        _change0 = PropertyValueChange<T1>.Empty;
        _change1 = PropertyValueChange<T2>.Empty;
        _change2 = PropertyValueChange<T3>.Empty;
        PropertyValueChangesPool<PropertyValueChanges<T1, T2, T3>>.Return(this);
    }

    public Component GetChangedComponent(int index) => index switch
    {
        0 => _change0.ChangedComponent,
        1 => _change1.ChangedComponent,
        2 => _change2.ChangedComponent,
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public string GetChangedPropertyName(int index) => index switch
    {
        0 => _change0.ChangedPropertyName,
        1 => _change1.ChangedPropertyName,
        2 => _change2.ChangedPropertyName,
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public object? GetNestedItem(int index) => index switch
    {
        0 => _change0.NestedItem,
        1 => _change1.NestedItem,
        2 => _change2.NestedItem,
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public string? GetNestedPropertyName(int index) => index switch
    {
        0 => _change0.NestedPropertyName,
        1 => _change1.NestedPropertyName,
        2 => _change2.NestedPropertyName,
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public T GetValue<T>(int index, ValueType type) => index switch
    {
        0 when _change0 is PropertyValueChange<T> change => type == ValueType.Current
            ? change.NewValue
            : change.OldValue,
        1 when _change1 is PropertyValueChange<T> change => type == ValueType.Current
            ? change.NewValue
            : change.OldValue,
        2 when _change2 is PropertyValueChange<T> change => type == ValueType.Current
            ? change.NewValue
            : change.OldValue,
        0 => throw new InvalidOperationException(
            $"{typeof(T).Name} is not compatible with the change type {typeof(T1).Name} at position {index}"),
        1 => throw new InvalidOperationException(
            $"{typeof(T).Name} is not compatible with the change type {typeof(T2).Name} at position {index}"),
        2 => throw new InvalidOperationException(
            $"{typeof(T).Name} is not compatible with the change type {typeof(T3).Name} at position {index}"),
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public void EnqueuePropertyValueChangedMessage<TEntity>(int index, string messageName)
        where TEntity : Entity, new()
    {
        switch (index)
        {
            case 0:
                _change0.EnqueuePropertyValueChangedMessage<TEntity>(messageName);
                break;
            case 1:
                _change1.EnqueuePropertyValueChangedMessage<TEntity>(messageName);
                break;
            case 2:
                _change2.EnqueuePropertyValueChangedMessage<TEntity>(messageName);
                break;
            default:
                throw new IndexOutOfRangeException(nameof(index));
        }
    }
}
