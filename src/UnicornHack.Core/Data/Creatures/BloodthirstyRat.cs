namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature BloodthirstyRat = new Creature
    {
        Name = "bloodthirsty rat",
        SpeciesClass = SpeciesClass.Rodent,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "bloodthirsty rat ability #1",
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
            }
        },
        InitialLevel = 2,
        GenerationWeight = "5",
        PreviousStageName = "sewer rat",
        GenerationFlags = CreatureGenerationFlags.SmallGroup,
        Noise = ActorNoiseType.Sqeek,
        Size = 2,
        Weight = 150,
        MovementDelay = 20,
        TurningDelay = 20,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Armor = 1,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
