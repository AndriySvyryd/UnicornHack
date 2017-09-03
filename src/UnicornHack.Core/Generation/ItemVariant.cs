using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Data.Items;
using UnicornHack.Effects;
using UnicornHack.Utils;

namespace UnicornHack.Generation
{
    public class ItemVariant : ICSScriptSerializable, ILoadable
    {
        private ItemType? _itemType;
        private Material? _material;
        private SizeCategory? _equipableSizes;
        private bool? _nameable;
        private int? _stackSize;
        private ISet<string> _simpleProperties;
        private IDictionary<string, object> _valuedProperties;
        private EquipmentSlot? _equipableSlots;

        public virtual string Name { get; set; }

        public virtual string BaseName { get; set; }
        public ItemVariant Base => BaseName == null ? null : Loader.Get(BaseName);

        public virtual Weight GenerationWeight { get; set; }

        public virtual ItemType Type
        {
            get => _itemType ?? Base?.Type ?? ItemType.None;
            set => _itemType = value;
        }

        public virtual Material Material
        {
            get => _material ?? Base?.Material ?? Material.Default;
            set => _material = value;
        }

        public virtual bool Nameable
        {
            get => _nameable ?? Base?.Nameable ?? true;
            set => _nameable = value;
        }

        public virtual int StackSize
        {
            get => _stackSize ?? Base?.StackSize ?? 1;
            set => _stackSize = value;
        }

        public virtual SizeCategory EquipableSizes
        {
            get => _equipableSizes ?? Base?.EquipableSizes ?? SizeCategory.All;
            set => _equipableSizes = value;
        }

        public virtual EquipmentSlot EquipableSlots
        {
            get => _equipableSlots ?? Base?.EquipableSlots ?? EquipmentSlot.Default;
            set => _equipableSlots = value;
        }

        public virtual ISet<AbilityDefinition> Abilities { get; set; } = new HashSet<AbilityDefinition>();

        public virtual ISet<string> SimpleProperties
        {
            get
            {
                if (_simpleProperties != null)
                {
                    return _simpleProperties;
                }
                if (Base != null)
                {
                    return Base.SimpleProperties;
                }
                return _simpleProperties = new HashSet<string>();
            }
            set => _simpleProperties = value;
        }

        public virtual IDictionary<string, object> ValuedProperties
        {
            get
            {
                if (_valuedProperties != null)
                {
                    return _valuedProperties;
                }
                if (Base != null)
                {
                    return Base.ValuedProperties;
                }
                return _valuedProperties = new Dictionary<string, object>();
            }
            set => _valuedProperties = value;
        }

        public virtual Item Instantiate(Game game)
        {
            var itemInstance = CreateInstance(game);
            itemInstance.BaseName = Name;
            itemInstance.Type = Type;
            itemInstance.Material = Material;
            itemInstance.Nameable = Nameable;
            itemInstance.StackSize = StackSize;
            itemInstance.EquipableSizes = EquipableSizes;
            itemInstance.EquipableSlots = EquipableSlots;

            var innateAbility =
                new AbilityDefinition(game) {Name = Actor.InnateAbilityName, Activation = AbilityActivation.Always};

            foreach (var simpleProperty in SimpleProperties)
            {
                innateAbility.Effects.Add(
                    new ChangeProperty<bool>(game) {PropertyName = simpleProperty, Value = true});
            }

            foreach (var valuedProperty in ValuedProperties)
            {
                innateAbility.Effects.Add(
                    Effect.CreateChangeProperty(game, valuedProperty.Key, valuedProperty.Value));
            }

            foreach (var ability in Abilities)
            {
                itemInstance.Add(ability.Instantiate(game));
            }

            return itemInstance;
        }

        public virtual IReadOnlyList<Item> Instantiate(IItemLocation location, int? quantity = null)
        {
            var items = new List<Item>();
            quantity = quantity ?? 1;
            for (var i = 0; i < quantity; i++)
            {
                var item = Instantiate(location.Game);
                items.Add(item);
                if (!item.MoveTo(location))
                {
                    foreach (var createdItem in items)
                    {
                        createdItem.Remove();
                    }
                    return new List<Item>();
                }
            }

            return items;
        }

        protected virtual Item CreateInstance(Game game) => new Item(game);

        private Func<string, byte, int, float> _weightFunction;

        public virtual float GetWeight(Level level)
        {
            if (_weightFunction == null)
            {
                _weightFunction = (GenerationWeight ?? new DefaultWeight()).CreateItemWeightFunction();
            }

            return _weightFunction(level.Branch.Name, level.Depth, 0);
        }

        public static readonly GroupedCSScriptLoader<ItemGroup, ItemVariant> Loader =
            new GroupedCSScriptLoader<ItemGroup, ItemVariant>(@"Data\Items\", i => ItemGroup.Loader.Object.GetGroups(i),
                typeof(ItemVariantData));

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<ItemVariant>(GetPropertyConditions<ItemVariant>());

        protected static Dictionary<string, Func<TItemVariant, object, bool>> GetPropertyConditions<TItemVariant>()
            where TItemVariant : ItemVariant
        {
            return new Dictionary<string, Func<TItemVariant, object, bool>>
            {
                {nameof(Name), (o, v) => v != null},
                {nameof(BaseName), (o, v) => v != null},
                {nameof(Type), (o, v) => (ItemType)v != (o.Base?.Type ?? ItemType.None)},
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                {
                    nameof(GenerationWeight),
                    (o, v) => (Weight)v != null && (!(v is DefaultWeight def) || def.Multiplier != 1)
                },
                {nameof(Material), (o, v) => (Material)v != (o.Base?.Material ?? Material.Default)},
                {nameof(Nameable), (o, v) => (bool)v != (o.Base?.Nameable ?? true)},
                {nameof(StackSize), (o, v) => (int)v != (o.Base?.StackSize ?? 1)},
                {nameof(EquipableSizes), (o, v) => (SizeCategory)v != (o.Base?.EquipableSizes ?? SizeCategory.All)},
                {
                    nameof(EquipableSlots),
                    (o, v) => (EquipmentSlot)v != (o.Base?.EquipableSlots ?? EquipmentSlot.Default)
                },
                {nameof(Abilities), (o, v) => ((ICollection<AbilityDefinition>)v).Count != 0},
                {nameof(SimpleProperties), (o, v) => ((ICollection<string>)v).Count != 0},
                {nameof(ValuedProperties), (o, v) => ((IDictionary<string, object>)v).Keys.Count != 0}
            };
        }

        public virtual ICSScriptSerializer GetSerializer() => Serializer;
    }
}