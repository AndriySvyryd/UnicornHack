new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Bark,
    PreviousStageName = "housecat",
    Name = "large cat",
    Species = Species.Cat,
    SpeciesClass = SpeciesClass.Feline,
    MovementRate = 15,
    Size = Size.Small,
    Weight = 250,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}
