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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "90*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect> {new Soak {Damage = "90*physicalScaling"}}
                },
                new Ability
                {
                    Activation = ActivationType.OnMeleeHit,
                    Action = AbilityAction.Touch,
                    SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                    Accuracy = "10+attackScaling",
                    Effects = new HashSet<Effect> {new Soak {Damage = "40*physicalScaling"}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Sex = Sex.None,
            Weight = 500,
            MovementDelay = 1100,
            TurningDelay = 1100,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -4,
            Armor = 1,
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
