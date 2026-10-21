namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Rat = new Creature
    {
        Name = "rat",
        SpeciesClass = SpeciesClass.Rodent,
        MovementDelay = 20,
        TurningDelay = 20,
        Size = 2,
        Weight = 150,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Armor = 1,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
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
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            },
            new Ability
            {
                Name = "swarming rush",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage(), new Topple() }
            }
        },
        EnglishDescription = "A low, whiskered rodent with coarse grey-brown fur, rounded ears and bright black eyes. Its pointed muzzle twitches above small paws, while a long, scaly tail drags behind it.",
        InitialLevel = 2,
        GenerationWeight = "$branch == 'outer wards' ? 2 : ($branch == 'ruined stronghold' ? 1 : 0)",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Behavior = AIBehavior.Wandering,
        Noise = ActorNoiseType.Sqeek
    };
}
