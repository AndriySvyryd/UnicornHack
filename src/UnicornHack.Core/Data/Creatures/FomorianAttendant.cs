namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature FomorianAttendant = new Creature
    {
        Name = "fomorian attendant",
        SpeciesClass = SpeciesClass.Humanoid,
        Weight = 1200,
        MightBonus = 2,
        FocusBonus = 1,
        Armor = 1,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "attendant's blow",
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
            }
        },
        EnglishDescription = "A heavy-boned humanoid with a thick neck, broad hands and a deeply creased face. Its shoulders sit at uneven heights beneath coarse, drab clothing.",
        InitialLevel = 4,
        GenerationWeight = "0"
    };
}
