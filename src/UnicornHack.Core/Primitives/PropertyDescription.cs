using System;
using System.Linq;
using System.Reflection;
using UnicornHack.Generation;
using UnicornHack.Utils;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Primitives;

public abstract class PropertyDescription : ILoadable
{
    public string Name => PropertyInfo.Name;

    public int ComponentId
    {
        get;
        set;
    }

    protected PropertyInfo PropertyInfo
    {
        get;
        set;
    }

    public abstract Type PropertyType
    {
        get;
    }

    public bool IsCalculated
    {
        get;
        set;
    }

    public static readonly AttributeLoader<PropertyAttribute, ComponentAttribute, PropertyDescription> Loader =
        new AttributeLoader<PropertyAttribute, ComponentAttribute, PropertyDescription>(
            CreatePropertyDescription, typeof(PropertyDescription).Assembly);

    private static PropertyDescription CreatePropertyDescription(
        PropertyInfo property,
        Type componentType,
        PropertyAttribute propertyAttribute,
        ComponentAttribute componentAttribute)
        => (PropertyDescription)CreatePropertyDescriptionMethod
            .MakeGenericMethod(property.PropertyType.IsEnum
                ? property.PropertyType.GetEnumUnderlyingType().UnwrapNullableType()
                : property.PropertyType.UnwrapNullableType())
            .Invoke(null, new object[] { property, componentType, propertyAttribute, componentAttribute });

    private static readonly MethodInfo CreatePropertyDescriptionMethod = typeof(PropertyDescription).GetTypeInfo()
        .GetDeclaredMethods(nameof(CreatePropertyDescription)).Single(mi => mi.IsGenericMethodDefinition);

#pragma warning disable RCS1213 // Remove unused member declaration.
    private static PropertyDescription<T> CreatePropertyDescription<T>(
        PropertyInfo property,
        Type componentType,
        PropertyAttribute propertyAttribute,
        ComponentAttribute componentAttribute)
        where T : struct, IComparable<T>
        => new PropertyDescription<T>
        {
            ComponentId = componentAttribute.Id,
            PropertyInfo = property,
            IsCalculated = propertyAttribute.IsCalculated,
            MinValue = (T?)propertyAttribute.MinValue,
            MaxValue = (T?)propertyAttribute.MaxValue,
            DefaultValue = (T?)propertyAttribute.DefaultValue
                           ?? (propertyAttribute.IsCalculated
                               ? null
                               : (T?)property.GetValue(Activator.CreateInstance(componentType)))
        };
#pragma warning restore RCS1213 // Remove unused member declaration.
}
