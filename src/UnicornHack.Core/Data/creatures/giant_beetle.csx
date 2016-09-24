new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 3,
    GenerationFrequency = Frequency.Sometimes,
    Name = "giant beetle",
    Species = Species.Beetle,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 6,
    Size = Size.Tiny,
    Weight = 10,
    Nutrition = 10,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 5 } } }
    }
}
