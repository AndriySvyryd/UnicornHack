using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings;

[Component(Id = (int)EntityComponent.Physical)]
public class PhysicalComponent : GameComponent
{
    private Material _material;
    private int _weight;
    private int _size;
    private int _capacity;
    private bool _infravisible;
    private IReadOnlyCollection<GameEntity> _abilities;
    private IReadOnlyCollection<GameEntity> _appliedEffects;
    private IReadOnlyCollection<GameEntity> _items;

    public PhysicalComponent()
    {
        ComponentId = (int)EntityComponent.Physical;
    }

    [Property]
    public Material Material
    {
        get => _material;
        set => SetWithNotify(value, ref _material);
    }

    // units of 100 grams
    [Property(MinValue = 0, DefaultValue = 1)]
    public int Weight
    {
        get => _weight;
        set => SetWithNotify(value, ref _weight);
    }

    // Largest dimension in feet
    [Property(MinValue = 0, DefaultValue = 1)]
    public int Size
    {
        get => _size;
        set => SetWithNotify(value, ref _size);
    }

    [Property(MinValue = 0)]
    public int Capacity
    {
        get => _capacity;
        set => SetWithNotify(value, ref _capacity);
    }

    // TODO: Perf: Store number of items contained

    [Property]
    public bool Infravisible
    {
        get => _infravisible;
        set => SetWithNotify(value, ref _infravisible);
    }

    public IReadOnlyCollection<GameEntity> Abilities
    {
        get
        {
            if (_abilities == null)
            {
                var abilities = Entity.Item?.Abilities
                                ?? Entity.Being?.Abilities;

                if (abilities != null)
                {
                    _abilities = abilities;
                }
                else
                {
                    _abilities = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                    abilities = Entity.Sensor?.Abilities;
                    if (abilities != null)
                    {
                        _abilities = abilities;
                    }
                }
            }

            return _abilities;
        }
    }

    public IReadOnlyCollection<GameEntity> AppliedEffects
    {
        get
        {
            if (_appliedEffects == null)
            {
                var appliedEffects = Entity.Item?.AppliedEffects
                                     ?? Entity.Being?.AppliedEffects;

                if (appliedEffects != null)
                {
                    _appliedEffects = appliedEffects;
                }
                else
                {
                    _appliedEffects = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                    appliedEffects = Entity.Sensor?.AppliedEffects;
                    if (appliedEffects != null)
                    {
                        _appliedEffects = appliedEffects;
                    }
                }
            }

            return _appliedEffects;
        }
    }

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
                    items = Entity.Sensor?.Items;
                    if (items != null)
                    {
                        _items = items;
                    }
                }
            }

            return _items;
        }
    }

    protected override void Clean()
    {
        _material = default;
        _weight = default;
        _size = default;
        _capacity = default;
        _infravisible = default;
        _abilities = default;
        _appliedEffects = default;
        _items = default;

        base.Clean();
    }
}
