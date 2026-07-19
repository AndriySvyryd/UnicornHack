namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Lich = new Creature
    {
        Name = "lich",
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "lich ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ColdDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "lich ability #2",
                Type = AbilityType.RangedSpell,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Spell,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Doom { DurationAmount = "500*FocusModifier()", Amount = "40*FocusModifier()" } }
            }
        },
        InitialLevel = 11,
        NextStageName = "demilich",
        Behavior = AIBehavior.MagicUser,
        Noise = ActorNoiseType.Mumble,
        Weight = 600,
        MovementDelay = 100,
        TurningDelay = 100,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        Regeneration = 3,
        HealingResistance = 200,
        MagicResistance = 15,
        ColdResistance = 75,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true
    };
}
