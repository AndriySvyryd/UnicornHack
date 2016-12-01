new CreatureVariant
{
    Name = "floating eye",
    Species = Species.FloatingSphere,
    SpeciesClass = SpeciesClass.Aberration,
    InitialLevel = 2,
    ArmorClass = 9,
    MagicResistance = 10,
    MovementRate = 1,
    Weight = 10,
    Size = Size.Small,
    Nutrition = 10,
    Abilities = new List<Ability> { new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Paralyze { Duration = 35 } } } },
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
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Wandering
}
