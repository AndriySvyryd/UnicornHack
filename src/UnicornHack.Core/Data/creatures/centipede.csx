new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 4,
    GenerationFrequency = Frequency.Usually,
    Name = "centipede",
    Species = Species.Centipede,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 4,
    Size = Size.Tiny,
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
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
}
