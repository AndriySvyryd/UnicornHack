namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature BrynachsEagle = new Creature
    {
        Name = "Brynach's Eagle",
        SpeciesClass = SpeciesClass.Bird,
        Weight = 400,
        PerceptionBonus = 4,
        SpeedBonus = 4,
        FocusBonus = -1,
        TorsoType = TorsoType.Avian,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 1,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "talon attack",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage() }
            }
        },
        EnglishDescription = "A broad-winged eagle with dark flight feathers, a pale hooked beak and thick, curved talons. Its bright eyes remain fixed while its head turns in sharp, measured movements.",
        InitialLevel = 4,
        GenerationWeight = "0",
        Noise = ActorNoiseType.Sqeek
    };
}
