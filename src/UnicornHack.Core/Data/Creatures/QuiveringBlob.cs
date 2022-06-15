using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature QuiveringBlob = new Creature
    {
        Name = "quivering blob",
        Species = Species.Blob,
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "40*MightModifier()" } }
            }
        },
        InitialLevel = 5,
        GenerationWeight = "5",
        Behavior = AIBehavior.Wandering,
        Sex = Sex.None,
        Size = 2,
        Weight = 200,
        MovementDelay = 1100,
        TurningDelay = 1100,
        Perception = -7,
        Might = -8,
        Speed = -7,
        Focus = -2,
        Armor = 1,
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
