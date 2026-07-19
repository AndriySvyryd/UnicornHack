using System.Reflection;

namespace UnicornHack.Utils.DataLoading;

public class AttributeLoader<TPropertyAttribute, TClassAttribute, TLoadable> : Loader<TLoadable>
    where TClassAttribute : Attribute
    where TPropertyAttribute : Attribute
    where TLoadable : class, ILoadable
{
    private readonly Func<PropertyInfo, Type, TPropertyAttribute, TClassAttribute, TLoadable> _createLoadable;
    private readonly IReadOnlyList<Type> _targetTypes;
    private readonly object _lockRoot = new();

    public AttributeLoader(
        Func<PropertyInfo, Type, TPropertyAttribute, TClassAttribute, TLoadable> createLoadable,
        IReadOnlyList<Type> targetTypes)
    {
        _createLoadable = createLoadable;
        _targetTypes = targetTypes;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "The supplied types are explicitly rooted with DynamicDependency attributes at the loader declaration.")]
    protected override Dictionary<string, TLoadable> CreateNameLookup()
    {
        lock (_lockRoot)
        {
            var lookup = new Dictionary<string, TLoadable>(StringComparer.OrdinalIgnoreCase);

            foreach (var type in _targetTypes)
            {
                var typeAttribute =
                    (TClassAttribute?)type.GetCustomAttribute(typeof(TClassAttribute), inherit: true);
                if (typeAttribute == null)
                {
                    continue;
                }

                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var propertyAttribute = property.GetCustomAttribute<TPropertyAttribute>(inherit: true);
                    if (propertyAttribute == null)
                    {
                        continue;
                    }

                    var instance = _createLoadable(property, type, propertyAttribute, typeAttribute);
                    lookup[instance.Name] = instance;
                }
            }

            return lookup;
        }
    }
}
