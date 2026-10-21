namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature PitViper = new Creature
    {
        Name = "pit viper",
        SpeciesClass = SpeciesClass.Reptile,
        MovementDelay = -20,
        TurningDelay = -20,
        Weight = 100,
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
            }
        },
        EnglishDescription = "A stout snake with rough, mottled scales and a broad, triangular head. Small pits mark its face between the eyes and nostrils, and its tail narrows abruptly from the thick body.",
        InitialLevel = 6,
        GenerationWeight = "0",
        Behavior = AIBehavior.Stalking,
        Noise = ActorNoiseType.Hiss
    };
}
