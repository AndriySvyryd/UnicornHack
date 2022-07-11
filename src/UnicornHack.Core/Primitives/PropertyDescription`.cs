using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Primitives;

public class PropertyDescription<T> : PropertyDescription
    where T : struct, IComparable<T>
{
    public override Type PropertyType
    {
        get;
    } = typeof(T);

    public T? DefaultValue
    {
        get;
        set;
    }

    public T? MinValue
    {
        get;
        set;
    }

    public T? MaxValue
    {
        get;
        set;
    }

    public void SetValue(T value, Component component)
    {
        if (MinValue != null
            && MinValue.Value.CompareTo(value) > 0)
        {
            value = MinValue.Value;
        }

        if (MaxValue != null
            && MaxValue.Value.CompareTo(value) < 0)
        {
            value = MaxValue.Value;
        }

        PropertyInfo.SetValue(component, value);
    }

    public T GetValue(Component component) => (T)PropertyInfo.GetValue(component)!;
}
