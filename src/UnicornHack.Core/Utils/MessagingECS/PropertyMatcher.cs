using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyMatcher
    {
        private readonly Dictionary<(int, string), object> _properties = new Dictionary<(int, string), object>();

        public PropertyMatcher With<T>(Expression<Func<Component, T>> getProperty, int componentId)
        {
            var propertyName = getProperty.GetPropertyAccess().Name;
            _properties.Add((componentId, propertyName), getProperty.Compile());
            return this;
        }

        public bool TryGetValue<T>(Entity entity, int componentId, string propertyName,
            Component changedComponent, string changedPropertyName, T propertyValue, out T value)
        {
            if (!_properties.TryGetValue((componentId, propertyName), out var getValue))
            {
                throw new InvalidOperationException($"The matcher wasn't setup for property {propertyName}");
            }

            if (changedComponent?.ComponentId == componentId)
            {
                if (propertyName == changedPropertyName)
                {
                    value = propertyValue;
                    return true;
                }
            }
            else
            {
                changedComponent = entity.FindComponent(componentId);
                if (changedComponent == null)
                {
                    value = default;
                    return false;
                }
            }

            value = ((Func<Component, T>)getValue)(changedComponent);
            return true;
        }

        public bool TryGetValue<T>(Entity entity, int componentId, string propertyName,
            IReadOnlyList<IPropertyValueChange> changes, bool getOldValue, out T value)
        {
            if (!_properties.TryGetValue((componentId, propertyName), out var getValue))
            {
                throw new InvalidOperationException($"This matcher can't handle property {propertyName}");
            }

            Component component = null;
            if (changes != null)
            {
                for (var i = 0; i < changes.Count; i++)
                {
                    var valueChange = changes[i];

                    if (valueChange.ChangedComponent?.ComponentId == componentId)
                    {
                        if (valueChange.ChangedPropertyName == propertyName)
                        {
                            var change = (PropertyValueChange<T>)valueChange;
                            value = getOldValue ? change.OldValue : change.NewValue;
                            return true;
                        }

                        component = valueChange.ChangedComponent;
                    }
                }
            }

            if (component == null)
            {
                component = entity.FindComponent(componentId);
                if (component == null)
                {
                    value = default;
                    return false;
                }
            }

            value = ((Func<Component, T>)getValue)(component);
            return true;
        }

        public bool Matches(int componentId, string propertyName)
            => _properties.ContainsKey((componentId, propertyName));
    }
}
