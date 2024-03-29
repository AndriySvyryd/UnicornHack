namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Monkey = new Creature
    {
        Name = "monkey",
        Species = Species.Simian,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Bite,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20*MightModifier()" } }
            },
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Claw,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "25*MightModifier()" } }
            }
        },
        InitialLevel = 2,
        GenerationWeight = "2",
        Noise = ActorNoiseType.Growl,
        Size = 2,
        Weight = 100,
        MovementDelay = -34,
        TurningDelay = -34,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -8,
        Armor = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true
    };
}
