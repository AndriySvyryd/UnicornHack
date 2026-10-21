namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Ettin = new Creature
    {
        Name = "ettin",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 16,
        Weight = 2250,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        FocusBonus = -4,
        Armor = 3,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "two-headed force",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "90*MightModifier()" } }
            },
            new Ability
            {
                Name = "left fist",
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
                Name = "right fist",
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
        EnglishDescription = "A hulking, two-headed humanoid with thick limbs and a broad, sloping chest. Its mismatched heads sit close together, each with a heavy jaw and a tangled crop of hair.",
        InitialLevel = 10,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth >= 2 ? 2 : 0",
        Noise = ActorNoiseType.Boast
    };
}
