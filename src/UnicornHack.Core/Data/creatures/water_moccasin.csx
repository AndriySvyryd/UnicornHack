new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 3,
    GenerationFlags = GenerationFlags.LargeGroup,
    Noise = ActorNoiseType.Hiss,
    Name = "water moccasin",
    Species = Species.Snake,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 15,
    Size = Size.Small,
    Weight = 150,
    Nutrition = 75,
    SimpleProperties = new HashSet<string> { "Swimming", "Concealment", "Infravision", "SerpentlikeBody", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 3 } }
        }
    }
}
