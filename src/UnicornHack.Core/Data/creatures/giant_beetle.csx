new CreatureVariant
{
    Name = "giant beetle",
    Species = Species.Beetle,
    SpeciesClass = SpeciesClass.Vermin,
    InitialLevel = 5,
    ArmorClass = 3,
    MovementRate = 6,
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 5 } } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Sometimes
}
