using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

[Component(Id = (int)EntityComponent.Item)]
public class ItemComponent : GameComponent
{
    private GameEntity _containerEntity;
    private int? _containerId;
    private ItemType _type;
    private string _templateName;
    private string _name;
    private SizeCategory _equipableSizes;
    private EquipmentSlot _equipableSlots;
    private EquipmentSlot _equippedSlot;
    private int _maxStackSize;
    private int? _count;
    private IReadOnlyCollection<GameEntity> _abilities;
    private IReadOnlyCollection<GameEntity> _appliedEffects;
    private IReadOnlyCollection<GameEntity> _items;

    public ItemComponent()
    {
        ComponentId = (int)EntityComponent.Item;
    }

    public int? ContainerId
    {
        get => _containerId;
        set
        {
            _containerEntity = null;
            SetWithNotify(value, ref _containerId);
        }
    }

    public ItemType Type
    {
        get => _type;
        set => SetWithNotify(value, ref _type);
    }

    public string TemplateName
    {
        get => _templateName;
        set => SetWithNotify(value, ref _templateName);
    }

    public string Name
    {
        get => _name;
        set => SetWithNotify(value, ref _name);
    }

    public SizeCategory EquipableSizes
    {
        get => _equipableSizes;
        set => SetWithNotify(value, ref _equipableSizes);
    }

    public EquipmentSlot EquipableSlots
    {
        get => _equipableSlots;
        set => SetWithNotify(value, ref _equipableSlots);
    }

    public EquipmentSlot EquippedSlot
    {
        get => _equippedSlot;
        set => SetWithNotify(value, ref _equippedSlot);
    }

    public int MaxStackSize
    {
        get => _maxStackSize;
        set => SetWithNotify(value, ref _maxStackSize);
    }

    public int? Count
    {
        get => _count;
        set => SetWithNotify(value, ref _count);
    }

    public GameEntity ContainerEntity
    {
        get => _containerEntity ??= Entity.Manager.FindEntity(_containerId);
        set
        {
            _containerEntity = value;
            ContainerId = value?.Id;
        }
    }

    public IReadOnlyCollection<GameEntity> Abilities
    {
        get
        {
            if (_abilities == null)
            {
                var abilities = Entity.Being?.Abilities;
                if (abilities != null)
                {
                    _abilities = abilities;
                }
                else
                {
                    _abilities = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                    abilities = Entity.Physical?.Abilities
                                ?? Entity.Sensor?.Abilities;
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
                var appliedEffects = Entity.Being?.AppliedEffects;
                if (appliedEffects != null)
                {
                    _appliedEffects = appliedEffects;
                }
                else
                {
                    _appliedEffects = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                    appliedEffects = Entity.Physical?.AppliedEffects
                                     ?? Entity.Sensor?.AppliedEffects;
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
                var items = Entity.Being?.Items;

                if (items != null)
                {
                    _items = items;
                }
                else
                {
                    _items = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                    items = Entity.Physical?.Items
                            ?? Entity.Sensor?.Items;
                    if (items != null)
                    {
                        _items = items;
                    }
                }
            }

            return _items;
        }
    }

    public int GetQuantity()
        => Count ?? ((Entity.Physical?.Capacity ?? 0) == 0
            ? 1
            : Entity.Physical.Items.Count + 1);

    protected override void Clean()
    {
        _containerId = default;
        _type = default;
        _templateName = default;
        _name = default;
        _equipableSizes = default;
        _equipableSlots = default;
        _equippedSlot = default;
        _maxStackSize = default;
        _count = default;
        _abilities = default;
        _appliedEffects = default;
        _items = default;

        base.Clean();
    }
}
