namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Cobra = new Creature
    {
        Name = "cobra",
        SpeciesClass = SpeciesClass.Reptile,
        MovementDelay = -34,
        TurningDelay = -34,
        Weight = 250,
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
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "venomous bite",
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
                Name = "blinding spit",
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
        EnglishDescription = "A smooth-scaled snake with a slender body and a broad hood flaring behind its head. Its raised neck sways above a loose coil while a forked tongue flickers between closed jaws.",
        InitialLevel = 7,
        GenerationWeight = "0",
        Noise = ActorNoiseType.Hiss
    };
}
