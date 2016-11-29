new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 3,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Name = "fire ant",
    Species = Species.Ant,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 18,
    Size = Size.Tiny,
    Weight = 30,
    Nutrition = 10,
    SimpleProperties = new HashSet<string> { "Infravisibility", "AnimalBody", "Handlessness", "Carnivorism", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "Stealthiness", 3 }, { "SlimingResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new FireDamage { Damage = 5 } } }
    }
}
