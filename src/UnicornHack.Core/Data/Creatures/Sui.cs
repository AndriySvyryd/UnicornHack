namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Sui = new Creature
    {
        Name = "Sui",
        SpeciesClass = SpeciesClass.Reptile,
        Size = 8,
        Weight = 1500,
        PerceptionBonus = -3,
        MightBonus = -4,
        SpeedBonus = -3,
        FocusBonus = -4,
        Armor = 3,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        RespirationType = RespirationType.Amphibious,
        LocomotionType = LocomotionType.Swimming,
        SlotCapacity = 1,
        Infravisible = true,
        Infravision = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "draining bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new DrainEnergy(), new Overcharge { DurationAmount = "1000*FocusModifier()", Amount = "30*FocusModifier()" } }
            },
            new Ability
            {
                Name = "lingering drain",
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new DrainEnergy { Amount = "5*FocusModifier()" } }
            }
        },
        EnglishDescription = "A squat reptilian creature with a broad head, dark mottled scales and a thick, trailing tail. Its short legs splay beneath a low body, and small, glossy eyes watch from beneath ridged brows.",
        InitialLevel = 12,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable | CreatureGenerationFlags.LevelBoss,
        Noise = ActorNoiseType.Growl
    };
}
