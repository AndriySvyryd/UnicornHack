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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "220*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Breath,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 250,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new Blight {Damage = "140*mentalScaling"}}
                }
            },
            InitialLevel = 18,
            Sex = Sex.None,
            Size = 8,
            Weight = 2000,
            MovementDelay = 100,
            TurningDelay = 100,
            Focus = 4,
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
