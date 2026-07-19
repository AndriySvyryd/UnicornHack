namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature ElvenWarrior = new Creature
    {
        Name = "elven warrior",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "elven warrior ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "50*MightModifier()" } }
            }
        },
        Weight = 800,
        PerceptionBonus = -7,
        MightBonus = -8,
        SpeedBonus = -7,
        FocusBonus = -2,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true,
        InitialLevel = 4,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.AlignmentAware | AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach
    };
}
