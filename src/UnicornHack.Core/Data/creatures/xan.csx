new CreatureVariant
{
    Name = "xan",
    Species = Species.Xan,
    SpeciesClass = SpeciesClass.Vermin,
    InitialLevel = 7,
    ArmorClass = -2,
    MagicResistance = 20,
    MovementRate = 18,
    Weight = 1,
    Size = Size.Tiny,
    Nutrition = 1,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Sting, Timeout = 1, Effects = new AbilityEffect[] { new Cripple { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 5 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Buzz
}
