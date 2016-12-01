using System;
using System.Collections.Generic;
using System.IO;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameDefinitions
{
    public class CreatureVariant : ActorVariant, ICSScriptSerializable
    {
        public byte InitialLevel { get; set; }
        public sbyte ArmorClass { get; set; }

        public byte MagicResistance { get; set; }

        public GenerationFlags GenerationFlags { get; set; }
        public Frequency GenerationFrequency { get; set; }

        public MonsterBehavior Behavior { get; set; }

        public sbyte Alignment { get; set; }
        public ActorNoiseType Noise { get; set; }
        public CreatureVariant Corpse => CorpseVariantName == null ? null : Get(CorpseVariantName);
        public CreatureVariant PreviousStage => PreviousStageName == null ? null : Get(PreviousStageName);
        public CreatureVariant NextStage => NextStageName == null ? null : Get(NextStageName);

        public string CorpseVariantName { get; set; }
        public string PreviousStageName { get; set; }
        public string NextStageName { get; set; }

        public static readonly string BasePath = Path.Combine(AppContext.BaseDirectory, @"data\creatures\");

        protected static Dictionary<string, CreatureVariant> NameLookup { get; } =
            new Dictionary<string, CreatureVariant>(StringComparer.Ordinal);

        private static bool _allLoaded;

        public static IEnumerable<CreatureVariant> GetAllCreatureVariants()
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

        public static CreatureVariant Get(string name)
        {
            CreatureVariant variant;
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

        private static CreatureVariant Load(string path)
        {
            var variant = CSScriptDeserializer.LoadFile<CreatureVariant>(path);
            variant.Initialize();
            NameLookup[variant.Name] = variant;
            return variant;
        }

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<CreatureVariant>(
            new Dictionary<string, Func<CreatureVariant, object>>
            {
                {nameof(Name), o => o.Name},
                {nameof(Species), o => o.Species},
                {nameof(SpeciesClass), o => o.SpeciesClass},
                {nameof(CorpseVariantName), o => o.CorpseVariantName},
                {nameof(PreviousStageName), o => o.PreviousStageName},
                {nameof(NextStageName), o => o.NextStageName},
                {nameof(InitialLevel), o => o.InitialLevel},
                {nameof(ArmorClass), o => o.ArmorClass},
                {nameof(MagicResistance), o => o.MagicResistance},
                {nameof(MovementRate), o => o.MovementRate},
                {nameof(Weight), o => o.Weight},
                {nameof(Size), o => o.Size},
                {nameof(Nutrition), o => o.Nutrition},
                {nameof(Material), o => o.Material},
                {nameof(Abilities), o => o.Abilities},
                {nameof(SimpleProperties), o => o.SimpleProperties},
                {nameof(ValuedProperties), o => o.ValuedProperties},
                {nameof(GenerationFlags), o => o.GenerationFlags},
                {nameof(GenerationFrequency), o => o.GenerationFrequency},
                {nameof(Behavior), o => o.Behavior},
                {nameof(Alignment), o => o.Alignment},
                {nameof(Noise), o => o.Noise}
            },
            new Func<CreatureVariant, object>[0],
            new Dictionary<string, Func<CreatureVariant, object>>
            {
                {nameof(Material), o => Material.Flesh}
            });

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}