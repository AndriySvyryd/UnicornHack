using System.Reflection;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation;

/// <summary>
///     Indicates a property that can have its value affected by an <see cref="EffectComponent" />
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class PropertyAttribute : Attribute
{
    /// <summary>The function used when a property effect specifies <see cref="ValueCombinationFunction.Default" />.</summary>
    public ValueCombinationFunction DefaultCombinationFunction
    {
        get;
        set;
    } = ValueCombinationFunction.Sum;

    public object? MinValue
    {
        get;
        set;
    }

    public object? MaxValue
    {
        get;
        set;
    }

    public object? DefaultValue
    {
        get;
        set;
    }

    internal abstract PropertyDescription CreateDescription(
        PropertyInfo property, ComponentAttribute componentAttribute);
}

public sealed class PropertyAttribute<T> : PropertyAttribute
    where T : struct
{
    internal override PropertyDescription CreateDescription(
        PropertyInfo property, ComponentAttribute componentAttribute)
    {
        if (property.PropertyType != typeof(T))
        {
            throw new InvalidOperationException(
                $"Property '{property.DeclaringType?.Name}.{property.Name}' has type"
                + $" '{property.PropertyType}', but its attribute declares '{typeof(T)}'.");
        }

        return PropertyDescription<T>.Create(property, this, componentAttribute);
    }
}
