new Creature
{
    Name = "lightning bug",
    Species = Species.Beetle,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 9,
    MovementDelay = 100,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ElectricityDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new ElectricityDamage { Damage = 1 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness", "Herbivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "ElectricityResistance", 3 } },
    InitialLevel = 1,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Buzz
}
