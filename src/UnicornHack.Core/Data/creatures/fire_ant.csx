new CreatureVariant
{
    Name = "fire ant",
    Species = Species.Ant,
    SpeciesClass = SpeciesClass.Vermin,
    InitialLevel = 3,
    ArmorClass = 3,
    MagicResistance = 10,
    MovementRate = 18,
    Weight = 30,
    Size = Size.Tiny,
    Nutrition = 10,
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
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "AnimalBody", "Handlessness", "Carnivorism", "Asexuality", "SlimingResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "Stealthiness", 3 } },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly
}
