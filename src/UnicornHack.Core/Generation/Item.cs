using System.Linq.Expressions;
using CSharpScriptSerialization;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class Item : ICSScriptSerializable, ILoadable
{
    private ItemType? _itemType;
    private ItemComplexity? _itemComplexity;
    private Material? _material;
    private int? _weight;
    private SizeCategory? _equipableSizes;
    private EquipmentSlot? _equipableSlots;
    private int? _stackSize;
    private bool? _countable;
    private bool? _nameable;
    private int? _capacity;
    private int? _requiredFocus;
    private int? _requiredMight;
    private int? _requiredPerception;
    private int? _requiredSpeed;

    public string Name
    {
        get;
        set;
    } = null!;

    public string? BaseName
    {
        get;
        set;
    }

    public Item? BaseItem => BaseName == null ? null : Loader.Get(BaseName);

    private string? _generationWeight;

    public string? GenerationWeight
    {
        get => _generationWeight;
        set
        {
            _generationWeight = value;
            _weightFunction = null;
        }
    }

    public List<Ability> Abilities
    {
        get;
        set;
    } = new List<Ability>();

    public ItemType? Type
    {
        get => _itemType ?? BaseItem?.Type ?? ItemType.None;
        set => _itemType = value;
    }

    public ItemComplexity? Complexity
    {
        get => _itemComplexity ?? BaseItem?.Complexity;
        set => _itemComplexity = value;
    }

    public Material? Material
    {
        get => _material ?? BaseItem?.Material;
        set => _material = value;
    }

    public int? Weight
    {
        get => _weight ?? BaseItem?.Weight ?? 1;
        set => _weight = value;
    }

    public int? StackSize
    {
        get => _stackSize ?? BaseItem?.StackSize ?? 1;
        set => _stackSize = value;
    }

    public bool? Countable
    {
        get => _countable ?? BaseItem?.Countable;
        set => _countable = value;
    }

    public bool? Nameable
    {
        get => _nameable ?? BaseItem?.Nameable;
        set => _nameable = value;
    }

    public SizeCategory? EquipableSizes
    {
        get => _equipableSizes ?? BaseItem?.EquipableSizes;
        set => _equipableSizes = value;
    }

    public EquipmentSlot? EquipableSlots
    {
        get => _equipableSlots ?? BaseItem?.EquipableSlots;
        set => _equipableSlots = value;
    }

    public int? Capacity
    {
        get => _capacity ?? BaseItem?.Capacity;
        set => _capacity = value;
    }

    public int? RequiredFocus
    {
        get => _requiredFocus ?? BaseItem?.RequiredFocus;
        set => _requiredFocus = value;
    }

    public int? RequiredMight
    {
        get => _requiredMight ?? BaseItem?.RequiredMight;
        set => _requiredMight = value;
    }

    public int? RequiredPerception
    {
        get => _requiredPerception ?? BaseItem?.RequiredPerception;
        set => _requiredPerception = value;
    }

    public int? RequiredSpeed
    {
        get => _requiredSpeed ?? BaseItem?.RequiredSpeed;
        set => _requiredSpeed = value;
    }

    public bool Instantiate(GameEntity container, int quantity = 1)
    {
        var manager = container.Manager;
        var countable = Countable ?? false;
        var instanceCount = countable ? 1 : quantity;
        for (var i = 0; i < instanceCount; i++)
        {
            var itemEntityReference = Instantiate(manager);
            if (countable)
            {
                itemEntityReference.Referenced.Item!.Count = quantity;
            }

            var moveMessage = MoveItemMessage.Create(manager);
            moveMessage.ItemEntity = itemEntityReference.Referenced;
            moveMessage.TargetContainerEntity = container;

            if (!manager.ItemMovingSystem.CanMoveItem(moveMessage, manager))
            {
                var position = container.Position;
                if (position != null)
                {
                    moveMessage.TargetRoomEntity = position.RoomEntity;
                    moveMessage.TargetCell = position.Cell;
                    moveMessage.TargetContainerEntity = null;
                    if (!manager.ItemMovingSystem.CanMoveItem(moveMessage, manager))
                    {
                        manager.ReturnMessage(moveMessage);
                        return false;
                    }
                }
                else
                {
                    manager.ReturnMessage(moveMessage);
                    return false;
                }
            }

            itemEntityReference.Dispose();
            manager.Process(moveMessage);
        }

        return true;
    }

    public bool Instantiate(RoomComponent room, Point cell, int quantity = 1)
    {
        var roomEntity = room.Entity;
        var manager = roomEntity.Manager;
        var countable = Countable ?? false;
        var instanceCount = countable ? 1 : quantity;
        for (var i = 0; i < instanceCount; i++)
        {
            var itemEntityReference = Instantiate(manager);
            if (countable)
            {
                itemEntityReference.Referenced.Item!.Count = quantity;
            }

            var moveMessage = MoveItemMessage.Create(manager);
            moveMessage.ItemEntity = itemEntityReference.Referenced;
            moveMessage.TargetRoomEntity = roomEntity;
            moveMessage.TargetCell = cell;

            if (!manager.ItemMovingSystem.CanMoveItem(moveMessage, manager))
            {
                manager.ReturnMessage(moveMessage);
                return false;
            }

            itemEntityReference.Dispose();
            manager.Enqueue(moveMessage);
        }

        return true;
    }

    public ITransientReference<GameEntity> Instantiate(GameManager manager)
    {
        var itemEntityReference = manager.CreateEntity();

        AddToEntity(itemEntityReference.Referenced);

        return itemEntityReference;
    }

    public ItemComponent AddToEntity(GameEntity itemEntity)
    {
        var manager = itemEntity.Manager;
        var item = manager.CreateComponent<ItemComponent>(EntityComponent.Item);
        item.TemplateName = Name;
        item.Type = Type!.Value;
        item.MaxStackSize = StackSize!.Value;
        item.EquipableSizes = EquipableSizes ?? SizeCategory.None;
        item.EquipableSlots = EquipableSlots ?? EquipmentSlot.None;
        if (Countable ?? false)
        {
            item.Count = 1;
        }

        itemEntity.Item = item;

        var physical = manager.CreateComponent<PhysicalComponent>(EntityComponent.Physical);

        itemEntity.Physical = physical;

        itemEntity.ActiveEffects =
            manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);

        using (var innateAbilityReference = manager.CreateEntity())
        {
            var innateAbilityEntity = innateAbilityReference.Referenced;
            var abilityEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            abilityEffect.EffectType = EffectType.AddAbility;
            abilityEffect.CombinationFunction = ValueCombinationFunction.Sum;
            abilityEffect.Duration = EffectDuration.Infinite;

            innateAbilityEntity.Effect = abilityEffect;

            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = EffectApplicationSystem.InnateAbilityName;
            ability.Activation = ActivationType.Always;
            ability.SuccessCondition = AbilitySuccessCondition.Always;
            abilityEffect.TargetName = ability.Name;

            innateAbilityEntity.Ability = ability;

            ApplyBaseStats(itemEntity, manager);

            ability.OwnerEntity = itemEntity;
        }

        var possessionAbilityAdded = false;
        foreach (var abilityDefinition in Abilities)
        {
            using (var abilityEntityReference = manager.CreateEntity())
            {
                var abilityEntity = abilityEntityReference.Referenced;
                var abilityEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                abilityEffect.EffectType = EffectType.AddAbility;
                abilityEffect.CombinationFunction = ValueCombinationFunction.Sum;
                abilityEffect.Duration = EffectDuration.Infinite;

                abilityEntity.Effect = abilityEffect;

                var ability = abilityDefinition.AddToEffect(abilityEntity)!;
                abilityEffect.TargetName = ability.Name;

                ability.OwnerEntity = itemEntity;

                if ((abilityDefinition.Activation & ActivationType.Slottable) != 0
                    && item.EquipableSlots == EquipmentSlot.None)
                {
                    if (possessionAbilityAdded)
                    {
                        throw new InvalidOperationException(
                            $"Item {Name} has more than 1 activated itemAbility.");
                    }

                    possessionAbilityAdded = true;
                }
            }
        }

        if (!possessionAbilityAdded)
        {
            EnsureUseAbility(item, manager);
        }

        return item;
    }

    /// <summary>
    ///     Adds this item template's innate physical stats as the entity's first modifier instances.
    /// </summary>
    public void ApplyBaseStats(GameEntity itemEntity, GameManager manager)
    {
        var item = itemEntity.Item!;
        var activeEffects = itemEntity.ActiveEffects!;

        var capacity = 0;
        if ((item.Type & ItemType.Container) != 0)
        {
            capacity = Capacity ?? ItemMovingSystem.DefaultInventorySize;
        }
        else if (StackSize > 1)
        {
            capacity = StackSize.Value - 1;
        }

        activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty, nameof(PhysicalComponent.Weight),
            ValueCombinationFunction.MeanRoundDown,
            sourceAbilityType: AbilityType.Innate,
            sourceAbilityActivation: ActivationType.Always,
            magnitudePrimary: Weight ?? 1);
        activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty, nameof(PhysicalComponent.Material),
            ValueCombinationFunction.Override,
            sourceAbilityType: AbilityType.Innate,
            sourceAbilityActivation: ActivationType.Always,
            magnitudePrimary: (int)(Material ?? Primitives.Material.Default));
        if (capacity != 0)
        {
            activeEffects.AddPropertyEffect(
                EffectType.ChangeProperty, nameof(PhysicalComponent.Capacity),
                ValueCombinationFunction.Sum,
                sourceAbilityType: AbilityType.Innate,
                sourceAbilityActivation: ActivationType.Always,
                magnitudePrimary: capacity);
        }

    }

    private static GameEntity EnsureUseAbility(ItemComponent item, GameManager manager)
    {
        var abilityName = item.EquipableSlots != EquipmentSlot.None ? "Equip" : "Drop";
        foreach (var existingEntity in item.Abilities)
        {
            if (existingEntity.Ability is { Type: AbilityType.Item } existingAbility
                && existingAbility.Name == abilityName)
            {
                return existingEntity;
            }
        }

        using var abilityEntityReference = manager.CreateEntity();
        var abilityEntity = abilityEntityReference.Referenced;
        var equip = item.EquipableSlots != EquipmentSlot.None;
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = abilityName;
        ability.Type = AbilityType.Item;
        ability.Activation = ActivationType.Manual;
        ability.OwnerEntity = item.Entity;
        abilityEntity.Ability = ability;

        using var effectEntityReference = manager.CreateEntity();
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = equip ? EffectType.EquipItem : EffectType.Move;
        effect.TargetEntityId = item.EntityId;
        effect.ContainingAbilityId = abilityEntity.Id;
        effectEntityReference.Referenced.Effect = effect;

        return abilityEntity;
    }

    private Func<string, int, int, float>? _weightFunction;

    protected static readonly string DefaultGenerationWeight = "1.0";

    protected static readonly ParameterExpression BranchParameter =
        Expression.Parameter(typeof(string), name: "branch");

    protected static readonly ParameterExpression DepthParameter =
        Expression.Parameter(typeof(int), name: "depth");

    protected static readonly ParameterExpression InstancesParameter =
        Expression.Parameter(typeof(int), name: "instances");

    private static readonly UnicornExpressionVisitor _translator =
        new([BranchParameter, DepthParameter, InstancesParameter]);

    public static Func<string, int, int, float> CreateWeightFunction(string expression)
        => _translator.Translate<Func<string, int, int, float>, float>(expression);

    public float GetWeight(RoomComponent room)
    {
        if (_weightFunction == null)
        {
            try
            {
                _weightFunction = CreateWeightFunction(GenerationWeight ?? DefaultGenerationWeight);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while parsing the GenerationWeight for " + Name, e);
            }
        }

        try
        {
            return _weightFunction(room.Level.Branch.Name, room.Level.Depth, 0);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
        }
    }

    public static readonly GroupedCSScriptLoader<ItemGroup, Item> Loader =
        new(@"Data\Items\", i => ItemGroup.Loader.Object.GetGroups(i), typeof(ItemData));

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Item>(GetPropertyConditions<Item>());

    protected static Dictionary<string, Func<TItemVariant, object?, bool>> GetPropertyConditions<TItemVariant>()
        where TItemVariant : Item => new()
    {
        { nameof(Name), (_, v) => v != null },
        { nameof(BaseName), (_, v) => v != null },
        { nameof(Abilities), (_, v) => ((IReadOnlyList<Ability>)v!).Count != 0 },
        { nameof(Type), (o, v) => (ItemType?)v != (o.BaseItem?.Type ?? ItemType.None) },
        { nameof(Complexity), (o, v) => (ItemComplexity?)v != o.BaseItem?.Complexity },
        { nameof(GenerationWeight), (_, v) => v != null && (string)v != DefaultGenerationWeight },
        { nameof(Material), (o, v) => (Material?)v != o.BaseItem?.Material },
        { nameof(Weight), (o, v) => (int?)v != (o.BaseItem?.Weight ?? 1) },
        { nameof(StackSize), (o, v) => (int?)v != (o.BaseItem?.StackSize ?? 1) },
        { nameof(Countable), (o, v) => (bool?)v != o.BaseItem?.Countable },
        { nameof(Nameable), (o, v) => (bool?)v != o.BaseItem?.Nameable },
        { nameof(EquipableSizes), (o, v) => (SizeCategory?)v != o.BaseItem?.EquipableSizes },
        { nameof(EquipableSlots), (o, v) => (EquipmentSlot?)v != o.BaseItem?.EquipableSlots },
        { nameof(Capacity), (o, v) => (int?)v != o.BaseItem?.Capacity },
        { nameof(RequiredFocus), (o, v) => (int?)v != o.BaseItem?.RequiredFocus },
        { nameof(RequiredMight), (o, v) => (int?)v != o.BaseItem?.RequiredMight },
        { nameof(RequiredPerception), (o, v) => (int?)v != o.BaseItem?.RequiredPerception },
        { nameof(RequiredSpeed), (o, v) => (int?)v != o.BaseItem?.RequiredSpeed }
    };

    ICSScriptSerializer ICSScriptSerializable.GetSerializer() => Serializer;
}
