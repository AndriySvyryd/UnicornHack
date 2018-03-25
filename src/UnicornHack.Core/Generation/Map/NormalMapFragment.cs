using System;
using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map
{
    public class NormalMapFragment : MapFragment
    {
        private Func<string, byte, int, int, float> _weightFunction;

        public float GetWeight(LevelComponent level, Rectangle boundingRectangle)
        {
            // TODO: take transformations into account
            if (PayloadArea.Width > boundingRectangle.Width || PayloadArea.Height > boundingRectangle.Height)
            {
                return 0;
            }

            if (_weightFunction == null)
            {
                _weightFunction = (GenerationWeight ?? new DefaultWeight()).CreateFragmentWeightFunction();
            }

            return _weightFunction(level.Branch.Name, level.Depth, 0, 0);
        }

        public static readonly CSScriptLoader<NormalMapFragment> Loader =
            new CSScriptLoader<NormalMapFragment>(@"Data\Fragments\Normal\", typeof(NormalMapFragmentData));

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<NormalMapFragment>(GetPropertyConditions<NormalMapFragment>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
