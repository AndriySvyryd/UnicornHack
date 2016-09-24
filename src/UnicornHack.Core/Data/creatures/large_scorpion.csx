new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 3,
    GenerationFrequency = Frequency.Commonly,
    Name = "large scorpion",
    Species = Species.Scorpion,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 15,
    Size = Size.Small,
    Weight = 150,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Concealment", "AnimalBody", "Handlessness", "Carnivorism", "Oviparity" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
}
