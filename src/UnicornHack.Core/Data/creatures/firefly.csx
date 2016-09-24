new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 9,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Buzz,
    Name = "firefly",
    Species = Species.Beetle,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 12,
    Size = Size.Tiny,
    Weight = 10,
    Nutrition = 10,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravisibility", "AnimalBody", "Handlessness", "Herbivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new FireDamage { Damage = 1 } } }
    }
}
