new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 4,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Bark,
    Name = "wolf",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 500,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}
