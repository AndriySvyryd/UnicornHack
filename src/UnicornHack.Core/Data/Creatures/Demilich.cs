namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Demilich = new Creature
    {
        Name = "demilich",
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "demilich ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ColdDamage { Amount = "70*MightModifier()" } }
            }
        },
        InitialLevel = 14,
        PreviousStageName = "lich",
        Behavior = AIBehavior.MagicUser,
        Noise = ActorNoiseType.Mumble,
        Weight = 600,
        MovementDelay = 33,
        TurningDelay = 33,
        PerceptionBonus = -2,
        MightBonus = -2,
        SpeedBonus = -2,
        FocusBonus = 2,
        Regeneration = 3,
        HealingResistance = 200,
        Armor = 6,
        MagicResistance = 30,
        ColdResistance = 75,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true
    };
}
