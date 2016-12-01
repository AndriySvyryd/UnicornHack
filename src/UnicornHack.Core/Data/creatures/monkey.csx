new CreatureVariant
{
    Name = "monkey",
    Species = Species.Simian,
    InitialLevel = 2,
    ArmorClass = 6,
    MovementRate = 18,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Claw, Timeout = 1, Effects = new AbilityEffect[] { new StealItem() } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Omnivorism" },
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Growl
}
