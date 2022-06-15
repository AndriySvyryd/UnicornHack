using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature InvisibleStalker = new Creature
    {
        Name = "invisible stalker",
        Species = Species.Elemental,
        SpeciesClass = SpeciesClass.Extraplanar,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "100*MightModifier()" } }
            }
        },
        InitialLevel = 8,
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Size = 8,
        Weight = 900,
        Perception = -5,
        Might = -6,
        Speed = -5,
        Focus = -6,
        Armor = 3,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.Claws,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        NoiseLevel = 0,
        VisibilityLevel = 0,
        Infravision = true,
        InvisibilityDetection = true
    };
}
