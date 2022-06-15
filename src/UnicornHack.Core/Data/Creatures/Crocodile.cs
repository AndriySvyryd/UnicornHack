using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Crocodile = new Creature
    {
        Name = "crocodile",
        Species = Species.Crocodile,
        SpeciesClass = SpeciesClass.Reptile,
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
            }
        },
        InitialLevel = 8,
        GenerationWeight = "2",
        PreviousStageName = "baby crocodile",
        Size = 8,
        Weight = 1500,
        MovementDelay = 33,
        TurningDelay = 33,
        Perception = -5,
        Might = -6,
        Speed = -5,
        Focus = -6,
        Armor = 3,
        MagicResistance = 5,
        UpperExtremities = ExtremityType.None,
        RespirationType = RespirationType.Amphibious,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 1
    };
}
