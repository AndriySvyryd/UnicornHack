new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes,
    CorpseVariantName = "",
    Name = "flaming sphere",
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
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Explosion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 14 } }
        }
    }
}
