namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Ghul = new Creature
    {
        Name = "ghul",
        SpeciesClass = SpeciesClass.ShapeChanger,
        MovementDelay = 50,
        TurningDelay = 50,
        Weight = 400,
        PerceptionBonus = -3,
        MightBonus = -4,
        SpeedBonus = -3,
        FocusBonus = 2,
        Armor = 3,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "rending claw",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*MightModifier()" } }
            },
            new Ability
            {
                Name = "hindering claw",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Slow() }
            }
        },
        EnglishDescription = "A gaunt, long-limbed humanoid with grey skin and a narrow, sunken face. Its shoulders hunch above a hollow chest, and dark nails curve from its long fingers.",
        InitialLevel = 12,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Stalking,
        Noise = ActorNoiseType.Growl
    };
}
