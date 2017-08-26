using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Data.Properties;
using UnicornHack.Utils;

namespace UnicornHack
{
    public abstract class PropertyDescription : ILoadable, ICSScriptSerializable
    {
        public string Name { get; set; }
        public string Desciption { get; set; }
        public abstract Type PropertyType { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }

        public static readonly CSScriptLoader<PropertyDescription> Loader =
            new CSScriptLoader<PropertyDescription>(@"Data\Properties", typeof(PropertyData));

        protected static Dictionary<string, Func<TPropertyDescription, object, bool>> GetPropertyConditions<
            TPropertyDescription>()
            where TPropertyDescription : PropertyDescription
        {
            return new Dictionary<string, Func<TPropertyDescription, object, bool>>
            {
                {nameof(Name), (o, v) => v != null},
                {nameof(Desciption), (o, v) => v != null},
                {nameof(PropertyType), (o, v) => v != null},
                {nameof(MinValue), (o, v) => v != null},
                {nameof(MaxValue), (o, v) => v != null}
            };
        }

        public abstract ICSScriptSerializer GetSerializer();
    }
}