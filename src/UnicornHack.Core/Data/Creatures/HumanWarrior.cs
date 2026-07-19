namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature HumanWarrior = new Creature
    {
        Name = "human warrior",
        SpeciesClass = SpeciesClass.Humanoid,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "human warrior ability #1",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            }
        },
        Weight = 1000,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        InitialLevel = 1,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach
    };
}
