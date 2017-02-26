new Creature
{
    Name = "killer bee",
    Species = Species.Bee,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = -1,
    MovementDelay = 66,
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
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 2 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness", "Femaleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 1,
    GenerationFlags = GenerationFlags.LargeGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Buzz
}
