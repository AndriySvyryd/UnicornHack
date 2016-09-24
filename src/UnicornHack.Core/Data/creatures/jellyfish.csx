new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Sometimes,
    Name = "jellyfish",
    Species = Species.Jellyfish,
    MovementRate = 3,
    Size = Size.Small,
    Weight = 80,
    Nutrition = 20,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "NoInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 6 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
}
