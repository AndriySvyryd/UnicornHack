new Creature
{
    Name = "queen bee",
    Species = Species.Bee,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = -4,
    MovementDelay = 50,
    Weight = 5,
    Size = Size.Tiny,
    Nutrition = 5,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 8 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness", "Femaleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 9,
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Rarely,
    Noise = ActorNoiseType.Buzz
}
