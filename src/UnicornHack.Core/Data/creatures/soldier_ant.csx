new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 3,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Name = "soldier ant",
    Species = Species.Ant,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 18,
    Size = Size.Tiny,
    Weight = 20,
    Nutrition = 5,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Carnivorism", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Sting,
            Timeout = 5,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 5 } } }
    }
}
