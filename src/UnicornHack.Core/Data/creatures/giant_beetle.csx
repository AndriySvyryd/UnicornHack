new Creature
{
    Name = "giant beetle",
    Species = Species.Beetle,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 3,
    MovementRate = 6,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 5 } } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 5,
    GenerationFrequency = Frequency.Sometimes
}
