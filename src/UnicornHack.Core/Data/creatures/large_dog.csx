new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Bark,
    PreviousStageName = "dog",
    Name = "large dog",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 15,
    Size = Size.Medium,
    Weight = 600,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}
