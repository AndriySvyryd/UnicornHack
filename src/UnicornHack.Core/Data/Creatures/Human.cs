namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Human = new Creature
    {
        Name = "human",
        Species = Species.Human,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Damage = "20" } }
            }
        },
        InitialLevel = 1,
        GenerationWeight = "0",
        GenerationFlags = GenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.WeaponCollector,
        Noise = ActorNoiseType.Speach,
        Weight = 1000,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true
    };
}
