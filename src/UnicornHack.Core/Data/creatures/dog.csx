new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Bark,
    PreviousStageName = "little dog",
    NextStageName = "large dog",
    Name = "dog",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 16,
    Size = Size.Medium,
    Weight = 400,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}
