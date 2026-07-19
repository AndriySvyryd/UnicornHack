namespace UnicornHack.Utils.MessagingECS;

public class PropertyValueChanges : IPropertyValueChanges
{
    private PropertyValueChanges()
    {
    }

    public static readonly PropertyValueChanges Instance = new();

    public int Count => 0;

    public Component GetChangedComponent(int index)
        => throw new IndexOutOfRangeException(nameof(index));

    public string GetChangedPropertyName(int index)
        => throw new IndexOutOfRangeException(nameof(index));

    public object? GetNestedItem(int index)
        => throw new IndexOutOfRangeException(nameof(index));

    public string? GetNestedPropertyName(int index)
        => throw new IndexOutOfRangeException(nameof(index));

    public T GetChange<T>(int index)
        => throw new IndexOutOfRangeException(nameof(index));

    public T GetValue<T>(int index, ValueType type)
        => throw new IndexOutOfRangeException(nameof(index));

    public void EnqueuePropertyValueChangedMessage<TEntity>(int index, string messageName)
        where TEntity : Entity, new()
        => throw new IndexOutOfRangeException(nameof(index));

    // The empty singleton is shared and never pooled: it clones to itself and ignores Return.
    public IPropertyValueChanges Clone() => this;

    public void Return()
    {
    }
}
