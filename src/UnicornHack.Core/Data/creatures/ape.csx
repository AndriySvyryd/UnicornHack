new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 6,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Growl,
    NextStageName = "carnivorous ape",
    Name = "ape",
    Species = Species.Simian,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1100,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}
