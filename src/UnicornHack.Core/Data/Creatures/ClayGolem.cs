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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "160*physicalScaling"}}
                }
            },
            InitialLevel = 11,
            GenerationWeight = new DefaultWeight { Multiplier = 0F },
            Sex = Sex.None,
            Size = 8,
            Weight = 1500,
            MovementDelay = 71,
            TurningDelay = 71,
            Material = Material.Mineral,
            Perception = -4,
            Might = -4,
            Speed = -4,
            Armor = 1,
            MagicResistance = 20,
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
