using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature BlackPudding = new Creature
        {
            Name = "black pudding",
            Species = Species.Pudding,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Touch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 130}}
                },
                new Ability
                {
                    Activation = ActivationType.OnPhysicalMeleeHit,
                    Effects = new HashSet<Effect> {new Corrode {Damage = 30}}
                }
            },
            InitialLevel = 10,
            Sex = Sex.None,
            Weight = 512,
            Perception = 6,
            Might = 6,
            Speed = 6,
            Focus = 10,
            PhysicalDeflection = 14,
            AcidResistance = 75,
            ColdResistance = 75,
            ElectricityResistance = 75,
            StoningImmune = true,
            HeadType = HeadType.None,
            TorsoType = TorsoType.Amorphic,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            EyeCount = 0,
            NoiseLevel = 0,
            Mindless = true,
            NonAnimal = true,
            Reanimation = true
        };
    }
}
