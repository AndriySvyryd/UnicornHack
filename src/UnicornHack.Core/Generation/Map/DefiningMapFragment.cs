using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation.Map
{
    public class DefiningMapFragment : ConnectingMapFragment
    {
        public byte LevelHeight { get; set; } = 40;
        public byte LevelWidth { get; set; } = 80;
        public Layout Layout { get; set; } = new EmptyLayout();
        public CreatureGenerator CreatureGenerator { get; set; } = new CreatureGenerator();
        public ItemGenerator ItemGenerator { get; set; } = new ItemGenerator();
        public MapFeature DefaultTerrain { get; set; }
        public MapFeature DefaultPathTerrain { get; set; }

        // TODO: Subfragment, item and creature generation weight and distribution modifiers

        private Func<string, byte, int, int, float> _weightFunction;

        public float GetWeight(string branchName, byte depth)
        {
            if (_weightFunction == null)
            {
                _weightFunction = (GenerationWeight ?? new DefaultWeight()).CreateFragmentWeightFunction();
            }

            // TODO: pass correct counts
            return _weightFunction(branchName, depth, 0, 0);
        }

        public static new readonly CSScriptLoader<DefiningMapFragment> Loader =
            new CSScriptLoader<DefiningMapFragment>(@"Data\Fragments\Defining\", typeof(DefiningMapFragmentData));

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<DefiningMapFragment>(GetPropertyConditions<DefiningMapFragment>());

        protected static new Dictionary<string, Func<TDefiningMapFragment, object, bool>>
            GetPropertyConditions<TDefiningMapFragment>() where TDefiningMapFragment : DefiningMapFragment
        {
            var propertyConditions = ConnectingMapFragment.GetPropertyConditions<TDefiningMapFragment>();
            var mapCondition = propertyConditions[nameof(Map)];
            propertyConditions.Remove(nameof(Map));

            propertyConditions.Add(nameof(NoRandomDoorways), (o, v) => (bool)v);
            propertyConditions.Add(nameof(LevelHeight), (o, v) => (byte)v != 40);
            propertyConditions.Add(nameof(LevelWidth), (o, v) => (byte)v != 80);
            propertyConditions.Add(nameof(Layout), (o, v) => v != null && !(v is EmptyLayout));
            propertyConditions.Add(nameof(CreatureGenerator),
                (o, v) => v != null && ((CreatureGenerator)v).ExpectedInitialCount !=
                          new CreatureGenerator().ExpectedInitialCount);
            propertyConditions.Add(nameof(ItemGenerator),
                (o, v) => v != null &&
                          ((ItemGenerator)v).ExpectedInitialCount != new ItemGenerator().ExpectedInitialCount);
            propertyConditions.Add(nameof(DefaultTerrain), (o, v) => (MapFeature)v != MapFeature.Default);
            propertyConditions.Add(nameof(DefaultPathTerrain), (o, v) => (MapFeature)v != MapFeature.Default);
            propertyConditions.Add(nameof(Map), mapCondition);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
