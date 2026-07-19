namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature BogTroll = new Creature
    {
        Name = "bog troll",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "bog troll ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "90*MightModifier()" } }
            },
            new Ability
            {
                Name = "bog troll ability #2",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage() }
            },
            new Ability
            {
                Name = "bog troll ability #3",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "90*MightModifier()" } }
            },
            new Ability
            {
                Name = "bog troll ability #4",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "70*MightModifier()" } }
            }
        },
        InitialLevel = 11,
        GenerationWeight = "$branch == 'hell' ? 0 : 1",
        Behavior = AIBehavior.Stalking | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Grunt,
        Size = 8,
        Weight = 1200,
        MovementDelay = -15,
        TurningDelay = -15,
        PerceptionBonus = -4,
        MightBonus = -4,
        SpeedBonus = -4,
        FocusBonus = -4,
        Regeneration = 3,
        Armor = 3,
        MagicResistance = 20,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        LocomotionType = LocomotionType.Swimming,
        Infravisible = true,
        Infravision = true,
        Reanimation = true
    };
}
