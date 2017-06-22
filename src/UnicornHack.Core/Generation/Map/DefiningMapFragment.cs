using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class DefiningMapFragment : ConnectingMapFragment
    {
        public virtual byte LevelHeight { get; set; } = 40;
        public virtual byte LevelWidth { get; set; } = 80;
        public virtual Layout Layout { get; set; } = new EmptyLayout();
        public virtual CreatureGenerator CreatureGenerator { get; set; } = new CreatureGenerator();
        public virtual ItemGenerator ItemGenerator { get; set; } = new ItemGenerator();
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

        public static readonly CSScriptLoader<DefiningMapFragment> DefiningLoader =
            new CSScriptLoader<DefiningMapFragment>(@"data\fragments\defining\");

        public static DefiningMapFragment GetDefiningMapFragment(string name) => DefiningLoader.Get(name);

        public static IReadOnlyList<DefiningMapFragment> GetAllDefiningMapFragments() => DefiningLoader
            .GetAll();

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<DefiningMapFragment>(
            GetPropertyConditions<DefiningMapFragment>());

        protected new static Dictionary<string, Func<TDefiningMapFragment, object, bool>> GetPropertyConditions
            <TDefiningMapFragment>()
            where TDefiningMapFragment : DefiningMapFragment
        {
            var propertyConditions = ConnectingMapFragment.GetPropertyConditions<TDefiningMapFragment>();
            var mapCondition = propertyConditions[nameof(Map)];
            propertyConditions.Remove(nameof(Map));

            propertyConditions.Add(nameof(NoRandomDoorways), (o, v) => (bool)v);
            propertyConditions.Add(nameof(LevelHeight), (o, v) => (byte)v != 40);
            propertyConditions.Add(nameof(LevelWidth), (o, v) => (byte)v != 80);
            propertyConditions.Add(nameof(Layout), (o, v) => v != null && !(v is EmptyLayout));
            propertyConditions.Add(nameof(Map), mapCondition);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;

        #endregion
    }
}