namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Salamander = new Creature
    {
        Name = "salamander",
        SpeciesClass = SpeciesClass.Extraplanar,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "salamander ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "90*MightModifier()" } }
            },
            new Ability
            {
                Name = "salamander ability #2",
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
                Name = "salamander ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new FireDamage { Amount = "30*MightModifier()" },
                    new FireDamage { Duration = EffectDuration.UntilTimeout, Amount = "30*MightModifier()" }
                }
            },
            new Ability
            {
                Name = "salamander ability #4",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new FireDamage { Amount = "70*MightModifier()" },
                    new FireDamage { Duration = EffectDuration.UntilTimeout, Amount = "70*MightModifier()" }
                }
            },
            new Ability
            {
                Name = "salamander ability #5",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Hug,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new FireDamage { Amount = "100*MightModifier()" },
                    new FireDamage { Duration = EffectDuration.UntilTimeout, Amount = "100*MightModifier()" }
                }
            }
        },
        InitialLevel = 10,
        GenerationWeight = "$branch == 'hell' ? 2 : 0",
        Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Mumble,
        Size = 8,
        Weight = 1500,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        Armor = 5,
        FireResistance = 75,
        TorsoType = TorsoType.Serpentine,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        Infravisible = true,
        Infravision = true
    };
}
