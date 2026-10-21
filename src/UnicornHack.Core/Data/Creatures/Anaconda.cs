namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Anaconda = new Creature
    {
        Name = "anaconda",
        SpeciesClass = SpeciesClass.Reptile,
        MovementDelay = 300,
        TurningDelay = 300,
        Weight = 250,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 2,
        TorsoType = TorsoType.Serpentine,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 0,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "crushing bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "crushing embrace",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "constricting embrace",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Bind() }
            }
        },
        EnglishDescription = "A thick-bodied snake with olive scales patterned in dark oval blotches. Its broad head rests low among heavy coils, with small eyes set high above the jaws.",
        InitialLevel = 6,
        GenerationWeight = "0",
        Noise = ActorNoiseType.Hiss
    };
}
