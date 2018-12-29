using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Data.Items;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.DataLoading;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation
{
    public class Item : Affectable, ICSScriptSerializable, ILoadable
    {
        private ItemType? _itemType;
        private ItemComplexity? _itemComplexity;
        private Material? _material;
        private int? _weight;
        private SizeCategory? _equipableSizes;
        private EquipmentSlot? _equipableSlots;
        private int? _hindrance;
        private int? _stackSize;
        private bool? _countable;
        private bool? _nameable;
        private int? _capacity;

        public string Name { get; set; }

        public string BaseName { get; set; }
        public Item BaseItem => BaseName == null ? null : Loader.Get(BaseName);

        public Weight GenerationWeight { get; set; }

        public ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();

        public ItemType? Type
        {
            get => _itemType ?? BaseItem?.Type;
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

        public int? Hindrance
        {
            get => _hindrance ?? BaseItem?.Hindrance;
            set => _hindrance = value;
        }

        public int? StackSize
        {
            get => _stackSize ?? BaseItem?.StackSize;
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
                    itemEntityReference.Referenced.Item.Count = quantity;
                }

                var moveMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
                moveMessage.ItemEntity = itemEntityReference.Referenced;
                moveMessage.TargetContainerEntity = container;

                if (!manager.ItemMovingSystem.CanMoveItem(moveMessage, manager))
                {
                    manager.Queue.ReturnMessage(moveMessage);
                    return false;
                }

                itemEntityReference.Dispose();
                manager.Enqueue(moveMessage);
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
                    itemEntityReference.Referenced.Item.Count = quantity;
                }

                var moveMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
                moveMessage.ItemEntity = itemEntityReference.Referenced;
                moveMessage.TargetLevelEntity = levelEntity;
                moveMessage.TargetCell = cell;

                if (!manager.ItemMovingSystem.CanMoveItem(moveMessage, manager))
                {
                    manager.Queue.ReturnMessage(moveMessage);
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
            item.Type = Type ?? ItemType.None;
            item.MaxStackSize = StackSize ?? 1;
            item.EquipableSizes = EquipableSizes ?? SizeCategory.None;
            item.EquipableSlots = EquipableSlots ?? EquipmentSlot.None;
            if (Countable ?? false)
            {
                item.Count = 1;
            }

            itemEntity.Item = item;

            var physical = manager.CreateComponent<PhysicalComponent>(EntityComponent.Physical);
            physical.Material = Material ?? Primitives.Material.Default;

            if ((item.Type & ItemType.Container) != 0)
            {
                physical.Capacity = Capacity ?? 1;
            }
            else if (StackSize > 1)
            {
                physical.Capacity = StackSize - 1;
            }

            itemEntity.Physical = physical;

            using (var appliedEffectEntityReference = manager.CreateEntity())
            {
                var appliedEffectEntity = appliedEffectEntityReference.Referenced;
                var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                appliedEffect.EffectType = EffectType.AddAbility;
                appliedEffect.DurationTicks = (int)EffectDuration.Infinite;

                appliedEffectEntity.Effect = appliedEffect;

                var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                ability.Name = LivingSystem.InnateAbilityName;
                ability.Activation = ActivationType.Always;

                appliedEffectEntity.Ability = ability;

                CreatePropertyEffect(nameof(ItemComponent.Hindrance), Hindrance, appliedEffectEntity.Id, manager);

                ability.OwnerId = itemEntity.Id;
                appliedEffect.AffectedEntityId = itemEntity.Id;
            }

            foreach (var abilityDefinition in Abilities)
            {
                using (var abilityEntityReference = manager.CreateEntity())
                {
                    var abilityEntity = abilityEntityReference.Referenced;
                    var abilityEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                    abilityEffect.EffectType = EffectType.AddAbility;
                    abilityEffect.DurationTicks = (int)EffectDuration.Infinite;

                    abilityEntity.Effect = abilityEffect;

                    var ability = abilityDefinition.AddToEffect(abilityEntity);

                    ability.OwnerId = itemEntity.Id;
                    abilityEffect.AffectedEntityId = itemEntity.Id;

                    if ((abilityDefinition.Activation
                        & (ActivationType.WhileToggled | ActivationType.ManualActivation | ActivationType.Targeted)) != 0)
                    {
                        AddPossesedAbility(ability, item, manager);
                    }
                }
            }

            return item;
        }

        private void AddPossesedAbility(AbilityComponent ability, ItemComponent item, GameManager manager)
        {
            using (var abilityEntityReference = manager.CreateEntity())
            {
                var abilityEntity = abilityEntityReference.Referenced;

                var abilityEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                abilityEffect.EffectType = EffectType.AddAbility;
                abilityEffect.DurationTicks = (int)EffectDuration.Infinite;

                abilityEntity.Effect = abilityEffect;

                var possessedAbility = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                possessedAbility.Name = "Add " + ability.Name + " ability";
                possessedAbility.Activation = ActivationType.WhilePossessed;

                abilityEntity.Ability = possessedAbility;

                using (var activateAbilityEntityReference = manager.CreateEntity())
                {
                    var activateAbilityEntity = activateAbilityEntityReference.Referenced;

                    var activateAbilityEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                    activateAbilityEffect.EffectType = EffectType.AddAbility;
                    activateAbilityEffect.DurationTicks = (int)EffectDuration.Infinite;

                    activateAbilityEntity.Effect = activateAbilityEffect;

                    var activateAbility = ability.AddToEffect(activateAbilityEntity, includeEffects: false);
                    activateAbility.Name = item.TemplateName + ": " + ability.Name;

                    activateAbilityEffect.ContainingAbilityId = abilityEntity.Id;

                    using (var activateEffectEntityReference = manager.CreateEntity())
                    {
                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.Activate;
                        effect.TargetEntityId = ability.EntityId;
                        effect.DurationTicks = (int)((ability.Activation & ActivationType.Continuous) == 0
                            ? EffectDuration.Instant : EffectDuration.Infinite);
                        effect.ContainingAbilityId = activateAbilityEntity.Id;

                        activateEffectEntityReference.Referenced.Effect = effect;
                    }
                }

                possessedAbility.OwnerId = item.EntityId;
                abilityEffect.AffectedEntityId = item.EntityId;
            }
        }

        private Func<string, byte, int, float> _weightFunction;

        public float GetWeight(LevelComponent level)
        {
            if (_weightFunction == null)
            {
                _weightFunction = (GenerationWeight ?? new DefaultWeight()).CreateItemWeightFunction();
            }

            return _weightFunction(level.Branch.Name, level.Depth, 0);
        }

        public static readonly GroupedCSScriptLoader<ItemGroup, Item> Loader =
            new GroupedCSScriptLoader<ItemGroup, Item>(@"Data\Items\", i => ItemGroup.Loader.Object.GetGroups(i),
                typeof(ItemData));

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Item>(GetPropertyConditions<Item>());

        protected static Dictionary<string, Func<TItemVariant, object, bool>> GetPropertyConditions<TItemVariant>()
            where TItemVariant : Item => new Dictionary<string, Func<TItemVariant, object, bool>>
        {
            {nameof(Name), (o, v) => v != null},
            {nameof(BaseName), (o, v) => v != null},
            {nameof(Abilities), (o, v) => ((ICollection<Ability>)v).Count != 0},
            {nameof(Type), (o, v) => (ItemType?)v != o.BaseItem?.Type},
            {nameof(Complexity), (o, v) => (ItemComplexity?)v != o.BaseItem?.Complexity},
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            {
                nameof(GenerationWeight),
                (o, v) => (Weight)v != null && (!(v is DefaultWeight def) || def.Multiplier != 1)
            },
            {nameof(Material), (o, v) => (Material?)v != o.BaseItem?.Material},
            {nameof(Weight), (o, v) => (int?)v != o.BaseItem?.Weight},
            {nameof(Hindrance), (o, v) => (int?)v != o.BaseItem?.Hindrance},
            {nameof(StackSize), (o, v) => (int?)v != o.BaseItem?.StackSize},
            {nameof(Countable), (o, v) => (bool?)v != o.BaseItem?.Countable},
            {nameof(Nameable), (o, v) => (bool?)v != o.BaseItem?.Nameable},
            {nameof(EquipableSizes), (o, v) => (SizeCategory?)v != o.BaseItem?.EquipableSizes},
            {nameof(EquipableSlots), (o, v) => (EquipmentSlot?)v != o.BaseItem?.EquipableSlots},
            {nameof(Capacity), (o, v) => (int?)v != o.BaseItem?.Capacity}
        };

        ICSScriptSerializer ICSScriptSerializable.GetSerializer() => Serializer;
    }
}
