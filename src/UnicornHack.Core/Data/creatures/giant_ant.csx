new Creature
{
    Name = "giant ant",
    Species = Species.Ant,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 3,
    MovementDelay = 66,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Carnivorism", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    InitialLevel = 2,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly
}
