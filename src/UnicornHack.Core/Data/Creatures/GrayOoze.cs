using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature GrayOoze = new Creature
        {
            Name = "gray ooze",
            Species = Species.Ooze,
            MovementDelay = 1200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 90}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Soak {Damage = 90}}
                },
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeHit,
                    Effects = new HashSet<Effect> {new Soak {Damage = 40}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Sex = Sex.None,
            Weight = 500,
            Perception = 2,
            Might = 2,
            Speed = 2,
            Focus = 6,
            PhysicalDeflection = 12,
            AcidResistance = 75,
            ColdResistance = 75,
            FireResistance = 75,
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
