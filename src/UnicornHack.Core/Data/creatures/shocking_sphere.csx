new Creature
{
    Name = "shocking sphere",
    Species = Species.FloatingSphere,
    SpeciesClass = SpeciesClass.Extraplanar,
    ArmorClass = 4,
    MagicResistance = 10,
    MovementDelay = 92,
    Weight = 10,
    Size = Size.Small,
    Nutrition = 10,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Explosion,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ElectricityDamage { Damage = 14 } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "NonAnimal",
        "Breathlessness",
        "Limblessness",
        "Headlessness",
        "Mindlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "ElectricityResistance", 3 } },
    InitialLevel = 6,
    CorpseName = "",
    GenerationFrequency = Frequency.Sometimes
}
