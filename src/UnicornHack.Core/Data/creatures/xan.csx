new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = -2,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Buzz,
    Name = "xan",
    Species = Species.Xan,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 18,
    Size = Size.Tiny,
    Weight = 1,
    Nutrition = 1,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.Targetted, Action = AbilityAction.Sting, Timeout = 1, Effects = new AbilityEffect[] { new Cripple { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 5 } } }
    }
}
