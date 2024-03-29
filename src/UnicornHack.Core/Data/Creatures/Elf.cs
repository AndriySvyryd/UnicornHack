namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Elf = new Creature
    {
        Name = "elf",
        Species = Species.Elf,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Damage = "50" } }
            }
        },
        InitialLevel = 4,
        GenerationWeight = "0",
        GenerationFlags = GenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.AlignmentAware | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach,
        Weight = 800,
        Perception = -7,
        Might = -8,
        Speed = -7,
        Focus = -2,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true
    };
}
