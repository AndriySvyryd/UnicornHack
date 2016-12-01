new CreatureVariant
{
    Name = "dire wolf",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    InitialLevel = 7,
    ArmorClass = 3,
    MovementRate = 12,
    Weight = 1200,
    Size = Size.Medium,
    Nutrition = 500,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Bark
}
