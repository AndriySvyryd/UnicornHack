new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Mew,
    PreviousStageName = "kitten",
    NextStageName = "large cat",
    Name = "housecat",
    Species = Species.Cat,
    SpeciesClass = SpeciesClass.Feline,
    MovementRate = 16,
    Size = Size.Small,
    Weight = 200,
    Nutrition = 150,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}
