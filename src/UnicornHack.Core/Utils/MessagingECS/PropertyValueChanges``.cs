namespace UnicornHack.Utils.MessagingECS;

public class PropertyValueChanges<T1, T2> : IPropertyValueChanges
{
    private PropertyValueChange<T1> _change0;
    private PropertyValueChange<T2> _change1;
#if DEBUG
    private bool _pooled;
#endif

    public PropertyValueChanges(
        in PropertyValueChange<T1> change0,
        in PropertyValueChange<T2> change1)
    {
        _change0 = change0;
        _change1 = change1;
    }

    public static PropertyValueChanges<T1, T2> Rent(
        in PropertyValueChange<T1> change0,
        in PropertyValueChange<T2> change1)
    {
        var pooled = PropertyValueChangesPool<PropertyValueChanges<T1, T2>>.TryRent();
        if (pooled == null)
        {
            return new PropertyValueChanges<T1, T2>(change0, change1);
        }

        pooled._change0 = change0;
        pooled._change1 = change1;
#if DEBUG
        pooled._pooled = false;
#endif
        return pooled;
    }

    public int Count => 2;

    public IPropertyValueChanges Clone() => Rent(_change0, _change1);

    public void Return()
    {
#if DEBUG
        Debug.Assert(!_pooled, "PropertyValueChanges returned to the pool twice");
        _pooled = true;
#endif
        _change0 = PropertyValueChange<T1>.Empty;
        _change1 = PropertyValueChange<T2>.Empty;
        PropertyValueChangesPool<PropertyValueChanges<T1, T2>>.Return(this);
    }

    public Component GetChangedComponent(int index) => index switch
    {
        0 => _change0.ChangedComponent,
        1 => _change1.ChangedComponent,
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public string GetChangedPropertyName(int index) => index switch
    {
        0 => _change0.ChangedPropertyName,
        1 => _change1.ChangedPropertyName,
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
        < 0 or > 1 => throw new IndexOutOfRangeException(nameof(index)),
        _ => throw new InvalidOperationException(
            $"{typeof(T).Name} is not compatible with the change type {GetType(index)} at position {index}")
    };

    private Type GetType(int index) => index switch
    {
        0 => _change0.GetType().GenericTypeArguments[0],
        1 => _change1.GetType().GenericTypeArguments[0],
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
            default:
                throw new IndexOutOfRangeException(nameof(index));
        }
    }
}
