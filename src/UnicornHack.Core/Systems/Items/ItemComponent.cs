using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Items;

[Component(Id = (int)EntityComponent.Item)]
public class ItemComponent : GameComponent
{
    private GameEntity? _containerEntity;
    private int? _containerId;
    private ItemType _type;
    private string? _templateName;
    private string? _name;
    private SizeCategory _equipableSizes;
    private EquipmentSlot _equipableSlots;
    private EquipmentSlot _equippedSlot;
    private int _maxStackSize;
    private int? _count;
    private IReadOnlyCollection<GameEntity>? _items;

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
        get => _templateName!;
        set => SetWithNotify(value, ref _templateName);
    }

    public string? Name
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

    public GameEntity? ContainerEntity
    {
        get
        {
            _containerEntity ??= Entity.Manager.FindEntity(_containerId);
            Debug.Assert(_containerEntity != null || _containerId == null,
                $"ContainerEntity {_containerId} not loaded");
            return _containerEntity;
        }
        set
        {
            _containerEntity = value;
            ContainerId = value?.Id;
        }
    }

    public IReadOnlyCollection<GameEntity> Items
    {
        get
        {
            if (_items == null)
            {
                // Assign a provisional set before delegating so a sibling component's Items getter that
                // delegates back here (e.g. PhysicalComponent) sees a non-null value and doesn't recurse.
                _items = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                var items = Entity.Being?.Items
                            ?? Entity.Physical?.Items;
                if (items != null)
                {
                    _items = items;
                }
            }

            return _items;
        }
    }

    public IReadOnlyCollection<GameEntity> Abilities => Entity.ActiveEffects!.Abilities;

    public int GetQuantity()
        => Count ?? ((Entity.Physical?.Capacity ?? 0) == 0
            ? 1
            : Entity.Physical!.Items.Count + 1);

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
        _items = default;

        base.Clean();
    }
}
