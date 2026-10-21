namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Firefly = new Creature
    {
        Name = "firefly",
        SpeciesClass = SpeciesClass.Vermin,
        Size = 1,
        Weight = 10,
        PerceptionBonus = -2,
        MightBonus = -9,
        SpeedBonus = -5,
        FocusBonus = -9,
        TorsoType = TorsoType.Insectoid,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "fire sting",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Sting,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new FireDamage { Amount = "10*MightModifier()" },
                    new FireDamage { Duration = EffectDuration.UntilTimeout, Amount = "10*MightModifier()" }
                }
            },
            new Ability
            {
                Name = "dazzling flash",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 3,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Fascinate() }
            }
        },
        EnglishDescription = "A winged insect with dark, narrow wing cases and fine, jointed antennae. A soft yellow-green light glows at the tip of its segmented abdomen.",
        InitialLevel = 1,
        GenerationWeight = "$branch == 'outer wards' ? 2 : ($branch == 'ruined stronghold' ? 1 : 0)",
        Noise = ActorNoiseType.Buzz
    };
}
