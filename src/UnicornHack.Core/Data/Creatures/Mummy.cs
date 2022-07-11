namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Mummy = new Creature
    {
        Name = "mummy",
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "50*MightModifier()" } }
            }
        },
        InitialLevel = 6,
        Noise = ActorNoiseType.Moan,
        Weight = 1000,
        Perception = -6,
        Might = -6,
        Speed = -6,
        Focus = -2,
        Armor = 3,
        MagicResistance = 10,
        ColdResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        RespirationType = RespirationType.None,
        Infravision = true
    };
}
