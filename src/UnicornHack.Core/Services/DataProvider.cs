using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Services;

/// <summary>
///     Default <see cref="IDataProvider" /> that exposes the static loader singletons
///     directly. Subclasses can override individual properties to substitute isolated
///     loaders — most commonly <see cref="DefiningMapFragments" /> in tests that register
///     synthetic fragments.
/// </summary>
public class DataProvider : IDataProvider
{
    /// <summary>Shared default instance used by <see cref="GameServices" /> when no other
    /// provider is supplied.</summary>
    public static DataProvider Default { get; } = new();

    public virtual ILoader<Branch> Branches => Branch.Loader;
    public virtual IGroupedLoader<AbilityType, Ability> Abilities => Ability.Loader;
    public virtual IIndexedGroupedLoader<byte, string, Creature> Creatures => Creature.Loader;
    public virtual IGroupedLoader<ItemGroup, Item> Items => Item.Loader;
    public virtual ISingleLoader<ItemGroup> ItemGroups => ItemGroup.Loader;
    public virtual IIndexedLoader<string, PlayerRace> PlayerRaces => PlayerRace.Loader;
    public virtual ILoader<DefiningMapFragment> DefiningMapFragments => DefiningMapFragment.Loader;
    public virtual ILoader<ConnectingMapFragment> ConnectingMapFragments => ConnectingMapFragment.Loader;
    public virtual ILoader<NormalMapFragment> NormalMapFragments => NormalMapFragment.Loader;
    public virtual ILoader<PropertyDescription> PropertyDescriptions => PropertyDescription.Loader;
}
