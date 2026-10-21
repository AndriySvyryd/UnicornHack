namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Koschei = new Creature
    {
        Name = "Koschei",
        SpeciesClass = SpeciesClass.Humanoid,
        MovementDelay = 100,
        TurningDelay = 100,
        Weight = 600,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        MagicResistance = 15,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "deathless touch",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            },
            new Ability
            {
                Name = "doom",
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
        EnglishDescription = "An exceedingly thin, upright figure with parchment-pale skin and sharp, angular features. Sparse hair frames a high brow, while long, bony hands emerge from dark, hanging sleeves.",
        InitialLevel = 11,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.BranchBoss,
        Behavior = AIBehavior.MagicUser,
        Noise = ActorNoiseType.Mumble
    };
}
