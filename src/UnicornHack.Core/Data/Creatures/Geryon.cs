namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Geryon = new Creature
    {
        Name = "Geryon",
        SpeciesClass = SpeciesClass.Aberration,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Geryon ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()" } }
            },
            new Ability
            {
                Name = "Geryon ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()" } }
            },
            new Ability
            {
                Name = "Geryon ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Sting,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new ToxinDamage { Duration = EffectDuration.UntilTimeout, Amount = "50*MightModifier()" } }
            }
        },
        InitialLevel = 30,
        GenerationWeight = "$branch == 'hell' ? 0 : 1",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Stalking,
        Noise = ActorNoiseType.Cuss,
        Sex = Sex.Male,
        Size = 16,
        Weight = 2500,
        MovementDelay = 300,
        TurningDelay = 300,
        PerceptionBonus = 6,
        MightBonus = 6,
        SpeedBonus = 6,
        FocusBonus = 6,
        Armor = 6,
        MagicResistance = 37,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        LowerExtremities = ExtremityType.None,
        LocomotionType = LocomotionType.Flying,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true
    };
}
