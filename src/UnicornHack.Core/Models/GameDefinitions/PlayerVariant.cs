using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameDefinitions
{
    public class PlayerVariant : ActorVariant, ICSScriptSerializable
    {
        public CreatureVariant BaseCreatureVariant
            => BaseCreatureVariantName == null ? null : CreatureVariant.Get(BaseCreatureVariantName);

        public virtual string BaseCreatureVariantName { get; set; }

        public override Species Species
        {
            get { return _species ?? BaseCreatureVariant?.Species ?? Species.Default; }
            set { _species = value; }
        }

        public override SpeciesClass SpeciesClass
        {
            get { return _speciesClass ?? BaseCreatureVariant?.SpeciesClass ?? SpeciesClass.None; }
            set { _speciesClass = value; }
        }

        public override byte MovementRate
        {
            get { return _movementRate ?? BaseCreatureVariant?.MovementRate ?? 0; }
            set { _movementRate = value; }
        }

        public override Size Size
        {
            get { return _size ?? BaseCreatureVariant?.Size ?? 0; }
            set { _size = value; }
        }

        public override short Weight
        {
            get { return _weight ?? BaseCreatureVariant?.Weight ?? 0; }
            set { _weight = value; }
        }

        public override short Nutrition
        {
            get { return _nutrition ?? BaseCreatureVariant?.Nutrition ?? 0; }
            set { _nutrition = value; }
        }

        public override ISet<string> SimpleProperties { get; set; }
        public override IDictionary<string, object> ValuedProperties { get; set; }

        public virtual int StrengthBonus { get; set; }
        public virtual int DexterityBonus { get; set; }
        public virtual int ConstitutionBonus { get; set; }
        public virtual int IntelligenceBonus { get; set; }
        public virtual int WillpowerBonus { get; set; }
        public virtual int SpeedBonus { get; set; }
        public virtual IDictionary<Skill, int> SkillAptitudes { get; set; }

        protected override void Initialize()
        {
            SkillAptitudes = SkillAptitudes ?? new ConcurrentDictionary<Skill, int>();
            base.Initialize();
        }

        public static readonly string BasePath = Path.Combine(AppContext.BaseDirectory, @"data\players\");
        private static bool _allLoaded;
        public static readonly int StartingAttributeValue = 10;
        private Species? _species;
        private SpeciesClass? _speciesClass;
        private byte? _movementRate;
        private Size? _size;
        private short? _weight;
        private short? _nutrition;

        public static Dictionary<string, PlayerVariant> NameLookup { get; } =
            new Dictionary<string, PlayerVariant>(StringComparer.Ordinal);

        public static IEnumerable<PlayerVariant> GetAllPlayerVariants()
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

        public static PlayerVariant Get(string name)
        {
            PlayerVariant variant;
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

        private static PlayerVariant Load(string path)
        {
            var variant = CSScriptDeserializer.LoadFile<PlayerVariant>(path);
            variant.Initialize();
            NameLookup[variant.Name] = variant;
            return variant;
        }

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<PlayerVariant>(
            new Dictionary<string, Func<PlayerVariant, object>>
            {
                {nameof(Name), o => o.Name},
                {nameof(BaseCreatureVariantName), o => o.BaseCreatureVariantName},
                {nameof(Species), o => o.Species},
                {nameof(SpeciesClass), o => o.SpeciesClass},
                {nameof(Weight), o => o.Weight},
                {nameof(Size), o => o.Size},
                {nameof(MovementRate), o => o.MovementRate},
                {nameof(Nutrition), o => o.Nutrition},
                {nameof(Abilities), o => o.Abilities},
                {nameof(SimpleProperties), o => o.SimpleProperties},
                {nameof(ValuedProperties), o => o.ValuedProperties},
                {nameof(StrengthBonus), o => o.StrengthBonus},
                {nameof(DexterityBonus), o => o.DexterityBonus},
                {nameof(ConstitutionBonus), o => o.ConstitutionBonus},
                {nameof(IntelligenceBonus), o => o.IntelligenceBonus},
                {nameof(WillpowerBonus), o => o.WillpowerBonus},
                {nameof(SpeedBonus), o => o.SpeedBonus},
                {nameof(SkillAptitudes), o => o.SkillAptitudes}
            },
            new Func<PlayerVariant, object>[0],
            new Dictionary<string, Func<PlayerVariant, object>>
            {
                {nameof(Species), o => o.BaseCreatureVariant?.Species ?? Species.Default},
                {nameof(SpeciesClass), o => o.BaseCreatureVariant?.SpeciesClass ?? SpeciesClass.None},
                {nameof(Weight), o => o.BaseCreatureVariant?.Weight ?? 0},
                {nameof(Size), o => o.BaseCreatureVariant?.Size ?? Size.None},
                {nameof(MovementRate), o => o.BaseCreatureVariant?.MovementRate ?? 0},
                {nameof(Nutrition), o => o.BaseCreatureVariant?.Nutrition ?? 0}
            });

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}