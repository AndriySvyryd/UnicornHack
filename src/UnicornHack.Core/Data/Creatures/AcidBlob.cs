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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "splashback", Activation = ActivationType.OnMeleeHit,
                    Effects = new HashSet<Effect> {new Corrode {Damage = "40"}}
                }
            },
            InitialLevel = 1,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            Behavior = AIBehavior.Wandering,
            Sex = Sex.None,
            Size = 1,
            Weight = 30,
            MovementDelay = 400,
            Perception = 1,
            Might = 5,
            Speed = 1,
            Focus = 6,
            Armor = 1,
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
