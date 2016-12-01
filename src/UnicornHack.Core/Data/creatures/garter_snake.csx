new CreatureVariant
{
    Name = "garter snake",
    Species = Species.Snake,
    SpeciesClass = SpeciesClass.Reptile,
    InitialLevel = 1,
    ArmorClass = 8,
    MovementRate = 8,
    Weight = 50,
    Size = Size.Tiny,
    Nutrition = 25,
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
,
    SimpleProperties = new HashSet<string> { "Swimming", "Concealment", "Infravision", "SerpentlikeBody", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    GenerationFlags = GenerationFlags.LargeGroup,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Hiss
}
