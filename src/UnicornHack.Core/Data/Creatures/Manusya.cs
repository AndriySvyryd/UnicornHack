namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Manusya = new Creature
    {
        Name = "manusya",
        SpeciesClass = SpeciesClass.Humanoid,
        Weight = 1000,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "trained strikes",
                Activation = ActivationType.OnMeleeAttack,
                Action = AbilityAction.Modifier,
                Effects = new List<Effect> { new PhysicalDamage { Amount = "20*MightModifier()" } }
            }
        },
        EnglishDescription = "A humanlike figure with an upright stance, an expressive face and slender, five-fingered hands. Plain, loosely woven clothing falls in soft folds around its limbs.",
        InitialLevel = 1,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Noise = ActorNoiseType.Speach
    };
}
