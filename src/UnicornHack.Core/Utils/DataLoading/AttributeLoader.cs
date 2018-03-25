using System;
using System.Reflection;

namespace UnicornHack.Utils.DataLoading
{
    public class AttributeLoader<TPropertyAttribute, TClassAttribute, TLoadable> : Loader<TLoadable>
        where TClassAttribute : Attribute
        where TPropertyAttribute : Attribute
        where TLoadable : class, ILoadable
    {
        private readonly Func<PropertyInfo, Type, TPropertyAttribute, TClassAttribute, TLoadable> _createLoadable;
        private readonly Assembly _targetAssembly;

        public AttributeLoader(
            Func<PropertyInfo, Type, TPropertyAttribute, TClassAttribute, TLoadable> createLoadable,
            Assembly targetAssembly)
        {
            _createLoadable = createLoadable;
            _targetAssembly = targetAssembly;
        }

        protected override void EnsureLoaded()
        {
            if (NameLookup.Count != 0)
            {
                return;
            }

            foreach (var type in _targetAssembly.GetTypes())
            {
                var typeAttribute = (TClassAttribute)type.GetCustomAttribute(typeof(TClassAttribute), inherit: true);
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
                    NameLookup[instance.Name] = instance;
                }
            }
        }
    }
}
