using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Halfling = new Creature
        {
            Name = "halfling",
            Species = Species.Halfling,
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 5F},
            GenerationFlags = GenerationFlags.NonPolymorphable,
            Behavior = AIBehavior.AlignmentAware | AIBehavior.WeaponCollector,
            Noise = ActorNoiseType.Speach,
            Weight = 500,
            MovementDelay = 33,
            TurningDelay = 33,
            Perception = -1,
            Might = -8,
            Speed = -4,
            Focus = -8,
            MagicResistance = 5,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            Infravisible = true,
            Infravision = true
        };
    }
}
