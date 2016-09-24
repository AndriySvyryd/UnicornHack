new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 9,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Buzz,
    Name = "lightning bug",
    Species = Species.Beetle,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 12,
    Size = Size.Tiny,
    Weight = 10,
    Nutrition = 10,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness", "Herbivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "ElectricityResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 1 } } }
    }
}
