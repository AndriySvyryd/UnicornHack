using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class EncompassingMapFragment : MapFragment
    {
        public override MapFragmentType Type => MapFragmentType.Encompassing;
        public virtual byte LevelHeight { get; set; }
        public virtual byte LevelWidth { get; set; }
        // TODO: Subfragment, item and creature generation weight and distribution modifiers
        // TODO: default terrain type for floor/wall/empty space

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

        public new static readonly CSScriptLoader<EncompassingMapFragment> Loader =
            new CSScriptLoader<EncompassingMapFragment>(@"data\fragments\encompassing\");

        public new static EncompassingMapFragment Get(string name) => Loader.Get(name);

        public static IEnumerable<EncompassingMapFragment> GetAllEncompassingMapFragments() => Loader.GetAll();

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