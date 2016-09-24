new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 2,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Hiss,
    Name = "pit viper",
    Species = Species.Snake,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 15,
    Size = Size.Medium,
    Weight = 100,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Swimming", "Concealment", "Infravision", "SerpentlikeBody", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 5 } }
        }
    }
}
