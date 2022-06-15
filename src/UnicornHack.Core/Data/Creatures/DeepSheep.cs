using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature DeepSheep = new Creature
    {
        Name = "deep sheep",
        Species = Species.Quadruped,
        SpeciesClass = SpeciesClass.Quadrupedal,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Headbutt,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "40*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20*MightModifier()" } }
            }
        },
        InitialLevel = 2,
        GenerationWeight = "4",
        GenerationFlags = GenerationFlags.SmallGroup,
        Noise = ActorNoiseType.Bleat,
        Weight = 600,
        MovementDelay = 33,
        TurningDelay = 33,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -8,
        Armor = 1,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        AcuityLevel = 0,
        Infravisible = true
    };
}
