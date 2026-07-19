using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Services;

/// <summary>
///     Game-scoped façade over the <c>*.Loader</c> singletons that hold parsed CS-script
///     game data (creatures, items, abilities, branches, fragments, …).
/// </summary>
/// <remarks>
///     <para>
///         Production runtime code goes through this abstraction instead so that:
///     </para>
///     <list type="bullet">
///         <item>
///             Tests can isolate the loaders they mutate (most importantly
///             <see cref="DefiningMapFragments" />, into which test setup adds synthetic
///             fragments) without polluting other concurrently running tests.
///         </item>
///         <item>
///             Game data sources can in principle be substituted (e.g. for mods or alternate
///             rule sets) without rewriting every call site.
///         </item>
///     </list>
/// </remarks>
public interface IDataProvider
{
    ILoader<Branch> Branches { get; }
    IGroupedLoader<AbilityType, Ability> Abilities { get; }
    IIndexedGroupedLoader<byte, string, Creature> Creatures { get; }
    IGroupedLoader<ItemGroup, Item> Items { get; }
    ISingleLoader<ItemGroup> ItemGroups { get; }
    IIndexedLoader<string, PlayerRace> PlayerRaces { get; }
    ILoader<DefiningMapFragment> DefiningMapFragments { get; }
    ILoader<ConnectingMapFragment> ConnectingMapFragments { get; }
    ILoader<NormalMapFragment> NormalMapFragments { get; }
    ILoader<PropertyDescription> PropertyDescriptions { get; }
}
