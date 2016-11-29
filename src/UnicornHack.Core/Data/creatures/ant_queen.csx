new CreatureVariant
{
    InitialLevel = 9,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Rarely,
    Name = "ant queen",
    Species = Species.Ant,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 18,
    Size = Size.Tiny,
    Weight = 10,
    Nutrition = 10,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Carnivorism", "Femaleness", "Oviparity" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
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
}
