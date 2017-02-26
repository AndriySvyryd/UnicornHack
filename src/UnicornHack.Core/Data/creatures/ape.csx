new Creature
{
    Name = "ape",
    Species = Species.Simian,
    ArmorClass = 6,
    MovementDelay = 100,
    Weight = 1100,
    Size = Size.Medium,
    Nutrition = 500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Omnivorism" },
    InitialLevel = 4,
    NextStageName = "carnivorous ape",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Growl
}
