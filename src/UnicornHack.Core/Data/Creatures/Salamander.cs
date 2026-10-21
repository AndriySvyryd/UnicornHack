namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Salamander = new Creature
    {
        Name = "salamander",
        SpeciesClass = SpeciesClass.Reptile,
        MovementDelay = -20,
        TurningDelay = -20,
        Size = 2,
        Weight = 100,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -8,
        Armor = 2,
        FireResistance = 100,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 0,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "defensive bite",
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
        EnglishDescription = "A low, slender reptile with smooth black skin broken by vivid amber patches. Short, splayed legs carry its elongated body, and a thick tail tapers behind its rounded head.",
        InitialLevel = 5,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Wandering,
        Noise = ActorNoiseType.Hiss
    };
}
