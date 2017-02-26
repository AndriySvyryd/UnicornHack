new Creature
{
    Name = "water moccasin",
    Species = Species.Snake,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 3,
    MovementDelay = 80,
    Weight = 150,
    Size = Size.Small,
    Nutrition = 75,
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
    GenerationFlags = GenerationFlags.LargeGroup,
    Noise = ActorNoiseType.Hiss
}
