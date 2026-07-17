using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation.Map;

/// <summary>
///     A <see cref="CSScriptLoader{T}" /> that expands every source fragment into its full
///     D4 orbit (<see cref="MapFragmentTransform" />), constrained by the source's
///     <see cref="MapFragment.NoMirror" />, <see cref="MapFragment.NoTurnOver" />, and
///     <see cref="MapFragment.NoTransform" /> flags.
///     <para>
///         The loader initializes each source fragment via
///         <see cref="MapFragment.EnsureInitialized()" /> and materializes every
///         non-identity transform in the orbit as a sibling entry keyed by the variant's
///         transformed name. No geometry-based deduplication is performed — authors that
///         want to collapse symmetric payloads must set the matching exclusion flag.
///         <see cref="MapFragment.VariantMultiplier" /> is derived dynamically from
///         <see cref="MapFragment.VariantCount" /> so the sum of the orbit's pick weights
///         equals the source's authored weight.
///     </para>
/// </summary>
public class MapFragmentLoader<T> : CSScriptLoader<T> where T : MapFragment
{
    public MapFragmentLoader(string relativePath, Type dataType) : base(relativePath, dataType)
    {
    }

    protected override Dictionary<string, T> Load()
    {
        var sources = base.Load();
        if (sources.Count == 0)
        {
            return sources;
        }

        var expanded = new Dictionary<string, T>(sources.Count * 4, StringComparer.OrdinalIgnoreCase);
        foreach (var source in sources.Values)
        {
            source.EnsureInitialized();
            source.ValidatePlacementCapability();
            // Compile the weight delegate once on the source before cloning, so every variant
            // in the orbit shares it through CreateVariant's member-wise clone.
            source.EnsureWeightFunctionCompiled();
            expanded[source.Name] = source;

            var orbit = MapFragmentTransforms.GetOrbit(source.NoMirror, source.NoTurnOver, source.NoTransform);
            foreach (var transform in orbit)
            {
                if (transform == MapFragmentTransform.Identity)
                {
                    continue;
                }

                var variant = (T)source.CreateVariant(transform);
                expanded[variant.Name] = variant;
            }
        }

        return expanded;
    }
}
