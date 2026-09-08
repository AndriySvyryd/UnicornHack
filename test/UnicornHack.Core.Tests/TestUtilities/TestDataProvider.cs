using UnicornHack.Services;

namespace UnicornHack;

/// <summary>
///     Per-test <see cref="DataProvider" /> that wraps every production loader in a
///     <see cref="TestLoader{T}" /> (or grouped/single variant). Tests append synthetic
///     entries to any of the loaders via their <c>Add</c> / <c>AddRange</c> methods and can
///     toggle <see cref="TestLoader{T}.IncludeProduction" /> per-loader to exclude production
///     data when they want a hermetic set. All wrappers are isolated per provider instance,
///     so concurrent tests don't pollute each other through the static singletons.
/// </summary>
public sealed class TestDataProvider : DataProvider
{
    public TestDataProvider()
    {
        Branches = new TestLoader<Branch>(Branch.Loader);
        Abilities = new TestGroupedLoader<AbilityType, Ability>(a => a.Type, Ability.Loader);
        Creatures = new TestIndexedGroupedLoader<byte, string, Creature>(
            c => c.InitialLevel, creature => creature.GetRaceAbilityName(), Creature.Loader,
            StringComparer.OrdinalIgnoreCase);
        Items = new TestGroupedLoader<ItemGroup, Item>(
            i => ItemGroup.Loader.Object.GetGroups(i), Item.Loader);
        ItemGroups = new TestSingleLoader<ItemGroup>(ItemGroup.Loader);
        PlayerRaces = new TestIndexedLoader<string, PlayerRace>(
            race => race.GetRaceAbilityName(), PlayerRace.Loader, StringComparer.OrdinalIgnoreCase);
        DefiningMapFragments = new TestLoader<DefiningMapFragment>(DefiningMapFragment.Loader);
        ConnectingMapFragments = new TestLoader<ConnectingMapFragment>(ConnectingMapFragment.Loader);
        NormalMapFragments = new TestLoader<NormalMapFragment>(NormalMapFragment.Loader);
        PropertyDescriptions = new TestLoader<PropertyDescription>(PropertyDescription.Loader);
    }

    public override TestLoader<Branch> Branches { get; }
    public override TestGroupedLoader<AbilityType, Ability> Abilities { get; }
    public override TestIndexedGroupedLoader<byte, string, Creature> Creatures { get; }
    public override TestGroupedLoader<ItemGroup, Item> Items { get; }
    public override TestSingleLoader<ItemGroup> ItemGroups { get; }
    public override TestIndexedLoader<string, PlayerRace> PlayerRaces { get; }
    public override TestLoader<DefiningMapFragment> DefiningMapFragments { get; }
    public override TestLoader<ConnectingMapFragment> ConnectingMapFragments { get; }
    public override TestLoader<NormalMapFragment> NormalMapFragments { get; }
    public override TestLoader<PropertyDescription> PropertyDescriptions { get; }

    public void AddAbilities(params Ability[] abilities)
    {
        var lookup = Abilities.TestItems.Concat(abilities).ToDictionary(
            ability => ability.Name, StringComparer.OrdinalIgnoreCase);
        AbilityLoader.Prepare(lookup);
        Abilities.AddRange(abilities);
    }
}
