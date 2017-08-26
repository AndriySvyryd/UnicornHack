using System;
using System.Collections.Generic;
using CSharpScriptSerialization;

namespace UnicornHack
{
    public class PropertyDescription<T> : PropertyDescription
    {
        public override Type PropertyType { get; set; } = typeof(T);
        public T DefaultValue { get; set; }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<PropertyDescription<T>>(GetPropertyConditions<PropertyDescription<T>>());

        protected new static Dictionary<string, Func<TPropertyDescription, object, bool>>
            GetPropertyConditions<TPropertyDescription>() where TPropertyDescription : PropertyDescription<T>
        {
            var propertyConditions = PropertyDescription.GetPropertyConditions<TPropertyDescription>();
            propertyConditions[nameof(PropertyType)] = (o, v) => false;

#pragma warning disable IDE0034 // Simplify 'default' expression
            propertyConditions.Add(nameof(DefaultValue), (o, v) => !Equals((T)v, default(T)));
#pragma warning restore IDE0034 // Simplify 'default' expression
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}