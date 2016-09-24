new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Growl,
    PreviousStageName = "ape",
    Name = "carnivorous ape",
    Species = Species.Simian,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1250,
    Nutrition = 550,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Carnivorism" },
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
    }
}
