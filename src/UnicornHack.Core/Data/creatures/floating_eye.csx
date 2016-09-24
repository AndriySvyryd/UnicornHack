new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 9,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Wandering,
    Name = "floating eye",
    Species = Species.FloatingSphere,
    SpeciesClass = SpeciesClass.Aberration,
    MovementRate = 1,
    Size = Size.Small,
    Weight = 10,
    Nutrition = 10,
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
    Abilities = new List<Ability> { new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Paralyze { Duration = 35 } } } }
}
