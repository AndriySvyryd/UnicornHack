namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Meridiana = new Creature
    {
        Name = "Meridiana",
        SpeciesClass = SpeciesClass.Demon,
        Material = Material.Air,
        Size = 8,
        Weight = 100,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        MagicResistance = 35,
        FireResistance = 75,
        FascinateResistance = 100,
        SedateResistance = 100,
        SuffocateResistance = 100,
        TorsoType = TorsoType.Amorphic,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Flying,
        NonAnimal = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "siphoning touch",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new DrainEnergy { Amount = "30*FocusModifier()" } }
            },
            new Ability
            {
                Name = "borrowed-force discharge",
                Type = AbilityType.RangedSpell,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 3,
                Action = AbilityAction.Spell,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PsychicDamage { Amount = "50*FocusModifier()" } }
            },
            new Ability
            {
                Name = "captivating presence",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 3,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Fascinate() }
            }
        },
        EnglishDescription = "A pale, luminous haze gathers into the suggestion of a tall, draped figure. Its smooth outline dissolves into curling ribbons before any lasting features can be made out.",
        InitialLevel = 9,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Stalking | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Mumble,
        Sex = Sex.Female
    };
}
