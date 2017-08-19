using System.Collections.Generic;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant LongWormTail = new CreatureVariant
        {
            Name = "long worm tail",
            Species = Species.Worm,
            SpeciesClass = SpeciesClass.Vermin,
            ValuedProperties = new Dictionary<string, object> {{"largeness", Size.Gigantic}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable
        };
    }
}