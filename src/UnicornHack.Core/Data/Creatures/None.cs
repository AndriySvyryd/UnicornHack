using System.Collections.Generic;
using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly CreatureVariant NONE = new CreatureVariant
        {
            Name = "NONE",
            ValuedProperties = new Dictionary<string, object> {{"weight", 0}},
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable
        };
    }
}