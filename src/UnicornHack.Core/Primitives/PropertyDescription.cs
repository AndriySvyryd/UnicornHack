using System.Linq.Expressions;
using System.Reflection;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Primitives;

public abstract class PropertyDescription : ILoadable
{
    public string Name
    {
        get;
        init;
    } = null!;

    public int ComponentId
    {
        get;
        init;
    }

    public ValueCombinationFunction DefaultCombinationFunction
    {
        get;
        init;
    }

    /// <summary>
    ///     The property's declared CLR type - the concrete enum, or <see cref="int" />/<see cref="bool" /> -
    ///     used to route change notifications with the correct type. Stored explicitly so a description needs
    ///     no backing <see cref="System.Reflection.PropertyInfo" /> and can describe a property with no field.
    /// </summary>
    public Type DeclaredType
    {
        get;
        init;
    } = null!;

    /// <summary>
    ///     The backing component property, present only for a field-backed (<c>[Property&lt;T&gt;]</c>) description;
    ///     <see langword="null" /> for a purely dynamic property resolved solely through the effect layers.
    ///     Only reflection-based readers (e.g. compiled <see cref="Condition" />s) use it.
    /// </summary>
    protected PropertyInfo? PropertyInfo
    {
        get;
        init;
    }

    public abstract Type PropertyType
    {
        get;
    }

    /// <summary>
    ///     Raises the ECS property-value-changed notification for a resolved property, using the
    ///     property's declared CLR type (int, bool, or the concrete enum) so it routes to the same
    ///     typed message queue the legacy backing-field setter would have used.
    /// </summary>
    public abstract void RaiseResolvedChange(
        Entity entity, int oldValue, int newValue, Component component);

    public abstract Expression CreateGetValueExpression(Expression componentExpression);

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(BeingComponent))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(PhysicalComponent))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ActiveEffectsComponent))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ItemComponent))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(PositionComponent))]
    public static readonly AttributeLoader<PropertyAttribute, ComponentAttribute, PropertyDescription> Loader =
        new(CreatePropertyDescription,
        [
            typeof(BeingComponent),
            typeof(PhysicalComponent),
            typeof(ActiveEffectsComponent),
            typeof(ItemComponent),
            typeof(PositionComponent)
        ]);

    private static PropertyDescription CreatePropertyDescription(
        PropertyInfo property,
        Type componentType,
        PropertyAttribute propertyAttribute,
        ComponentAttribute componentAttribute)
        => propertyAttribute.CreateDescription(property, componentAttribute);
}
