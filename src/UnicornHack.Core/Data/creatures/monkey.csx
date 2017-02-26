new Creature
{
    Name = "monkey",
    Species = Species.Simian,
    ArmorClass = 6,
    MovementDelay = 66,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Claw, Timeout = 1, Effects = new HashSet<Effect> { new StealItem { } } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Omnivorism" },
    InitialLevel = 2,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Growl
}
