new Creature
{
    Name = "xan",
    Species = Species.Xan,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = -2,
    MagicResistance = 20,
    MovementDelay = 66,
    Weight = 1,
    Size = Size.Tiny,
    Nutrition = 1,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Sting, Timeout = 1, Effects = new HashSet<Effect> { new Cripple { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 5 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "AnimalBody", "Handlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 7,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Buzz
}
