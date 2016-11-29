new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 8,
    GenerationFlags = GenerationFlags.LargeGroup,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Hiss,
    Name = "garter snake",
    Species = Species.Snake,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 8,
    Size = Size.Tiny,
    Weight = 50,
    Nutrition = 25,
    SimpleProperties = new HashSet<string> { "Swimming", "Concealment", "Infravision", "SerpentlikeBody", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}
