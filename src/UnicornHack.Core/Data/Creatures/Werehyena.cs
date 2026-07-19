namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Werehyena = new Creature
    {
        Name = "werehyena",
        SpeciesClass = SpeciesClass.ShapeChanger,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "werehyena ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "werehyena ability #2",
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
                Name = "werehyena ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 250,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ConferLycanthropy { CreatureName = "ratwere" } }
            }
        },
        InitialLevel = 3,
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Lycanthrope,
        Weight = 1000,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Regeneration = 3,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Lycanthropy = "hyenawere"
    };
}
