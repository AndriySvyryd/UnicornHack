using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Cerberous = new Creature
    {
        Name = "Cerberous",
        Species = Species.Dog,
        SpeciesClass = SpeciesClass.Canine,
        Abilities = new HashSet<Ability>
        {
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "100*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Breath,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Burn { Damage = "100*FocusModifier()" } }
            }
        },
        InitialLevel = 12,
        GenerationWeight = "$branch == 'hell' ? 6 : 0",
        Noise = ActorNoiseType.Bark,
        Weight = 700,
        MovementDelay = -15,
        TurningDelay = -15,
        Perception = -3,
        Might = -4,
        Speed = -3,
        Focus = -4,
        Armor = 4,
        MagicResistance = 10,
        FireResistance = 75,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
