new Creature
{
    Name = "snake",
    Species = Species.Snake,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 3,
    MovementRate = 15,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Concealment", "Infravision", "SerpentlikeBody", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    InitialLevel = 4,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Hiss
}