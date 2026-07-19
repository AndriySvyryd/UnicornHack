using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings;

[Component(Id = (int)EntityComponent.Physical)]
public class PhysicalComponent : GameComponent
{
    private IReadOnlyCollection<GameEntity>? _items;

    public PhysicalComponent()
    {
        ComponentId = (int)EntityComponent.Physical;
    }

    [Property<Material>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public Material Material => (Material)Entity.ActiveEffects!.ResolveInt(nameof(Material));

    // units of 100 grams
    [Property<int>(MinValue = 0, DefaultValue = 1,
        DefaultCombinationFunction = ValueCombinationFunction.MeanRoundDown)]
    public int Weight => Entity.ActiveEffects!.ResolveInt(nameof(Weight));

    // Largest dimension in feet
    [Property<int>(MinValue = 0, DefaultValue = 1,
        DefaultCombinationFunction = ValueCombinationFunction.MeanRoundDown)]
    public int Size => Entity.ActiveEffects!.ResolveInt(nameof(Size));

    [Property<int>(MinValue = 0)]
    public int Capacity => Entity.ActiveEffects!.ResolveInt(nameof(Capacity));

    // TODO: Perf: Store number of items contained

    [Property<bool>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool Infravisible => Entity.ActiveEffects!.ResolveBool(nameof(Infravisible));

    public IReadOnlyCollection<GameEntity> Items
    {
        get
        {
            if (_items == null)
            {
                var items = Entity.Item?.Items
                            ?? Entity.Being?.Items;

                if (items != null)
                {
                    _items = items;
                }
                else
                {
                    _items = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                }
            }

            return _items;
        }
    }

    public IReadOnlyCollection<GameEntity> Abilities => Entity.ActiveEffects!.Abilities;

    protected override void Clean()
    {
        _items = default;

        base.Clean();
    }
}
