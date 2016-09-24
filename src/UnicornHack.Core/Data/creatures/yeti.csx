new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Growl,
    Name = "yeti",
    Species = Species.Simian,
    MovementRate = 15,
    Size = Size.Large,
    Weight = 1600,
    Nutrition = 700,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
