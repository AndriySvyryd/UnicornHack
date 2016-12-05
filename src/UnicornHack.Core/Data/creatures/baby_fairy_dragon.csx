new Creature
{
    Name = "baby fairy dragon",
    Species = Species.Dragon,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 2,
    MagicResistance = 10,
    MovementRate = 9,
    Weight = 1500,
    Size = Size.Large,
    Nutrition = 500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Invisibility", "Infravision", "AnimalBody", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "ThickHide", 3 } },
    InitialLevel = 12,
    NextStageName = "fairy dragon",
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Roar
}
