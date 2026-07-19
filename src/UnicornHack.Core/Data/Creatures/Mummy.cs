namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Mummy = new Creature
    {
        Name = "mummy",
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "mummy ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" }, new Decay() }
            }
        },
        InitialLevel = 6,
        Noise = ActorNoiseType.Moan,
        Weight = 1000,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -2,
        HealingResistance = 150,
        Armor = 3,
        MagicResistance = 10,
        ColdResistance = 75,
        DecayResistance = 100,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true
    };
}
