using System;
using System.Collections.Generic;
using System.IO;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameDefinitions
{
    public class CreatureVariant : ActorVariant
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
    }
}