namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Zombie = new Creature
    {
        Name = "zombie",
        Species = Species.Human,
        SpeciesClass = SpeciesClass.Undead,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.Targeted,
                Trigger = ActivationType.OnMeleeAttack,
                Range = 1,
                Action = AbilityAction.Punch,
                SuccessCondition = AbilitySuccessCondition.NormalAttack,
                Accuracy = "5+PerceptionModifier()",
                Cooldown = 100,
                Delay = "100*SpeedModifier()",
                Effects = new List<Effect> { new PhysicalDamage { Damage = "40*MightModifier()" } }
            }
        },
        InitialLevel = 4,
        GenerationWeight = "4",
        GenerationFlags = GenerationFlags.SmallGroup,
        Behavior = AIBehavior.Stalking,
        Noise = ActorNoiseType.Moan,
        Weight = 1000,
        Perception = -7,
        Might = -8,
        Speed = -7,
        Focus = -2,
        ColdResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true,
        Mindless = true
    };
}
