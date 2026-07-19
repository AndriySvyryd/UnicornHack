namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature LargeCobra = new Creature
    {
        Name = "large cobra",
        SpeciesClass = SpeciesClass.Reptile,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "large cobra ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ToxinDamage { Duration = EffectDuration.UntilTimeout, Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "large cobra ability #2",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Spit,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Blind() }
            }
        },
        InitialLevel = 7,
        GenerationWeight = "2",
        Noise = ActorNoiseType.Hiss,
        Weight = 250,
        MovementDelay = -34,
        TurningDelay = -34,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 4,
        DecayResistance = 50,
        TorsoType = TorsoType.Serpentine,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0,
        VisibilityLevel = 2,
        Infravision = true
    };
}
