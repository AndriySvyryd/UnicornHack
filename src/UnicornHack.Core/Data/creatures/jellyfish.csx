new Creature
{
    Name = "jellyfish",
    Species = Species.Jellyfish,
    ArmorClass = 6,
    MovementDelay = 400,
    Weight = 80,
    Size = Size.Small,
    Nutrition = 20,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 6 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 2 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "NoInventory" },
    InitialLevel = 3,
    GenerationFrequency = Frequency.Sometimes
}
