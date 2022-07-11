namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Ghost = new Creature
    {
        Name = "ghost",
        Species = Species.Ghost,
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Touch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "10*MightModifier()" } }
            }
        },
        InitialLevel = 10,
        GenerationWeight = "0",
        GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Wandering | AIBehavior.Stalking,
        Weight = 0,
        MovementDelay = 300,
        TurningDelay = 300,
        Material = Material.Air,
        Perception = -4,
        Might = -4,
        Speed = -4,
        Armor = 7,
        MagicResistance = 7,
        ColdResistance = 75,
        VoidResistance = 75,
        SlimingImmune = true,
        StoningImmune = true,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        LocomotionType = LocomotionType.Walking | LocomotionType.Phasing,
        SlotCapacity = 0,
        Infravision = true
    };
}
