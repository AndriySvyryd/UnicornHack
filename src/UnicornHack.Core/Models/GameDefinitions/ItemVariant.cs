using System;
using System.Collections.Generic;
using System.IO;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameDefinitions
{
    public class ItemVariant : ICSScriptSerializable
    {
        public virtual string Name { get; set; }

        public virtual ItemType Type
        {
            get { return _itemType ?? BaseItemVariant?.Type ?? ItemType.None; }
            set { _itemType = value; }
        }

        public ItemVariant BaseItemVariant
            => BaseItemVariantName == null ? null : Get(BaseItemVariantName);

        public virtual string BaseItemVariantName { get; set; }

        /// <summary> 100g units </summary>
        public virtual short Weight
        {
            get { return _weight ?? BaseItemVariant?.Weight ?? 0; }
            set { _weight = value; }
        }

        public virtual short Nutrition
        {
            get { return _nutrition ?? BaseItemVariant?.Nutrition ?? Weight; }
            set { _nutrition = value; }
        }

        // Material

        public virtual bool Nameable
        {
            get { return _nameable ?? BaseItemVariant?.Nameable ?? true; }
            set { _nameable = value; }
        }

        public virtual int StackSize
        {
            get { return _stackSize ?? BaseItemVariant?.StackSize ?? 1; }
            set { _stackSize = value; }
        }

        public virtual Size Size
        {
            get { return _size ?? BaseItemVariant?.Size ?? Size.None; }
            set { _size = value; }
        }

        public virtual Size SizeRestrictions
        {
            get { return _sizeRestrictions ?? BaseItemVariant?.SizeRestrictions ?? Size.None; }
            set { _sizeRestrictions = value; }
        }

        public virtual IList<EquipmentSlot> EquipableSlots { get; set; }
        public virtual IList<Ability> Abilities { get; set; }
        public virtual ISet<string> SimpleProperties { get; set; }
        public virtual IDictionary<string, object> ValuedProperties { get; set; }

        protected virtual void Initialize()
        {
            Abilities = Abilities ?? new List<Ability>();
            SimpleProperties = SimpleProperties ?? new HashSet<string>();
            ValuedProperties = ValuedProperties ?? new Dictionary<string, object>();
            EquipableSlots = EquipableSlots ?? new List<EquipmentSlot>();
        }

        public static readonly string BasePath = Path.Combine(AppContext.BaseDirectory, @"data\items\");
        private static bool _allLoaded;

        private ItemType? _itemType;
        private short? _weight;
        private short? _nutrition;
        private Size? _size;
        private Size? _sizeRestrictions;
        private bool? _nameable;
        private int? _stackSize;

        public static Dictionary<string, ItemVariant> NameLookup { get; } =
            new Dictionary<string, ItemVariant>(StringComparer.Ordinal);

        public static IEnumerable<ItemVariant> GetAllItemVariants()
        {
            if (!_allLoaded)
            {
                foreach (var file in
                    Directory.EnumerateFiles(BasePath, "*" + CSScriptDeserializer.Extension,
                        SearchOption.AllDirectories))
                {
                    if (!NameLookup.ContainsKey(
                        CSScriptDeserializer.GetNameFromFilename(Path.GetFileNameWithoutExtension(file))))
                    {
                        Load(file);
                    }
                }
                _allLoaded = true;
            }

            return NameLookup.Values;
        }

        public static ItemVariant Get(string name)
        {
            ItemVariant variant;
            if (NameLookup.TryGetValue(name, out variant))
            {
                return variant;
            }

            var path = Path.Combine(BasePath, CSScriptDeserializer.GetFilename(name));
            if (!File.Exists(path))
            {
                return null;
            }

            return Load(path);
        }

        private static ItemVariant Load(string path)
        {
            var variant = CSScriptDeserializer.LoadFile<ItemVariant>(path);
            variant.Initialize();
            NameLookup[variant.Name] = variant;
            return variant;
        }

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<ItemVariant>(
            new Dictionary<string, Func<ItemVariant, object>>
            {
                {nameof(Name), o => o.Name},
                {nameof(BaseItemVariantName), o => o.BaseItemVariantName},
                {nameof(Type), o => o.Type},
                {nameof(Weight), o => o.Weight},
                {nameof(Nutrition), o => o.Nutrition},
                {nameof(Nameable), o => o.Nameable},
                {nameof(StackSize), o => o.StackSize},
                {nameof(Size), o => o.Size},
                {nameof(SizeRestrictions), o => o.SizeRestrictions},
                {nameof(EquipableSlots), o => o.EquipableSlots},
                {nameof(Abilities), o => o.Abilities},
                {nameof(SimpleProperties), o => o.SimpleProperties},
                {nameof(ValuedProperties), o => o.ValuedProperties}
            },
            new Func<ItemVariant, object>[0],
            new Dictionary<string, Func<ItemVariant, object>>
            {
                {nameof(Type), o => o.BaseItemVariant?.Type ?? ItemType.None},
                {nameof(Weight), o => o.BaseItemVariant?.Weight ?? 0},
                {nameof(Size), o => o.BaseItemVariant?.Size ?? Size.None},
                {nameof(SizeRestrictions), o => o.BaseItemVariant?.SizeRestrictions ?? Size.None},
                {nameof(Nutrition), o => o.BaseItemVariant?.Nutrition ?? 0},
                {nameof(Nameable), o => o.BaseItemVariant?.Nameable ?? true},
                {nameof(StackSize), o => o.BaseItemVariant?.StackSize ?? 1}
            });

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}