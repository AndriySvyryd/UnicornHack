new Creature
{
    Name = "flaming sphere",
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
            Effects = new HashSet<Effect> { new FireDamage { Damage = 14 } }
        }
    }
,
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
    InitialLevel = 6,
    CorpseName = "",
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Sometimes
}
