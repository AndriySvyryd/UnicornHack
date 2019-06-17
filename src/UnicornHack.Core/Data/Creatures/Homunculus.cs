using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature Homunculus = new Creature
        {
            Name = "homunculus",
            Species = Species.Homunculus,
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "10*physicalScaling"}}
                },
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
                    Effects = new HashSet<Effect>
                        {new Sedate {Duration = EffectDuration.UntilTimeout, DurationAmount = "2"}}
                }
            },
            InitialLevel = 2,
            GenerationWeight = new DefaultWeight {Multiplier = 4F},
            Behavior = AIBehavior.Stalking,
            Sex = Sex.None,
            Size = 2,
            Weight = 60,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -4,
            Regeneration = 3,
            Armor = 2,
            MagicResistance = 5,
            Infravisible = true,
            Infravision = true,
            Mindless = true
        };
    }
}
