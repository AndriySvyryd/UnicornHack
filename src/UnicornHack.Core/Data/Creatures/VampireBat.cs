namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature VampireBat = new Creature
    {
        Name = "vampire bat",
        SpeciesClass = SpeciesClass.Bird,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "vampire bat ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "vampire bat ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new ChangeProperty<int> { Duration = EffectDuration.UntilTimeout, DurationAmount = "5*MightModifier()", Amount = "-1", PropertyName = "Might" }
                }
            }
        },
        InitialLevel = 5,
        GenerationWeight = "5",
        PreviousStageName = "giant bat",
        Behavior = AIBehavior.Wandering,
        Noise = ActorNoiseType.Sqeek,
        Size = 1,
        Weight = 100,
        MovementDelay = -40,
        TurningDelay = -40,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Regeneration = 3,
        Armor = 2,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 1,
        Quietness = 250,
        Infravisible = true
    };
}
