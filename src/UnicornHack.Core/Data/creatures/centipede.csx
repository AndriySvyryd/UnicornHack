new CreatureVariant
{
    Name = "centipede",
    Species = Species.Centipede,
    SpeciesClass = SpeciesClass.Vermin,
    InitialLevel = 2,
    ArmorClass = 4,
    MovementRate = 4,
    Weight = 50,
    Size = Size.Tiny,
    Nutrition = 25,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Concealment", "Clinginess", "AnimalBody", "Handlessness", "Carnivorism", "Oviparity" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    GenerationFrequency = Frequency.Usually
}
