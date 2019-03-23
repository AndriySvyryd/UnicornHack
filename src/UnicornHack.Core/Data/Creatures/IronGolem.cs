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
                    Range = 1,
                    Action = AbilityAction.Punch,
                    Cooldown = 100,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "220"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 20,
                    Action = AbilityAction.Breath,
                    Cooldown = 250,
                    Effects = new HashSet<Effect> {new Blight {Damage = "140"}}
                }
            },
            InitialLevel = 18,
            Sex = Sex.None,
            Size = 8,
            Weight = 2000,
            Perception = 10,
            Might = 10,
            Speed = 10,
            Focus = 14,
            Armor = 3,
            MagicResistance = 30,
            ColdResistance = 75,
            FireResistance = 75,
            WaterResistance = 50,
            SlimingImmune = true,
            StoningImmune = true,
            TorsoType = TorsoType.Humanoid,
            UpperExtremities = ExtremityType.GraspingFingers,
            LowerExtremities = ExtremityType.Fingers,
            RespirationType = RespirationType.None,
            Mindless = true,
            NonAnimal = true
        };
    }
}
