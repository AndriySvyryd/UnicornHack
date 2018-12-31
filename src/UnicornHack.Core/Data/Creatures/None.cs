using UnicornHack.Generation;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature NONE = new Creature
        {
            Name = "NONE",
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
            Weight = 0,
            Perception = 1,
            Might = 1,
            Speed = 1,
            Focus = 1
        };
    }
}
