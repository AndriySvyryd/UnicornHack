new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Rarely,
    Noise = ActorNoiseType.Growl,
    Name = "sasquatch",
    Species = Species.Simian,
    MovementRate = 15,
    Size = Size.Large,
    Weight = 1550,
    Nutrition = 650,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Omnivorism" },
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
    }
}
