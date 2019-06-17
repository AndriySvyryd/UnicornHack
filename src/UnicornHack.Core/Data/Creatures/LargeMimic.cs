using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature LargeMimic = new Creature
        {
            Name = "large mimic",
            Species = Species.Mimic,
            SpeciesClass = SpeciesClass.ShapeChanger,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Bite,
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
                    Effects = new HashSet<Effect>
                    {
                        new Stick()
                    }
                }
            },
            InitialLevel = 8,
            GenerationWeight = new DefaultWeight {Multiplier = 3F},
            PreviousStageName = "small mimic",
            NextStageName = "giant mimic",
            Weight = 600,
            MovementDelay = 300,
            TurningDelay = 300,
            Perception = -5,
            Might = -6,
            Speed = -5,
            Focus = -6,
            Armor = 1,
            MagicResistance = 5,
            AcidResistance = 75,
            HeadType = HeadType.None,
            TorsoType = TorsoType.Amorphic,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            EyeCount = 0,
            NoiseLevel = 0,
            VisibilityLevel = 1,
            Infravisible = true,
            Clingy = true
        };
    }
}
