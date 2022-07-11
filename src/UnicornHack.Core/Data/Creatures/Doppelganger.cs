namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Doppelganger = new Creature
    {
        Name = "doppelganger",
        Species = Species.Doppelganger,
        SpeciesClass = SpeciesClass.ShapeChanger,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Damage = "60" } }
            },
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
                Effects = new List<Effect> { new PhysicalDamage { Damage = "10*MightModifier()" } }
            }
        },
        InitialLevel = 9,
        GenerationFlags = GenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Imitate,
        Weight = 1000,
        Perception = -5,
        Might = -6,
        Speed = -5,
        Armor = 2,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true
    };
}
