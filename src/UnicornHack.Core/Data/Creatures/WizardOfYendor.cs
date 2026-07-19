namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature WizardOfYendor = new Creature
    {
        Name = "Wizard of Yendor",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Wizard of Yendor ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            }
        },
        InitialLevel = 30,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.RangedPeaceful | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Cuss,
        Sex = Sex.Male,
        Weight = 1000,
        PerceptionBonus = 6,
        FocusBonus = 6,
        Regeneration = 3,
        EnergyRegeneration = 2,
        Armor = 9,
        MagicResistance = 50,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.Fingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Walking | LocomotionType.Teleportation,
        Telepathic = 3,
        Infravisible = true,
        InvisibilityDetection = true
    };
}
