new CreatureVariant
{
    Name = "ant queen",
    Species = Species.Ant,
    SpeciesClass = SpeciesClass.Vermin,
    InitialLevel = 9,
    MagicResistance = 20,
    MovementRate = 18,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 5 } } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Carnivorism", "Femaleness", "Oviparity" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Rarely
}
