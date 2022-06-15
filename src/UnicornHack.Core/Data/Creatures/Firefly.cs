using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Firefly = new Creature
    {
        Name = "firefly",
        Species = Species.Beetle,
        SpeciesClass = SpeciesClass.Vermin,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "fire sting",
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Sting,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Burn { Damage = "10*MightModifier()" } }
            }
        },
        InitialLevel = 1,
        GenerationWeight = "4",
        Noise = ActorNoiseType.Buzz,
        Size = 1,
        Weight = 10,
        Perception = -9,
        Might = -9,
        Speed = -5,
        Focus = -9,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        Infravisible = true
    };
}
