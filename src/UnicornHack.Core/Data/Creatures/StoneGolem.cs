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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.Attack,
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "160*physicalScaling"}}
                }
            },
            InitialLevel = 14,
            Sex = Sex.None,
            Size = 8,
            Weight = 2000,
            MovementDelay = 100,
            TurningDelay = 100,
            Perception = -2,
            Might = -2,
            Speed = -2,
            Focus = 2,
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
