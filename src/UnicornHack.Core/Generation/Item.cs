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

public class Item : Affectable, ICSScriptSerializable, ILoadable
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

    public ISet<Ability> Abilities
    {
        get;
        set;
    } = new HashSet<Ability>();

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
        get => _weight ?? BaseItem?.Weight;
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
                    moveMessage.TargetLevelEntity = position.LevelEntity;
                    moveMessage.TargetCell = position.LevelCell;
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

    public bool Instantiate(LevelComponent level, Point cell, int quantity = 1)
    {
        var levelEntity = level.Entity;
        var manager = levelEntity.Manager;
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
            moveMessage.TargetLevelEntity = levelEntity;
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

        var capacity = 0;
        if ((item.Type & ItemType.Container) != 0)
        {
            capacity = Capacity ?? ItemMovingSystem.DefaultInventorySize;
        }
        else if (StackSize > 1)
        {
            capacity = StackSize.Value - 1;
        }

        itemEntity.Physical = physical;

        using (var innateAbilityReference = manager.CreateEntity())
        {
            var innateAbilityEntity = innateAbilityReference.Referenced;
            var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            appliedEffect.EffectType = EffectType.AddAbility;
            appliedEffect.Duration = EffectDuration.Infinite;

            innateAbilityEntity.Effect = appliedEffect;

            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = EffectApplicationSystem.InnateAbilityName;
            ability.Activation = ActivationType.Always;
            ability.SuccessCondition = AbilitySuccessCondition.Always;

            innateAbilityEntity.Ability = ability;

            AddPropertyEffect(nameof(PhysicalComponent.Weight), Weight ?? 0, innateAbilityEntity.Id, manager,
                ValueCombinationFunction.MeanRoundDown);
            AddPropertyEffect(nameof(PhysicalComponent.Material), (int?)(Material ?? Primitives.Material.Default),
                innateAbilityEntity.Id, manager, ValueCombinationFunction.Override);
            AddPropertyEffect(nameof(PhysicalComponent.Capacity), capacity, innateAbilityEntity.Id, manager);

            ability.OwnerEntity = itemEntity;
            appliedEffect.AffectedEntity = itemEntity;
        }

        var activateAdded = false;
        foreach (var abilityDefinition in Abilities)
        {
            using (var abilityEntityReference = manager.CreateEntity())
            {
                var abilityEntity = abilityEntityReference.Referenced;
                var abilityEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                abilityEffect.EffectType = EffectType.AddAbility;
                abilityEffect.Duration = EffectDuration.Infinite;

                abilityEntity.Effect = abilityEffect;

                var ability = abilityDefinition.AddToEffect(abilityEntity);

                ability.OwnerEntity = itemEntity;
                abilityEffect.AffectedEntity = itemEntity;

                if ((abilityDefinition.Activation & ActivationType.Slottable) != 0)
                {
                    if (abilityDefinition.ItemCondition == ActivationType.Default
                        || abilityDefinition.ItemCondition == ActivationType.WhilePossessed)
                    {
                        if (activateAdded)
                        {
                            throw new InvalidOperationException(
                                $"Item {Name} has more than 1 activated itemAbility.");
                        }

                        activateAdded = true;
                    }

                    AddPossessedAbility(ability, abilityDefinition.ItemCondition, item, manager);
                }
            }
        }

        if (!activateAdded)
        {
            AddPossessedAbility(itemAbility: null, ActivationType.Default, item, manager);
        }

        return item;
    }

    private void AddPossessedAbility(
        AbilityComponent? itemAbility, ActivationType itemCondition, ItemComponent item, GameManager manager)
    {
        using var abilityEntityReference = manager.CreateEntity();
        var possessedAbilityEntity = abilityEntityReference.Referenced;

        var possessedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        possessedEffect.EffectType = EffectType.AddAbility;
        possessedEffect.Duration = EffectDuration.Infinite;

        possessedAbilityEntity.Effect = possessedEffect;

        var possessedAbility = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        possessedAbility.Name = "Add " + (itemAbility?.Name ?? item.TemplateName) + " ability";
        possessedAbility.Activation = itemCondition == ActivationType.Default
            ? ActivationType.WhilePossessed
            : itemCondition;

        possessedAbilityEntity.Ability = possessedAbility;

        using (var addUseAbilityEntityReference = manager.CreateEntity())
        {
            var addUseAbilityEntity = addUseAbilityEntityReference.Referenced;

            var addUseAbilityEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            addUseAbilityEffect.EffectType = EffectType.AddAbility;
            addUseAbilityEffect.Duration = EffectDuration.Infinite;

            addUseAbilityEntity.Effect = addUseAbilityEffect;
            if (itemAbility != null)
            {
                var activateAbility = itemAbility.AddToEffect(addUseAbilityEntity, includeEffects: false);
                activateAbility.Name = item.TemplateName + ": " + itemAbility.Name;
                if (possessedAbility.Activation == ActivationType.WhilePossessed)
                {
                    activateAbility.Type = AbilityType.Item;
                }

                addUseAbilityEffect.ContainingAbilityId = possessedAbilityEntity.Id;

                using var activateEffectEntityReference = manager.CreateEntity();
                var activateEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                activateEffect.EffectType = EffectType.Activate;
                activateEffect.TargetEntityId = itemAbility.EntityId;
                activateEffect.Duration = (itemAbility.Activation & ActivationType.Continuous) == 0
                    ? EffectDuration.Instant
                    : EffectDuration.Infinite;
                activateEffect.ContainingAbilityId = addUseAbilityEntity.Id;

                activateEffectEntityReference.Referenced.Effect = activateEffect;
            }
            else
            {
                var equip = item.EquipableSlots != EquipmentSlot.None;
                var useAbility = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                useAbility.Name = item.TemplateName + ": " + (equip ? "Equip" : "Drop");
                useAbility.Type = AbilityType.Item;
                useAbility.Activation = ActivationType.Manual;

                addUseAbilityEntity.Ability = useAbility;

                addUseAbilityEffect.ContainingAbilityId = possessedAbilityEntity.Id;

                using var useEffectEntityReference = manager.CreateEntity();
                var useEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                useEffect.EffectType = equip ? EffectType.EquipItem : EffectType.Move;
                useEffect.TargetEntityId = item.EntityId;
                useEffect.ContainingAbilityId = addUseAbilityEntity.Id;

                useEffectEntityReference.Referenced.Effect = useEffect;
            }
        }

        possessedAbility.OwnerEntity = item.Entity;
        possessedEffect.AffectedEntity = item.Entity;
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
        new(new[] { BranchParameter, DepthParameter, InstancesParameter });

    public static Func<string, int, int, float> CreateWeightFunction(string expression)
        => _translator.Translate<Func<string, int, int, float>, float>(expression);

    public float GetWeight(LevelComponent level)
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
            return _weightFunction(level.Branch.Name, level.Depth, 0);
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
        { nameof(Abilities), (_, v) => ((ICollection<Ability>)v!).Count != 0 },
        { nameof(Type), (o, v) => (ItemType?)v != (o.BaseItem?.Type ?? ItemType.None) },
        { nameof(Complexity), (o, v) => (ItemComplexity?)v != o.BaseItem?.Complexity },
        { nameof(GenerationWeight), (_, v) => v != null && (string)v != DefaultGenerationWeight },
        { nameof(Material), (o, v) => (Material?)v != o.BaseItem?.Material },
        { nameof(Weight), (o, v) => (int?)v != o.BaseItem?.Weight },
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
