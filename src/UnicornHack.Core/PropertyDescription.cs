using System;
using UnicornHack.Data.Properties;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class PropertyDescription : ILoadable
    {
        public string Name { get; set; }
        public string Desciption { get; set; }
        public Type PropertyType { get; set; }
        public object DefaultValue { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public PropertyCombinationBehavior CombineUsing { get; set; }

        public static readonly CSScriptLoader<PropertyDescription> Loader =
            new CSScriptLoader<PropertyDescription>(@"Data\Properties", typeof(PropertyData));
    }
}