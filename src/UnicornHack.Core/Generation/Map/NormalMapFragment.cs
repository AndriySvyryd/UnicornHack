using System;
using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map;

public class NormalMapFragment : MapFragment
{
    private Func<string, int, int, int, float> _weightFunction;

    private static readonly UnicornExpressionVisitor _translator =
        new(new[] { BranchParameter, DepthParameter, InstancesParameter, TagInstancesParameter });

    protected override void ResetWeightFunction() => _weightFunction = null;

    public Func<string, int, int, int, float> CreateWeightFunction(string expression)
        => _translator.Translate<Func<string, int, int, int, float>, float>(expression);

    public float GetWeight(LevelComponent level, Rectangle boundingRectangle)
    {
        // TODO: take transformations into account
        if (PayloadArea.Width > boundingRectangle.Width || PayloadArea.Height > boundingRectangle.Height)
        {
            return 0;
        }

        if (_weightFunction == null)
        {
            try
            {
                _weightFunction = CreateWeightFunction(GenerationWeight ?? DefaultWeight);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while parsing the GenerationWeight for " + Name, e);
            }
        }

        try
        {
            return _weightFunction(level.Branch.Name, level.Depth, 0, 0);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
        }
    }

    public static readonly CSScriptLoader<NormalMapFragment> Loader =
        new CSScriptLoader<NormalMapFragment>(@"Data\Fragments\Normal\", typeof(NormalMapFragmentData));

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<NormalMapFragment>(GetPropertyConditions<NormalMapFragment>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
