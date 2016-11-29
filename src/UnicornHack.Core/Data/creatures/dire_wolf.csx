new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 3,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Bark,
    Name = "dire wolf",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1200,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
}
