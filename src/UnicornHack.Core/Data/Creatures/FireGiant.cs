namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature FireGiant = new Creature
    {
        Name = "fire giant",
        Species = Species.Giant,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Damage = "110" } }
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
        GenerationFlags = GenerationFlags.SmallGroup,
        Behavior = AIBehavior.GemCollector | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Boast,
        Size = 16,
        Weight = 2250,
        Perception = -5,
        Might = -6,
        Speed = -5,
        Focus = -6,
        Armor = 3,
        MagicResistance = 2,
        FireResistance = 75,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
