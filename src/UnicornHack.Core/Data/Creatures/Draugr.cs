namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Draugr = new Creature
    {
        Name = "draugr",
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "draugr ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "draugr ability #2",
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
                Name = "draugr ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new DrainLife { Amount = "2*MightModifier()" } }
            }
        },
        InitialLevel = 3,
        GenerationWeight = "2",
        Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Howl,
        Weight = 1200,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -4,
        HealingResistance = 150,
        Armor = 2,
        MagicResistance = 2,
        ColdResistance = 75,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true
    };
}
