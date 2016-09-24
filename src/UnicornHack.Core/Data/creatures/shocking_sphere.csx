new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Sometimes,
    CorpseVariantName = "",
    Name = "shocking sphere",
    Species = Species.FloatingSphere,
    SpeciesClass = SpeciesClass.Extraplanar,
    MovementRate = 13,
    Size = Size.Small,
    Weight = 10,
    Nutrition = 10,
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
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Explosion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 14 } }
        }
    }
}
