using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnicornHack.Utils.DataLoading;

public class AttributeLoader<TPropertyAttribute, TClassAttribute, TLoadable> : Loader<TLoadable>
    where TClassAttribute : Attribute
    where TPropertyAttribute : Attribute
    where TLoadable : class, ILoadable
{
    private readonly Func<PropertyInfo, Type, TPropertyAttribute, TClassAttribute, TLoadable> _createLoadable;
    private readonly Assembly _targetAssembly;
    private readonly object _lockRoot = new object();

    public AttributeLoader(
        Func<PropertyInfo, Type, TPropertyAttribute, TClassAttribute, TLoadable> createLoadable,
        Assembly targetAssembly)
    {
        _createLoadable = createLoadable;
        _targetAssembly = targetAssembly;
    }

    protected override void EnsureLoaded()
    {
        if (NameLookup != null)
        {
            return;
        }

        lock (_lockRoot)
        {
            if (NameLookup != null)
            {
                return;
            }

            var lookup = new Dictionary<string, TLoadable>(StringComparer.OrdinalIgnoreCase);

            foreach (var type in _targetAssembly.GetTypes())
            {
                var typeAttribute =
                    (TClassAttribute)type.GetCustomAttribute(typeof(TClassAttribute), inherit: true);
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

            NameLookup = lookup;
        }
    }
}
