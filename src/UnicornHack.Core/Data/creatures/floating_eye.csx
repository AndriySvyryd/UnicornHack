new Creature
{
    Name = "floating eye",
    Species = Species.FloatingSphere,
    SpeciesClass = SpeciesClass.Aberration,
    ArmorClass = 9,
    MagicResistance = 10,
    MovementRate = 1,
    Weight = 10,
    Size = Size.Small,
    Nutrition = 10,
    Abilities = new HashSet<Ability> { new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new Paralyze { Duration = 35 } } } },
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "Infravision",
        "Infravisibility",
        "NonAnimal",
        "Breathlessness",
        "Limblessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    InitialLevel = 2,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Wandering
}
