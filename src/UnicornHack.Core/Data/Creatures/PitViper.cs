namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature PitViper = new Creature
    {
        Name = "pit viper",
        SpeciesClass = SpeciesClass.Reptile,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "pit viper ability #1",
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
            }
        },
        InitialLevel = 6,
        GenerationWeight = "2",
        Noise = ActorNoiseType.Hiss,
        Weight = 100,
        MovementDelay = -20,
        TurningDelay = -20,
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
