namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Werewolf = new Creature
    {
        Name = "werewolf",
        SpeciesClass = SpeciesClass.ShapeChanger,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "werewolf ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "werewolf ability #2",
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
                Name = "werewolf ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 250,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ConferLycanthropy { CreatureName = "wolfwere" } }
            }
        },
        InitialLevel = 5,
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Lycanthrope,
        Weight = 1000,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Regeneration = 3,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Lycanthropy = "wolfwere"
    };
}
