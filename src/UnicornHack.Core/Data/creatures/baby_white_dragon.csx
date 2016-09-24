new CreatureVariant
{
    InitialLevel = 12,
    ArmorClass = 2,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Roar,
    NextStageName = "white dragon",
    Name = "baby white dragon",
    Species = Species.Dragon,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 9,
    Size = Size.Large,
    Weight = 1500,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "AnimalBody", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
}
