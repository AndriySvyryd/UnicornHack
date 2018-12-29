using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature ClayGolem = new Creature
        {
            Name = "clay golem",
            Species = Species.Golem,
            MovementDelay = 171,
            Material = Material.Mineral,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 160}}
                }
            },
            InitialLevel = 11,
            Sex = Sex.None,
            Size = 8,
            Weight = 1500,
            Perception = 6,
            Might = 6,
            Speed = 6,
            Focus = 10,
            MagicDeflection = 20,
            PhysicalDeflection = 13,
            ElectricityResistance = 75,
            SlimingImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Mindless = true,
            NonAnimal = true
        };
    }
}
