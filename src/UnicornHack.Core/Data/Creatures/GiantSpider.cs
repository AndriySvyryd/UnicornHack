namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GiantSpider = new Creature
    {
        Name = "giant spider",
        SpeciesClass = SpeciesClass.Vermin,
        MovementDelay = -20,
        TurningDelay = -20,
        Weight = 150,
        PerceptionBonus = -3,
        MightBonus = -8,
        SpeedBonus = -2,
        FocusBonus = -8,
        Armor = 3,
        DecayResistance = 50,
        ToppleResistance = 25,
        TorsoType = TorsoType.Arachnid,
        UpperExtremities = ExtremityType.Claws,
        LowerExtremities = ExtremityType.Claws,
        Clingy = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "poison bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ToxinDamage { Duration = EffectDuration.UntilTimeout, Amount = "10*MightModifier()" } }
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
                Cooldown = 500,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Weaken() }
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
                Cooldown = 500,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Paralyze { Amount = "100*MightModifier()" } }
            }
        },
        InitialLevel = 1,
        GenerationWeight = "2"
    };
}
