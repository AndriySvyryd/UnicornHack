namespace UnicornHack.Utils.MessagingECS;

public class PropertyValueChanges<T1, T2, T3> : IPropertyValueChanges
{
    private readonly PropertyValueChange<T1> _change0;
    private readonly PropertyValueChange<T2> _change1;
    private readonly PropertyValueChange<T3> _change2;

    public PropertyValueChanges(
        in PropertyValueChange<T1> change0,
        in PropertyValueChange<T2> change1,
        in PropertyValueChange<T3> change2)
    {
        _change0 = change0;
        _change1 = change1;
        _change2 = change2;
    }

    public int Count => 3;

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
        < 0 or > 2 => throw new IndexOutOfRangeException(nameof(index)),
        _ => throw new InvalidOperationException(
            $"{typeof(T).Name} is not compatible with the change type {GetType(index)} at position {index}")
    };

    private Type GetType(int index) => index switch
    {
        0 => _change0.GetType().GenericTypeArguments[0],
        1 => _change1.GetType().GenericTypeArguments[0],
        2 => _change2.GetType().GenericTypeArguments[0],
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
