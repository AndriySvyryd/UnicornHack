new CreatureVariant
{
    Name = "wolf",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    InitialLevel = 5,
    ArmorClass = 4,
    MovementRate = 12,
    Weight = 500,
    Size = Size.Medium,
    Nutrition = 250,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Bark
}
