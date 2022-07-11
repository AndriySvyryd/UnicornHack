namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GoreMole = new Creature
    {
        Name = "gore mole",
        Species = Species.Mole,
        SpeciesClass = SpeciesClass.Rodent,
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "30*MightModifier()" } }
            }
        },
        InitialLevel = 3,
        GenerationWeight = "3",
        Behavior = AIBehavior.GoldCollector | AIBehavior.GemCollector,
        Size = 2,
        Weight = 100,
        MovementDelay = 300,
        TurningDelay = 300,
        Perception = -8,
        Might = -8,
        Speed = -8,
        Focus = -8,
        MagicResistance = 10,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        LocomotionType = LocomotionType.Walking | LocomotionType.Tunneling,
        SlotCapacity = 1,
        Infravisible = true
    };
}
