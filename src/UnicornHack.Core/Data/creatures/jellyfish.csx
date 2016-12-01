new CreatureVariant
{
    Name = "jellyfish",
    Species = Species.Jellyfish,
    InitialLevel = 3,
    ArmorClass = 6,
    MovementRate = 3,
    Weight = 80,
    Size = Size.Small,
    Nutrition = 20,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 6 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "NoInventory" },
    GenerationFrequency = Frequency.Sometimes
}
