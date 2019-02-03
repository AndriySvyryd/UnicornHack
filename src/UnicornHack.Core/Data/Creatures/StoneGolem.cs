using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature StoneGolem = new Creature
        {
            Name = "stone golem",
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 160}}
                }
            },
            InitialLevel = 14,
            Sex = Sex.None,
            Size = 8,
            Weight = 2000,
            Perception = 8,
            Might = 8,
            Speed = 8,
            Focus = 12,
            Armor = 3,
            MagicResistance = 25,
            ColdResistance = 75,
            ElectricityResistance = 75,
            FireResistance = 75,
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
