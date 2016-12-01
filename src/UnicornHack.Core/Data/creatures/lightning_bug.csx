new CreatureVariant
{
    Name = "lightning bug",
    Species = Species.Beetle,
    SpeciesClass = SpeciesClass.Vermin,
    InitialLevel = 1,
    ArmorClass = 9,
    MovementRate = 12,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new ElectricityDamage { Damage = 1 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness", "Herbivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "ElectricityResistance", 3 } },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Buzz
}
