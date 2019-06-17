using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures
{
    public static partial class CreatureData
    {
        public static readonly Creature RabidRat = new Creature
        {
            Name = "rabid rat",
            Species = Species.Rat,
            SpeciesClass = SpeciesClass.Rodent,
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
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "50*physicalScaling"}}
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
                    {
                        new ChangeProperty<int>
                        {
                            PropertyName = "Might", Value = -1, Duration = EffectDuration.UntilTimeout,
                            DurationAmount = "5"
                        }
                    }
                }
            },
            InitialLevel = 3,
            GenerationWeight = new DefaultWeight {Multiplier = 6F},
            GenerationFlags = GenerationFlags.SmallGroup,
            Noise = ActorNoiseType.Sqeek,
            Size = 2,
            Weight = 150,
            Perception = -8,
            Might = -8,
            Speed = -8,
            Focus = -8,
            Armor = 2,
            TorsoType = TorsoType.Quadruped,
            UpperExtremities = ExtremityType.None,
            LowerExtremities = ExtremityType.Claws,
            InventorySize = 1,
            Infravisible = true
        };
    }
}
