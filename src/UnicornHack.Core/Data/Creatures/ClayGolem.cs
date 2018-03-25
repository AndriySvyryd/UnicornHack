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
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 160}}
                }
            },
            InitialLevel = 11,
            Sex = Sex.None,
            Size = 8,
            Weight = 1500,
            Agility = 6,
            Constitution = 6,
            Intelligence = 6,
            Quickness = 6,
            Strength = 6,
            Willpower = 11,
            MagicResistance = 40,
            PhysicalDeflection = 13,
            ElectricityResistance = 75,
            SlimingImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Mindless = true,
            NonAnimal = true
        };
    }
}
