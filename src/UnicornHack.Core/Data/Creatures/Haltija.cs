namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Haltija = new Creature
    {
        Name = "haltija",
        SpeciesClass = SpeciesClass.Extraplanar,
        MovementDelay = 300,
        TurningDelay = 300,
        Size = 8,
        Weight = 1000,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Walking | LocomotionType.Tunneling,
        SlotCapacity = 1,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "stone-toothed bite",
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
            }
        },
        EnglishDescription = "A slight, watchful humanoid with a lined face and bright eyes beneath a low brow. Layered, moss-colored clothing hangs loosely around its narrow shoulders and slender hands.",
        InitialLevel = 3,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Wandering
    };
}
