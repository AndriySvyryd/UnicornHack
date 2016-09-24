new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = -1,
    GenerationFlags = GenerationFlags.LargeGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Buzz,
    Name = "killer bee",
    Species = Species.Bee,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 18,
    Size = Size.Tiny,
    Weight = 5,
    Nutrition = 5,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness", "Femaleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
}
