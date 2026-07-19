using System.Linq.Expressions;
using System.Reflection;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Primitives;

public class PropertyDescription<T> : PropertyDescription
    where T : struct
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
            && Comparer<T>.Default.Compare(MinValue.Value, value) > 0)
        {
            value = MinValue.Value;
        }

        if (MaxValue != null
            && Comparer<T>.Default.Compare(MaxValue.Value, value) < 0)
        {
            value = MaxValue.Value;
        }

        PropertyInfo!.SetValue(component, value);
    }

    public T GetValue(Component component) => (T)PropertyInfo!.GetValue(component)!;

    public override Expression CreateGetValueExpression(Expression componentExpression)
        => Expression.Call(
            Expression.Constant(this), ((Func<Component, T>)GetValue).Method, componentExpression);

    public override void RaiseResolvedChange(
        Entity entity, int oldValue, int newValue, Component component)
        => entity.Manager.OnPropertyValueChanged(
            Name, ConvertValue(oldValue), ConvertValue(newValue), ComponentId, component);

    internal static PropertyDescription<T> Create(
        PropertyInfo property,
        PropertyAttribute propertyAttribute,
        ComponentAttribute componentAttribute)
        => new()
        {
            Name = property.Name,
            ComponentId = componentAttribute.Id,
            DeclaredType = property.PropertyType,
            PropertyInfo = property,
            DefaultCombinationFunction = propertyAttribute.DefaultCombinationFunction,
            MinValue = ConvertAttributeValue(propertyAttribute.MinValue),
            MaxValue = ConvertAttributeValue(propertyAttribute.MaxValue),
            DefaultValue = ConvertAttributeValue(propertyAttribute.DefaultValue)
        };

    private static T ConvertValue(int value)
    {
        if (typeof(T) == typeof(bool))
        {
            return (T)(object)(value != 0);
        }

        if (typeof(T).IsEnum)
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        return (T)(object)value;
    }

    private static T? ConvertAttributeValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is T converted)
        {
            return converted;
        }

        return typeof(T).IsEnum
            ? (T)Enum.ToObject(typeof(T), value)
            : (T)Convert.ChangeType(value, typeof(T));
    }
}
