namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Zombie = new Creature
    {
        Name = "zombie",
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "zombie ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "40*MightModifier()" } }
            }
        },
        InitialLevel = 4,
        GenerationWeight = "4",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.Stalking,
        Noise = ActorNoiseType.Moan,
        Weight = 1000,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -2,
        HealingResistance = 150,
        ColdResistance = 75,
        FrightenResistance = 100,
        SedateResistance = 100,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true,
        Mindless = true
    };
}
