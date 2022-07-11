namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Barghest = new Creature
    {
        Name = "barghest",
        Species = Species.Dog,
        SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "70*MightModifier()" } }
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "50*MightModifier()" } }
            }
        },
        InitialLevel = 9,
        GenerationWeight = "2",
        Behavior = AIBehavior.AlignmentAware | AIBehavior.Mountable,
        Noise = ActorNoiseType.Bark,
        Weight = 1200,
        MovementDelay = -25,
        TurningDelay = -25,
        Perception = -5,
        Might = -6,
        Speed = -5,
        Focus = -6,
        Armor = 4,
        MagicResistance = 10,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true
    };
}
