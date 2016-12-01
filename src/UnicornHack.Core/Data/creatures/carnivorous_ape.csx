new CreatureVariant
{
    Name = "carnivorous ape",
    Species = Species.Simian,
    PreviousStageName = "ape",
    InitialLevel = 6,
    ArmorClass = 6,
    MovementRate = 12,
    Weight = 1250,
    Size = Size.Medium,
    Nutrition = 550,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Carnivorism" },
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Growl
}
