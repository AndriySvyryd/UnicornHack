namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Gnome = new Creature
    {
        Name = "gnome",
        Species = Species.Gnome,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Damage = "30" } }
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
        InitialLevel = 1,
        GenerationWeight = "0",
        GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
        Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach,
        Size = 2,
        Weight = 650,
        MovementDelay = 100,
        TurningDelay = 100,
        Perception = -9,
        Might = -8,
        Speed = -7,
        Focus = -5,
        MagicResistance = 2,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
