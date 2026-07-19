namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GiantSpider = new Creature
    {
        Name = "giant spider",
        SpeciesClass = SpeciesClass.Vermin,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "posion bite",
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
                Name = "weakening bite",
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
            },
            new Ability
            {
                Name = "web",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 5,
                Action = AbilityAction.Spit,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 200,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Cripple() }
            }
        },
        InitialLevel = 1,
        GenerationWeight = "2",
        Weight = 150,
        MovementDelay = -20,
        TurningDelay = -20,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Armor = 3,
        DecayResistance = 50,
        ToppleResistance = 25,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        Clingy = true
    };
}
