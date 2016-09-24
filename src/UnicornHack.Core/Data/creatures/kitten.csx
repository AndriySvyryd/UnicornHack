new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Mew,
    NextStageName = "housecat",
    Name = "kitten",
    Species = Species.Cat,
    SpeciesClass = SpeciesClass.Feline,
    MovementRate = 18,
    Size = Size.Small,
    Weight = 150,
    Nutrition = 100,
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
