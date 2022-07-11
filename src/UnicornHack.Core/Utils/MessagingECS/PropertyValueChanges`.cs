namespace UnicornHack.Utils.MessagingECS;

public class PropertyValueChanges<T1> : IPropertyValueChanges
{
    private readonly PropertyValueChange<T1> _change0;

    public PropertyValueChanges(
        in PropertyValueChange<T1> change0)
    {
        _change0 = change0;
    }

    public int Count => 1;

    public Component GetChangedComponent(int index) => index switch
    {
        0 => _change0.ChangedComponent,
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public string GetChangedPropertyName(int index) => index switch
    {
        0 => _change0.ChangedPropertyName,
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public T GetValue<T>(int index, ValueType type) => index switch
    {
        0 when _change0 is PropertyValueChange<T> change => type == ValueType.Current
            ? change.NewValue
            : change.OldValue,
        < 0 or > 0 => throw new IndexOutOfRangeException(nameof(index)),
        _ => throw new InvalidOperationException(
            $"{typeof(T).Name} is not compatible with the change type {GetType(index)} at position {index}")
    };

    private Type GetType(int index) => index switch
    {
        0 => _change0.GetType().GenericTypeArguments[0],
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
            default:
                throw new IndexOutOfRangeException(nameof(index));
        }
    }
}
