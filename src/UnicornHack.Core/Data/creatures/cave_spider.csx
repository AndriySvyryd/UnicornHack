new CreatureVariant
{
    Name = "cave spider",
    Species = Species.Spider,
    SpeciesClass = SpeciesClass.Vermin,
    InitialLevel = 1,
    ArmorClass = 3,
    MovementRate = 12,
    Weight = 50,
    Size = Size.Small,
    Nutrition = 25,
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
,
    SimpleProperties = new HashSet<string> { "Concealment", "Clinginess", "AnimalBody", "Handlessness", "Carnivorism", "Oviparity" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Usually
}
