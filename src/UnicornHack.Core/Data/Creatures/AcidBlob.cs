using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature AcidBlob = new Creature
        {
            Name = "acid blob",
            Species = Species.Blob,
            MovementDelay = 400,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeHit,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 40}}
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Behavior = AIBehavior.Wandering,
            Sex = Sex.None,
            Size = 1,
            Weight = 30,
            Perception = 1,
            Might = 0,
            Speed = 1,
            Focus = 6,
            PhysicalDeflection = 12,
            AcidResistance = 75,
            StoningImmune = true,
            HeadType = HeadType.None,
            TorsoType = TorsoType.Amorphic,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            EyeCount = 0,
            NoiseLevel = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}
