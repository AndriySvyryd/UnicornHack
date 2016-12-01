new CreatureVariant
{
    Name = "killer bee",
    Species = Species.Bee,
    SpeciesClass = SpeciesClass.Vermin,
    InitialLevel = 1,
    ArmorClass = -1,
    MovementRate = 18,
    Weight = 5,
    Size = Size.Tiny,
    Nutrition = 5,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness", "Femaleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFlags = GenerationFlags.LargeGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Buzz
}
