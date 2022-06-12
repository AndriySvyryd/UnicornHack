using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS
{
    public class PropertyMatcher<TEntity>
        where TEntity : Entity, new()
    {
        public virtual PropertyMatcher<TEntity> With<T>(Expression<Func<Component, T>> getProperty, int componentId)
        {
            var propertyName = getProperty.GetSimplePropertyAccess().Name;
            return new UnaryPropertyMatcher((componentId, propertyName, getProperty.Compile()));
        }

        public bool TryGetValue<T>(
            in EntityChange<TEntity> entityChange, int componentId, string propertyName, ValueType type, out T value)
        {
            var getValue = Find(componentId, propertyName);
            if (getValue == null)
            {
                throw new InvalidOperationException($"This matcher can't handle property {propertyName}");
            }

            Component component = null;
            var changes = entityChange.PropertyChanges;
            if (changes != null)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < changes.Count; i++)
                {
                    var changedComponent = changes.GetChangedComponent(i);
                    if (changedComponent.ComponentId == componentId)
                    {
                        if (changes.GetChangedPropertyName(i) == propertyName)
                        {
                            value = changes.GetValue<T>(i, type);
                            return true;
                        }

                        component = changedComponent;
                    }
                }
            }

            if (component == null)
            {
                var changedComponent = entityChange.RemovedComponent;
                if (changedComponent != null
                    && changedComponent.ComponentId == componentId
                    && (type == ValueType.PreferOld
                        || ((changedComponent.Entity.FindComponent(componentId) == changedComponent)
                            == (type == ValueType.Current))))
                {
                    component = changedComponent;
                }
            }

            if (component == null
                && type != ValueType.Old)
            {
                component = entityChange.Entity.FindComponent(componentId);
            }

            if (component == null)
            {
                value = default;
                return false;
            }

            value = ((Func<Component, T>)getValue)(component);
            return true;
        }

        protected virtual object Find(int componentId, string propertyName)
            => null;

        public virtual bool Matches(int componentId, string propertyName)
            => false;

        private class UnaryPropertyMatcher : PropertyMatcher<TEntity>
        {
            private readonly (int, string, object) _property;

            public UnaryPropertyMatcher((int, string, object) property)
                => _property = property;

            public override PropertyMatcher<TEntity> With<T>(Expression<Func<Component, T>> getProperty, int componentId)
            {
                var propertyName = getProperty.GetSimplePropertyAccess().Name;
                return new BinaryPropertyMatcher(_property, (componentId, propertyName, getProperty.Compile()));
            }

            protected override object Find(int componentId, string propertyName)
            {
                if (_property.Item1 == componentId
                    && _property.Item2 == propertyName)
                {
                    return _property.Item3;
                }

                return default;
            }

            private bool Matches(int componentId)
            {
                if (_property.Item1 == componentId)
                {
                    return true;
                }

                return false;
            }

            public override bool Matches(int componentId, string propertyName)
                => propertyName == null
                    ? Matches(componentId)
                    : Find(componentId, propertyName) != null;
        }

        private class BinaryPropertyMatcher : PropertyMatcher<TEntity>
        {
            private readonly (int, string, object) _property0;
            private readonly (int, string, object) _property1;

            public BinaryPropertyMatcher(
                (int, string, object) property0,
                (int, string, object) property1)
            {
                _property0 = property0;
                _property1 = property1;
            }

            public override PropertyMatcher<TEntity> With<T>(Expression<Func<Component, T>> getProperty, int componentId)
            {
                var dictionary = new Dictionary<(int, string), object>
                {
                    {(_property0.Item1, _property0.Item2), _property0.Item3},
                    {(_property1.Item1, _property1.Item2), _property1.Item3}
                };

                var propertyName = getProperty.GetSimplePropertyAccess().Name;
                dictionary.Add((componentId, propertyName), getProperty.Compile());

                return new DictionaryPropertyMatcher(dictionary);
            }

            protected override object Find(int componentId, string propertyName)
            {
                if (_property0.Item1 == componentId
                    && _property0.Item2 == propertyName)
                {
                    return _property0.Item3;
                }

                if (_property1.Item1 == componentId
                    && _property1.Item2 == propertyName)
                {
                    return _property1.Item3;
                }

                return default;
            }

            private bool Matches(int componentId)
            {
                if (_property0.Item1 == componentId)
                {
                    return true;
                }

                if (_property1.Item1 == componentId)
                {
                    return true;
                }

                return false;
            }

            public override bool Matches(int componentId, string propertyName)
                => propertyName == null
                    ? Matches(componentId)
                    : Find(componentId, propertyName) != null;
        }

        private class DictionaryPropertyMatcher : PropertyMatcher<TEntity>
        {
            private readonly Dictionary<(int, string), object> _properties;

            public DictionaryPropertyMatcher(Dictionary<(int, string), object> properties)
                => _properties = properties;

            public override PropertyMatcher<TEntity> With<T>(Expression<Func<Component, T>> getProperty, int componentId)
            {
                var propertyName = getProperty.GetSimplePropertyAccess().Name;
                _properties.Add((componentId, propertyName), getProperty.Compile());
                return this;
            }

            protected override object Find(int componentId, string propertyName)
                => _properties[(componentId, propertyName)];

            public override bool Matches(int componentId, string propertyName)
                => propertyName == null
                    ? _properties.Keys.Any(k => k.Item1 == componentId)
                    : _properties.ContainsKey((componentId, propertyName));
        }
    }
}
