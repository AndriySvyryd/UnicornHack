new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = -4,
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Rarely,
    Noise = ActorNoiseType.Buzz,
    Name = "queen bee",
    Species = Species.Bee,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 24,
    Size = Size.Tiny,
    Weight = 5,
    Nutrition = 5,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness", "Femaleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 8 } } }
    }
}
