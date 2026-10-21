namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature KetebMeriri = new Creature
    {
        Name = "Keteb Meriri",
        SpeciesClass = SpeciesClass.Demon,
        Material = Material.Air,
        MovementDelay = 100,
        TurningDelay = 100,
        Weight = 0,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        VoidResistance = 75,
        DecayResistance = 100,
        TorsoType = TorsoType.Amorphic,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        RespirationType = RespirationType.None,
        SlotCapacity = 0,
        NonAnimal = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "bitter blow",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "noonday wasting",
                Type = AbilityType.RangedSpell,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 5,
                Action = AbilityAction.Spell,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 350,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Wither { Duration = EffectDuration.UntilTimeout, Amount = "50*FocusModifier()" }, new Decay() }
            }
        },
        EnglishDescription = "A wavering mass of dusty haze with a dark, shifting center. Thin veils peel from its edges, leaving its outline blurred like air above hot stone.",
        InitialLevel = 8,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Stalking | AIBehavior.MagicUser | AIBehavior.Roaming,
        Sex = Sex.None
    };
}
