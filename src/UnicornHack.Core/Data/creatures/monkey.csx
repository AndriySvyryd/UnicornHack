new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Growl,
    Name = "monkey",
    Species = Species.Simian,
    MovementRate = 18,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.Targetted, Action = AbilityAction.Claw, Timeout = 1, Effects = new AbilityEffect[] { new StealItem() } }
    }
}
