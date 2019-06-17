using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Shrieker = new Creature
        {
            Name = "shrieker",
            Species = Species.Fungus,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.Targeted,
                    Trigger = ActivationType.OnRangedAttack,
                    Range = 20,
                    Action = AbilityAction.Scream,
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = "5+attackScaling",
                    Cooldown = 100,
                    Delay = "100*attackScaling",
                    Effects = new HashSet<Effect>
                        {new Deafen {Duration = EffectDuration.UntilTimeout, DurationAmount = "3"}}
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Sex = Sex.None,
            Size = 2,
            Weight = 100,
            MovementDelay = 1100,
            TurningDelay = 1100,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -4,
            Armor = 1,
            HeadType = HeadType.None,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.None,
            RespirationType = RespirationType.None,
            InventorySize = 0,
            EyeCount = 0,
            Mindless = true,
            NonAnimal = true
        };
    }
}
