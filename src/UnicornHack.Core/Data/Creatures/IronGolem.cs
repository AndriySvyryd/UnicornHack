using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature IronGolem = new Creature
        {
            Name = "iron golem",
            Species = Species.Golem,
            MovementDelay = 200,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Punch,
                    Timeout = 1,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 220}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Action = AbilityAction.Breath,
                    Timeout = 5,
                    Effects = new HashSet<Effect> {new Blight {Damage = 140}}
                }
            },
            InitialLevel = 18,
            Sex = Sex.None,
            Size = 8,
            Weight = 2000,
            Agility = 10,
            Constitution = 10,
            Intelligence = 10,
            Quickness = 10,
            Strength = 10,
            Willpower = 15,
            MagicResistance = 60,
            PhysicalDeflection = 17,
            ColdResistance = 75,
            FireResistance = 75,
            WaterResistance = 50,
            SlimingImmune = true,
            StoningImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremeties = ExtremityType.GraspingFingers,
            LowerExtremeties = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Mindless = true,
            NonAnimal = true
        };
    }
}
