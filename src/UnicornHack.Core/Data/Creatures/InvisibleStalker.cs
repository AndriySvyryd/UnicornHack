namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature InvisibleStalker = new Creature
    {
        Name = "invisible stalker",
        SpeciesClass = SpeciesClass.Extraplanar,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "invisible stalker ability #1",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "100*MightModifier()" } }
            }
        },
        InitialLevel = 8,
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Size = 8,
        Weight = 900,
        PerceptionBonus = -5,
        MightBonus = -6,
        SpeedBonus = -5,
        FocusBonus = -6,
        Armor = 3,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.Claws,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Flying,
        Quietness = 250,
        VisibilityLevel = 0,
        Infravision = true,
        InvisibilityDetection = true
    };
}
