new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 3,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Name = "giant ant",
    Species = Species.Ant,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 18,
    Size = Size.Tiny,
    Weight = 10,
    Nutrition = 10,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Carnivorism", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
