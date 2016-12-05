new Creature
{
    Name = "cave spider",
    Species = Species.Spider,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 3,
    MovementRate = 12,
    Weight = 50,
    Size = Size.Small,
    Nutrition = 25,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Concealment", "Clinginess", "AnimalBody", "Handlessness", "Carnivorism", "Oviparity" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    InitialLevel = 1,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Usually
}
