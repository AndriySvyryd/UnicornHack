namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Firefly = new Creature
    {
        Name = "firefly",
        SpeciesClass = SpeciesClass.Vermin,
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
            }
        },
        InitialLevel = 1,
        GenerationWeight = "4",
        Noise = ActorNoiseType.Buzz,
        Size = 1,
        Weight = 10,
        PerceptionBonus = -2,
        MightBonus = -9,
        SpeedBonus = -5,
        FocusBonus = -9,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        Infravisible = true
    };
}
