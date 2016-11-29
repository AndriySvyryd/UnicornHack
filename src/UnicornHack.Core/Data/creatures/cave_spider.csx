new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 3,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Usually,
    Name = "cave spider",
    Species = Species.Spider,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 50,
    Nutrition = 25,
    SimpleProperties = new HashSet<string> { "Concealment", "Clinginess", "AnimalBody", "Handlessness", "Carnivorism", "Oviparity" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
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
}
