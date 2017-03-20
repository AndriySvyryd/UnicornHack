using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class EncompassingMapFragment : ConnectingMapFragment
    {
        public virtual byte LevelHeight { get; set; }
        public virtual byte LevelWidth { get; set; }
        public virtual Layout Layout { get; set; }
        public virtual MapFeature DefaultTerrain { get; set; }
        public virtual MapFeature DefaultPathTerrain { get; set; }

        // TODO: Subfragment, item and creature generation weight and distribution modifiers

        #region Actions

        private Func<string, byte, int, int, float> _weightFunction;

        public virtual float GetWeight(string branchName, byte depth)
        {
            if (_weightFunction == null)
            {
                _weightFunction = GenerationWeight.CreateFragmentWeightFunction();
            }

            // TODO: pass correct counts
            return _weightFunction(branchName, depth, 0, 0);
        }

        public override float GetWeight(Level level, Rectangle boundingRectangle)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region Serialization

        public static readonly CSScriptLoader<EncompassingMapFragment> EncompassingLoader =
            new CSScriptLoader<EncompassingMapFragment>(@"data\fragments\encompassing\");

        public static EncompassingMapFragment GetEncompassingMapFragment(string name) => EncompassingLoader.Get(name);

        public static IReadOnlyList<EncompassingMapFragment> GetAllEncompassingMapFragments() => EncompassingLoader
            .GetAll();

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<EncompassingMapFragment>(
            GetPropertyConditions<EncompassingMapFragment>());

        protected new static Dictionary<string, Func<TEncompassingMapFragment, object, bool>> GetPropertyConditions
            <TEncompassingMapFragment>()
            where TEncompassingMapFragment : EncompassingMapFragment
        {
            var propertyConditions = ConnectingMapFragment.GetPropertyConditions<TEncompassingMapFragment>();
            var mapCondition = propertyConditions[nameof(Map)];
            propertyConditions.Remove(nameof(Map));

            propertyConditions.Add(nameof(NoRandomDoorways), (o, v) => (bool)v);
            propertyConditions.Add(nameof(LevelHeight), (o, v) => (byte)v != 0);
            propertyConditions.Add(nameof(LevelWidth), (o, v) => (byte)v != 0);
            propertyConditions.Add(nameof(Layout), (o, v) => v != null && !(v is EmptyLayout));
            propertyConditions.Add(nameof(Map), mapCondition);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;

        #endregion
    }
}