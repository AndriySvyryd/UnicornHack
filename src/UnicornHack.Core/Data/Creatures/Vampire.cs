namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Vampire = new Creature
    {
        Name = "vampire",
        SpeciesClass = SpeciesClass.ShapeChanger | SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "vampire ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "40*MightModifier()" } }
            },
            new Ability
            {
                Name = "vampire ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "15*MightModifier()" } }
            },
            new Ability
            {
                Name = "vampire ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new DrainLife { Amount = "8*MightModifier()" } }
            },
            new Ability
            {
                Name = "vampire ability #4",
                Type = AbilityType.RangedSpell,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Spell,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new Wither { Amount = "40*FocusModifier()" },
                    new Wither { Duration = EffectDuration.UntilTimeout, Amount = "40*FocusModifier()" },
                    new Enervate()
                }
            }
        },
        InitialLevel = 20,
        Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Hiss,
        Weight = 1000,
        MovementDelay = -15,
        TurningDelay = -15,
        PerceptionBonus = 1,
        SpeedBonus = 1,
        FocusBonus = 6,
        Regeneration = 3,
        HealingResistance = 150,
        Armor = 7,
        MagicResistance = 30,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        VisibilityLevel = 0,
        Infravision = true,
        InvisibilityDetection = true
    };
}
