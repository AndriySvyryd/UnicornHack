namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Monkey = new Creature
    {
        Name = "monkey",
        SpeciesClass = SpeciesClass.Quadrupedal,
        MovementDelay = -34,
        TurningDelay = -34,
        Size = 2,
        Weight = 100,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
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
                Name = "raking claw",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Amount = "25*MightModifier()" } }
            },
            new Ability
            {
                Name = "snatching feint",
                Type = AbilityType.MeleeAttack,
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 300,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new Disarm() }
            }
        },
        EnglishDescription = "A small, lean primate with short brown fur, a bare muzzle and round, alert eyes. Long fingers curl at the ends of slender arms, and a flexible tail arcs behind it.",
        InitialLevel = 2,
        GenerationWeight = "0",
        Behavior = AIBehavior.Wandering,
        Noise = ActorNoiseType.Growl
    };
}
