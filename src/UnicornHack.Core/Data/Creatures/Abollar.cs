namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Abollar = new Creature
    {
        Name = "abollar",
        SpeciesClass = SpeciesClass.Aberration,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "abollar ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "abollar ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage() }
            },
            new Ability
            {
                Name = "abollar ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Suck,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new ChangeProperty<int> { Duration = EffectDuration.UntilTimeout, DurationAmount = "10*FocusModifier()", Amount = "-2", PropertyName = "Focus" }
                }
            }
        },
        InitialLevel = 21,
        GenerationWeight = "4.0",
        Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Gurgle,
        Size = 8,
        Weight = 1200,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 2,
        MagicResistance = 40,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Flying,
        Telepathic = 3,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true
    };
}
