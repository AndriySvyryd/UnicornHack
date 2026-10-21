namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Bat = new Creature
    {
        Name = "bat",
        SpeciesClass = SpeciesClass.Bird,
        MovementDelay = -40,
        TurningDelay = -40,
        Size = 1,
        Weight = 100,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Armor = 2,
        TorsoType = TorsoType.Avian,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        SlotCapacity = 1,
        Quietness = 250,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "darting bite",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "30*MightModifier()" } }
            },
            new Ability
            {
                Name = "echolocation pulse",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 5,
                Action = AbilityAction.Screech,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Expose() }
            }
        },
        EnglishDescription = "A small, dark-furred animal with broad ears and thin, leathery wings stretched over long finger bones. Tiny hooked feet curl beneath its body.",
        InitialLevel = 2,
        GenerationWeight = "$branch == 'outer wards' ? 2 : ($branch == 'ruined stronghold' ? 1 : 0)",
        Behavior = AIBehavior.Wandering,
        Noise = ActorNoiseType.Sqeek
    };
}
