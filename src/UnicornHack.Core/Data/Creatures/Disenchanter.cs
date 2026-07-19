namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Disenchanter = new Creature
    {
        Name = "disenchanter",
        SpeciesClass = SpeciesClass.MagicalBeast,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "disenchanter ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new DrainEnergy(), new Overcharge { DurationAmount = "1000*FocusModifier()", Amount = "30*FocusModifier()" } }
            },
            new Ability
            {
                Name = "disenchanter ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.OnMeleeHit,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.UnblockableAttack,
                Accuracy = "10+PerceptionModifier()",
                Effects = new List<Effect> { new DrainEnergy { Amount = "5*FocusModifier()" } }
            }
        },
        InitialLevel = 12,
        GenerationWeight = "2",
        Noise = ActorNoiseType.Growl,
        Weight = 750,
        PerceptionBonus = -3,
        MightBonus = -4,
        SpeedBonus = -3,
        FocusBonus = -4,
        Armor = 10,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true,
        Infravision = true
    };
}
