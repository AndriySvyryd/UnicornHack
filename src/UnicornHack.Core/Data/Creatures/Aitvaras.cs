namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Aitvaras = new Creature
    {
        Name = "aitvaras",
        SpeciesClass = SpeciesClass.Bird,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "aitvaras ability #1",
                Type = AbilityType.RangedAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnRangedAttack,
                Range = 20,
                Action = AbilityAction.Gaze,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect>
                {
                    new FireDamage { Amount = "70*FocusModifier()" },
                    new FireDamage { Duration = EffectDuration.UntilTimeout, Amount = "70*FocusModifier()" }
                }
            }
        },
        InitialLevel = 6,
        Noise = ActorNoiseType.Hiss,
        Size = 2,
        Weight = 30,
        MovementDelay = 100,
        TurningDelay = 100,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 2,
        MagicResistance = 15,
        FireResistance = 75,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
