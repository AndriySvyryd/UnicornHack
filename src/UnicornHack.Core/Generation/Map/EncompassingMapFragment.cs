using System;
using System.Collections.Generic;
using CSharpScriptSerialization;

namespace UnicornHack.Generation.Map
{
    public class EncompassingMapFragment : MapFragment
    {
        public override MapFragmentType Type => MapFragmentType.Encompassing;
        public virtual byte LevelHeight { get; set; }
        public virtual byte LevelWidth { get; set; }
        // TODO: Subfragment, item and creature generation weight and distribution modifiers
        // TODO: default terrain type for floor/wall/empty space

        #region Serialization

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<EncompassingMapFragment>(
            GetPropertyConditions<EncompassingMapFragment>());

        protected new static Dictionary<string, Func<TEncompassingMapFragment, object, bool>> GetPropertyConditions
            <TEncompassingMapFragment>()
            where TEncompassingMapFragment : EncompassingMapFragment
        {
            var propertyConditions = MapFragment.GetPropertyConditions<TEncompassingMapFragment>();
            propertyConditions[nameof(Type)] = (o, v) => false;
            propertyConditions.Add(nameof(LevelHeight), (o, v) => (byte)v != 0);
            propertyConditions.Add(nameof(LevelWidth), (o, v) => (byte)v != 0);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;

        #endregion
    }
}