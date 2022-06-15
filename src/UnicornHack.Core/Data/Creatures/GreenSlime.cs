using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GreenSlime = new Creature
    {
        Name = "green slime",
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
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Slime() }
            },
            new Ability
            {
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new Slime() }
            }
        },
        InitialLevel = 6,
        GenerationWeight = "$branch == 'hell' ? 1 : 0",
        Sex = Sex.None,
        Weight = 400,
        MovementDelay = 100,
        TurningDelay = 100,
        Perception = -6,
        Might = -6,
        Speed = -6,
        Focus = -2,
        Armor = 2,
        AcidResistance = 75,
        ColdResistance = 75,
        ElectricityResistance = 75,
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
